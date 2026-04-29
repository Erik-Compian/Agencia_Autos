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
        // 1. LISTADO CON BÚSQUEDA (Cumple Punto 6 y 9)
        public IActionResult Index(string buscar)
        {
            List<Servicio> lista = new List<Servicio>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();

            string query = @"SELECT s.*, v.Placa, c.Nombre as NomCli, c.Apellido as ApeCli,
                                    ps.Fecha_Prog as Fecha_Proximo_Servicio
                             FROM servicio s
                             INNER JOIN vehiculo v ON s.Id_Vehiculo = v.Id_Vehiculo
                             INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente
                             LEFT JOIN proximo_servicio ps ON s.Folio = ps.Folio";

            if (!string.IsNullOrEmpty(buscar))
                query += " WHERE s.Folio LIKE @b OR c.Nombre LIKE @b OR c.Apellido LIKE @b OR v.Placa LIKE @b";

            query += " ORDER BY s.Fecha_Ingreso DESC";

            using (var cmd = new MySqlCommand(query, conexion))
            {
                if (!string.IsNullOrEmpty(buscar)) cmd.Parameters.AddWithValue("@b", "%" + buscar + "%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Servicio
                        {
                            Folio = Convert.ToInt32(reader["Folio"]),
                            FechaIngreso = Convert.ToDateTime(reader["Fecha_Ingreso"]),
                            FechaProximoServicio = reader["Fecha_Proximo_Servicio"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Fecha_Proximo_Servicio"]),
                            Estatus = Enum.Parse<EstatusServicio>(reader["Estatus"].ToString().Replace(" ", ""), true),
                            Descripcion = reader["Descripcion"].ToString(),
                            QuienEntrego = reader["Quien_Entrego"].ToString(),
                            VehiculoAtendido = new Vehiculo
                            {
                                Placa = reader["Placa"].ToString(),
                                Dueño = new Cliente
                                {
                                    Nombre = reader["NomCli"].ToString(),
                                    Apellido = reader["ApeCli"].ToString()
                                }
                            }
                        });
                    }
                }
            }
            ViewBag.BusquedaActual = buscar;
            return View(lista);
        }

        // 2. RECEPCIÓN (GET/POST)
        public IActionResult Recepcion()
        {
            ViewBag.Vehiculos = ObtenerListaVehiculos();
            return View();
        }

        [HttpPost]
        public IActionResult Recepcion(int idVehiculo, string tipo, string quienEntrego)
        {
            try
            {
                ICreadorServicio fabrica = (tipo == "Preventivo")
                    ? new CreadorServicioPreventivo()
                    : (ICreadorServicio)new CreadorServicioCorrectivo();

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

        // 3. DETALLES
        public IActionResult Detalles(int id)
        {
            Servicio servicio = null;
            var conexion = ConexionBD.Instancia.ObtenerConexion();

            string query = @"SELECT s.*, v.Placa, v.Color, v.Km_Actual,
                                    c.Nombre as NomCli, c.Apellido as ApeCli, c.Telefono,
                                    ps.Fecha_Prog as Fecha_Proximo_Servicio
                             FROM servicio s
                             INNER JOIN vehiculo v ON s.Id_Vehiculo = v.Id_Vehiculo
                             INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente
                             LEFT JOIN proximo_servicio ps ON s.Folio = ps.Folio
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
                            FechaProximoServicio = reader["Fecha_Proximo_Servicio"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["Fecha_Proximo_Servicio"]),
                            Estatus = Enum.Parse<EstatusServicio>(reader["Estatus"].ToString().Replace(" ", ""), true),
                            Descripcion = reader["Descripcion"].ToString(),
                            QuienEntrego = reader["Quien_Entrego"].ToString(),
                            VehiculoAtendido = new Vehiculo
                            {
                                Placa = reader["Placa"].ToString(),
                                Color = reader["Color"].ToString(),
                                KmActual = Convert.ToInt32(reader["Km_Actual"]),
                                Dueño = new Cliente
                                {
                                    Nombre = reader["NomCli"].ToString(),
                                    Apellido = reader["ApeCli"].ToString(),
                                    Telefono = reader["Telefono"].ToString()
                                }
                            }
                        };
                    }
                }
            }

            if (servicio == null) return NotFound();

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

            ViewBag.CatalogoRefacciones = ObtenerListaRefacciones();
            return View(servicio);
        }

        // 4. GESTIÓN DE REFACCIONES E INVENTARIO
        [HttpPost]
        public IActionResult AgregarRefaccion(int folio, int idRefaccion, int cantidad)
        {
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            decimal precioUnitario = 0;

            using (var cp = new MySqlCommand("SELECT Precio FROM refaccion WHERE Id_Refaccion=@id", conexion))
            {
                cp.Parameters.AddWithValue("@id", idRefaccion);
                precioUnitario = Convert.ToDecimal(cp.ExecuteScalar());
            }

            using (var cmd = new MySqlCommand("INSERT INTO servicio_refaccion (Folio, Id_Refaccion, Cantidad, Precio_Aplicado) VALUES (@f, @r, @c, @p)", conexion))
            {
                cmd.Parameters.AddWithValue("@f", folio);
                cmd.Parameters.AddWithValue("@r", idRefaccion);
                cmd.Parameters.AddWithValue("@c", cantidad);
                cmd.Parameters.AddWithValue("@p", precioUnitario * cantidad);
                cmd.ExecuteNonQuery();
            }

            using (var cmdStock = new MySqlCommand("UPDATE refaccion SET Stock = Stock - @cant WHERE Id_Refaccion = @idR", conexion))
            {
                cmdStock.Parameters.AddWithValue("@cant", cantidad);
                cmdStock.Parameters.AddWithValue("@idR", idRefaccion);
                cmdStock.ExecuteNonQuery();
            }

            return RedirectToAction("Detalles", new { id = folio });
        }

        // 5. COMPROBANTE DE DATOS (Punto 6)
        public IActionResult Ticket(int id)
        {
            return Detalles(id);
        }

        // 6. DASHBOARD CON FILTROS (Punto 10)
        public IActionResult Dashboard(DateTime? fecha)
        {
            var conteos = new Dictionary<string, int>
            {
                { "EnEspera", 0 },
                { "EnProceso", 0 },
                { "Finalizado", 0 }
            };

            var conexion = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT Estatus, COUNT(*) as Total FROM servicio";
            if (fecha.HasValue) query += " WHERE DATE(Fecha_Ingreso) = @fecha";
            query += " GROUP BY Estatus";

            using (var cmd = new MySqlCommand(query, conexion))
            {
                if (fecha.HasValue) cmd.Parameters.AddWithValue("@fecha", fecha.Value.ToString("yyyy-MM-dd"));

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string estatusDB = reader["Estatus"].ToString().Replace(" ", "").ToLower();
                        var llaveCorrecta = conteos.Keys.FirstOrDefault(k => k.ToLower() == estatusDB);
                        if (llaveCorrecta != null)
                            conteos[llaveCorrecta] = Convert.ToInt32(reader["Total"]);
                    }
                }
            }

            ViewBag.FechaFiltro = fecha?.ToString("yyyy-MM-dd");
            return View("~/Views/Servicio/Dashboard.cshtml", conteos);
        }

        // 7. REGISTRO PRÓXIMO SERVICIO (Punto 9)
        [HttpPost]
        public IActionResult GuardarProximoServicio(int folio, DateTime fecha)
        {
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            int existe = 0;

            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM proximo_servicio WHERE Folio = @fol", conexion))
            {
                cmd.Parameters.AddWithValue("@fol", folio);
                existe = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (existe > 0)
            {
                using (var cmd = new MySqlCommand("UPDATE proximo_servicio SET Fecha_Prog = @fec WHERE Folio = @fol", conexion))
                {
                    cmd.Parameters.AddWithValue("@fec", fecha);
                    cmd.Parameters.AddWithValue("@fol", folio);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cmd = new MySqlCommand("INSERT INTO proximo_servicio (Folio, Fecha_Prog) VALUES (@fol, @fec)", conexion))
                {
                    cmd.Parameters.AddWithValue("@fol", folio);
                    cmd.Parameters.AddWithValue("@fec", fecha);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Detalles", new { id = folio });
        }

        // 8. AGENDA DE PRÓXIMOS SERVICIOS (Punto 9)
        public IActionResult Agenda()
        {
            List<Servicio> agenda = new List<Servicio>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();

            string query = @"SELECT s.Folio, ps.Fecha_Prog as Fecha_Proximo_Servicio,
                                    v.Placa, c.Nombre as NomCli, c.Apellido as ApeCli, c.Telefono
                             FROM servicio s
                             INNER JOIN proximo_servicio ps ON s.Folio = ps.Folio
                             INNER JOIN vehiculo v ON s.Id_Vehiculo = v.Id_Vehiculo
                             INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente
                             ORDER BY ps.Fecha_Prog ASC";

            using (var cmd = new MySqlCommand(query, conexion))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    agenda.Add(new Servicio
                    {
                        Folio = Convert.ToInt32(reader["Folio"]),
                        FechaProximoServicio = Convert.ToDateTime(reader["Fecha_Proximo_Servicio"]),
                        VehiculoAtendido = new Vehiculo
                        {
                            Placa = reader["Placa"].ToString(),
                            Dueño = new Cliente
                            {
                                Nombre = reader["NomCli"].ToString(),
                                Apellido = reader["ApeCli"].ToString(),
                                Telefono = reader["Telefono"].ToString()
                            }
                        }
                    });
                }
            }

            return View(agenda);
        }

        // 9. ACTUALIZAR ESTATUS
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

        // --- MÉTODOS AUXILIARES ---
        private List<SelectListItem> ObtenerListaVehiculos()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            using (var cmd = new MySqlCommand("SELECT Id_Vehiculo, Placa FROM vehiculo", conexion))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    lista.Add(new SelectListItem { Value = r["Id_Vehiculo"].ToString(), Text = r["Placa"].ToString() });
            }
            return lista;
        }

        private List<SelectListItem> ObtenerListaRefacciones()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            using (var cmd = new MySqlCommand("SELECT Id_Refaccion, Codigo, Nombre, Precio, Stock FROM refaccion WHERE Stock > 0", conexion))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    lista.Add(new SelectListItem
                    {
                        Value = r["Id_Refaccion"].ToString(),
                        Text = $"{r["Codigo"]} - {r["Nombre"]} (${r["Precio"]}) | Disp: {r["Stock"]}"
                    });
            }
            return lista;
        }
    }
}