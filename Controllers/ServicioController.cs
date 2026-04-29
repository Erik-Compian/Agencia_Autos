using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AgenciaMVC1.Models;
using AgenciaMVC1.Patterns;
using AgenciaMVC1.Data;
using AgenciaMVC1.Filters;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AgenciaMVC1.Controllers
{
    [ValidarSesion]
    public class ServicioController : Controller
    {
        // 1. LISTADO PRINCIPAL (Index)
        public IActionResult Index()
        {
            List<Servicio> lista = new List<Servicio>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            string query = @"SELECT s.*, v.Placa, c.Nombre as NomCli, c.Apellido as ApeCli
                             FROM servicio s
                             INNER JOIN vehiculo v ON s.Id_Vehiculo = v.Id_Vehiculo
                             INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente
                             ORDER BY s.Fecha_Ingreso DESC";

            using (var cmd = new MySqlCommand(query, conexion))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Servicio
                    {
                        Folio = Convert.ToInt32(reader["Folio"]),
                        FechaIngreso = Convert.ToDateTime(reader["Fecha_Ingreso"]),
                        Estatus = Enum.Parse<EstatusServicio>(reader["Estatus"].ToString().Replace(" ", ""), true),
                        Descripcion = reader["Descripcion"].ToString(),
                        QuienEntrego = reader["Quien_Entrego"].ToString(),
                        VehiculoAtendido = new Vehiculo
                        {
                            Placa = reader["Placa"].ToString(),
                            Dueño = new Cliente { Nombre = reader["NomCli"].ToString(), Apellido = reader["ApeCli"].ToString() }
                        }
                    });
                }
            }
            return View(lista);
        }

        // 2. RECEPCIÓN (GET): Formulario inicial
        public IActionResult Recepcion()
        {
            ViewBag.Vehiculos = ObtenerListaVehiculos();
            return View();
        }

        // 3. RECEPCIÓN (POST): Proceso con Factory Method
        [HttpPost]
        public IActionResult Recepcion(int idVehiculo, string tipo, string quienEntrego)
        {
            try
            {
                ICreadorServicio fabrica = (tipo == "Preventivo") ? new CreadorServicioPreventivo() : new CreadorServicioCorrectivo();
                int idAdmin = HttpContext.Session.GetInt32("AdminId") ?? 0;
                Servicio nuevo = fabrica.CrearServicio(new Vehiculo { IdVehiculo = idVehiculo }, idAdmin, quienEntrego);

                var conexion = ConexionBD.Instancia.ObtenerConexion();
                string sql = "INSERT INTO servicio (Id_Vehiculo, Id_Admin, Quien_Entrego, Fecha_Ingreso, Estatus, Descripcion) VALUES (@idV, @idA, @ent, @fec, @est, @des)";
                using (var cmd = new MySqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@idV", nuevo.IdVehiculo);
                    cmd.Parameters.AddWithValue("@idA", nuevo.IdAdmin);
                    cmd.Parameters.AddWithValue("@ent", nuevo.QuienEntrego);
                    cmd.Parameters.AddWithValue("@fec", nuevo.FechaIngreso);
                    cmd.Parameters.AddWithValue("@est", nuevo.Estatus.ToString());
                    cmd.Parameters.AddWithValue("@des", nuevo.Descripcion);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.Vehiculos = ObtenerListaVehiculos();
                return View();
            }
        }

        // 4. DETALLES: Carga info completa + Refacciones (Muchos a Muchos)
        public IActionResult Detalles(int id)
        {
            Servicio servicio = null;
            var conexion = ConexionBD.Instancia.ObtenerConexion();

            // Consulta principal
            string query = @"SELECT s.*, v.Placa, v.Color, v.Km_Actual, c.Nombre as NomCli, c.Apellido as ApeCli, c.Telefono
                             FROM servicio s 
                             INNER JOIN vehiculo v ON s.Id_Vehiculo = v.Id_Vehiculo
                             INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente 
                             WHERE s.Folio = @id";

            using (var cmd = new MySqlCommand(query, conexion))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        servicio = new Servicio
                        {
                            Folio = Convert.ToInt32(reader["Folio"]),
                            FechaIngreso = Convert.ToDateTime(reader["Fecha_Ingreso"]),
                            Estatus = Enum.Parse<EstatusServicio>(reader["Estatus"].ToString().Replace(" ", ""), true),
                            Descripcion = reader["Descripcion"].ToString(),
                            QuienEntrego = reader["Quien_Entrego"].ToString(),
                            VehiculoAtendido = new Vehiculo
                            {
                                Placa = reader["Placa"].ToString(),
                                Color = reader["Color"].ToString(),
                                KmActual = Convert.ToInt32(reader["Km_Actual"]),
                                Dueño = new Cliente { Nombre = reader["NomCli"].ToString(), Apellido = reader["ApeCli"].ToString(), Telefono = reader["Telefono"].ToString() }
                            }
                        };
                    }
                }
            }

            if (servicio == null) return NotFound();

            // Cargar lista de refacciones ya agregadas
            string queryRef = @"SELECT sr.*, r.Nombre, r.Codigo 
                                FROM servicio_refaccion sr 
                                INNER JOIN refaccion r ON sr.Id_Refaccion = r.Id_Refaccion 
                                WHERE sr.Folio = @id";

            using (var cmdRef = new MySqlCommand(queryRef, conexion))
            {
                cmdRef.Parameters.AddWithValue("@id", id);
                using (var reader2 = cmdRef.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        servicio.Refacciones.Add(new ServicioRefaccion
                        {
                            Folio = id,
                            IdRefaccion = Convert.ToInt32(reader2["Id_Refaccion"]),
                            Cantidad = Convert.ToInt32(reader2["Cantidad"]),
                            PrecioAplicado = Convert.ToDecimal(reader2["Precio_Aplicado"]),
                            RefaccionUtilizada = new Refaccion
                            {
                                Nombre = reader2["Nombre"].ToString(),
                                Codigo = reader2["Codigo"].ToString()
                            }
                        });
                    }
                }
            }

            // Llenar el menú desplegable de refacciones disponibles
            ViewBag.CatalogoRefacciones = ObtenerListaRefacciones();
            return View(servicio);
        }

        // 5. AGREGAR REFACCIÓN A LA ORDEN
        [HttpPost]
        public IActionResult AgregarRefaccion(int folio, int idRefaccion, int cantidad)
        {
            try
            {
                var conexion = ConexionBD.Instancia.ObtenerConexion();
                decimal precioUnitario = 0;

                // 1. Obtener el precio actual de la refacción
                using (var cp = new MySqlCommand("SELECT Precio FROM refaccion WHERE Id_Refaccion=@id", conexion))
                {
                    cp.Parameters.AddWithValue("@id", idRefaccion);
                    precioUnitario = Convert.ToDecimal(cp.ExecuteScalar());
                }

                // 2. Insertar en la tabla intermedia (ServicioRefaccion)
                string sqlInsert = "INSERT INTO servicio_refaccion (Folio, Id_Refaccion, Cantidad, Precio_Aplicado) VALUES (@f, @r, @c, @p)";
                using (var cmd = new MySqlCommand(sqlInsert, conexion))
                {
                    cmd.Parameters.AddWithValue("@f", folio);
                    cmd.Parameters.AddWithValue("@r", idRefaccion);
                    cmd.Parameters.AddWithValue("@c", cantidad);
                    cmd.Parameters.AddWithValue("@p", precioUnitario * cantidad);
                    cmd.ExecuteNonQuery();
                }

                // 3. ACTUALIZAR EL STOCK (Restar la cantidad usada)
                // Esta es la parte que faltaba para que el inventario se mueva
                string sqlUpdateStock = "UPDATE refaccion SET Stock = Stock - @cant WHERE Id_Refaccion = @idR";
                using (var cmdStock = new MySqlCommand(sqlUpdateStock, conexion))
                {
                    cmdStock.Parameters.AddWithValue("@cant", cantidad);
                    cmdStock.Parameters.AddWithValue("@idR", idRefaccion);
                    cmdStock.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al descontar del inventario: " + ex.Message;
            }

            return RedirectToAction("Detalles", new { id = folio });
        }

        // 6. ACTUALIZAR ESTATUS
        [HttpPost]
        public IActionResult ActualizarEstatus(int folio, string nuevoEstatus)
        {
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            using (var cmd = new MySqlCommand("UPDATE servicio SET Estatus=@est WHERE Folio=@fol", conexion))
            {
                cmd.Parameters.AddWithValue("@est", nuevoEstatus);
                cmd.Parameters.AddWithValue("@fol", folio);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Detalles", new { id = folio });
        }

        [HttpPost]
        public IActionResult EliminarRefaccion(int folio, int idRefaccion)
        {
            var conexion = ConexionBD.Instancia.ObtenerConexion();

            // 1. Antes de borrar, averiguamos cuántas piezas había para devolverlas al stock
            int cantidadADevolver = 0;
            string sqlCant = "SELECT Cantidad FROM servicio_refaccion WHERE Folio=@f AND Id_Refaccion=@r";
            using (var cmdC = new MySqlCommand(sqlCant, conexion))
            {
                cmdC.Parameters.AddWithValue("@f", folio);
                cmdC.Parameters.AddWithValue("@r", idRefaccion);
                cantidadADevolver = Convert.ToInt32(cmdC.ExecuteScalar());
            }

            // 2. Devolvemos las piezas al inventario
            string sqlUpdate = "UPDATE refaccion SET Stock = Stock + @cant WHERE Id_Refaccion = @idR";
            using (var cmdU = new MySqlCommand(sqlUpdate, conexion))
            {
                cmdU.Parameters.AddWithValue("@cant", cantidadADevolver);
                cmdU.Parameters.AddWithValue("@idR", idRefaccion);
                cmdU.ExecuteNonQuery();
            }

            // 3. Ahora sí, borramos el registro de la orden
            string sqlDel = "DELETE FROM servicio_refaccion WHERE Folio = @folio AND Id_Refaccion = @idR";
            using (var cmdD = new MySqlCommand(sqlDel, conexion))
            {
                cmdD.Parameters.AddWithValue("@folio", folio);
                cmdD.Parameters.AddWithValue("@idR", idRefaccion);
                cmdD.ExecuteNonQuery();
            }

            return RedirectToAction("Detalles", new { id = folio });
        }

        // --- MÉTODOS AUXILIARES PARA DROPDOWNS ---

        private List<SelectListItem> ObtenerListaVehiculos()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            using (var cmd = new MySqlCommand("SELECT Id_Vehiculo, Placa FROM vehiculo", conexion))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = r["Id_Vehiculo"].ToString(),
                        Text = r["Placa"].ToString()
                    });
                }
            }
            return lista;
        }

        private List<SelectListItem> ObtenerListaRefacciones()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            // Asegúrate de que las columnas existan en tu tabla MySQL
            string sql = "SELECT Id_Refaccion, Codigo, Nombre, Precio, Stock FROM refaccion WHERE Stock > 0";

            using (var cmd = new MySqlCommand(sql, conexion))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    lista.Add(new SelectListItem
                    {
                        Value = r["Id_Refaccion"].ToString(),
                        Text = $"{r["Codigo"]} - {r["Nombre"]} (${r["Precio"]}) | Disp: {r["Stock"]}"
                    });
                }
            }
            return lista;
        }
        // GET: Servicio/Ticket/id
        public IActionResult Ticket(int id)
        {
            Servicio servicio = null;
            var conexion = ConexionBD.Instancia.ObtenerConexion();

            // 1. Obtener datos del servicio, vehículo y cliente
            string query = @"SELECT s.*, v.Placa, v.Color, v.Km_Actual, 
                            c.Nombre as NomCli, c.Apellido as ApeCli
                     FROM servicio s
                     INNER JOIN vehiculo v ON s.Id_Vehiculo = v.Id_Vehiculo
                     INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente
                     WHERE s.Folio = @id";

            using (var cmd = new MySqlCommand(query, conexion))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        servicio = new Servicio
                        {
                            Folio = Convert.ToInt32(reader["Folio"]),
                            FechaIngreso = Convert.ToDateTime(reader["Fecha_Ingreso"]),
                            Estatus = Enum.Parse<EstatusServicio>(reader["Estatus"].ToString().Replace(" ", ""), true),
                            Descripcion = reader["Descripcion"].ToString(),
                            QuienEntrego = reader["Quien_Entrego"].ToString(),
                            VehiculoAtendido = new Vehiculo
                            {
                                Placa = reader["Placa"].ToString(),
                                Dueño = new Cliente { Nombre = reader["NomCli"].ToString(), Apellido = reader["ApeCli"].ToString() }
                            }
                        };
                    }
                }
            }

            if (servicio == null) return NotFound();

            
            // 2. Cargar las refacciones asociadas al servicio (Ruta: Controllers/ServicioController.cs)
            string qRef = @"SELECT sr.*, r.Nombre FROM servicio_refaccion sr 
                INNER JOIN refaccion r ON sr.Id_Refaccion = r.Id_Refaccion WHERE sr.Folio = @id";

            using (var cmd2 = new MySqlCommand(qRef, conexion))
            {
                // LÍNEA REQUERIDA PARA SOLUCIONAR EL ERROR:
                cmd2.Parameters.AddWithValue("@id", id);

                using (var r2 = cmd2.ExecuteReader())
                {
                    while (r2.Read())
                    {
                        servicio.Refacciones.Add(new ServicioRefaccion
                        {
                            PrecioAplicado = Convert.ToDecimal(r2["Precio_Aplicado"]),
                            Cantidad = Convert.ToInt32(r2["Cantidad"]),
                            RefaccionUtilizada = new Refaccion { Nombre = r2["Nombre"].ToString() }
                        });
                    }
                }
            }

            return View(servicio);
        }
    }

}