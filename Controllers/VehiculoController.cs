using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AgenciaMVC1.Data;
using AgenciaMVC1.Models;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using AgenciaMVC1.Filters;

namespace AgenciaMVC1.Controllers
{
    [ValidarSesion]
    public class VehiculoController : Controller
    {
        // READ: Lista de vehículos con sus relaciones completas
        public IActionResult Index()
        {
            List<Vehiculo> listaVehiculos = new List<Vehiculo>();
            var connection = ConexionBD.Instancia.ObtenerConexion();

            string query = @"SELECT v.*, c.Nombre as NomCli, c.Apellido as ApeCli, 
                             m.Nombre as NomMod, ma.Nombre as NomMar
                             FROM vehiculo v
                             INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente
                             INNER JOIN modelo m ON v.Id_Modelo = m.Id_Modelo
                             INNER JOIN marca ma ON m.Id_Marca = ma.Id_Marca";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    listaVehiculos.Add(new Vehiculo
                    {
                        IdVehiculo = Convert.ToInt32(reader["Id_Vehiculo"]),
                        Placa = reader["Placa"].ToString(),
                        NumSerie = reader["Num_Serie"].ToString(),
                        Color = reader["Color"].ToString(),
                        KmActual = Convert.ToInt32(reader["Km_Actual"]),
                        Dueño = new Cliente
                        {
                            Nombre = reader["NomCli"].ToString(),
                            Apellido = reader["ApeCli"].ToString()
                        },
                        ModeloVehiculo = new Modelo
                        {
                            Nombre = reader["NomMod"].ToString(),
                            MarcaVehiculo = new Marca { Nombre = reader["NomMar"].ToString() }
                        }
                    });
                }
            }
            return View(listaVehiculos);
        }

        // GET: Vista Crear con Dropdowns de Clientes y Modelos
        public IActionResult Crear()
        {
            CargarListas();
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Vehiculo v)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = @"INSERT INTO vehiculo (Id_Cliente, Id_Modelo, Num_Serie, Color, Placa, Km_Actual) 
                                 VALUES (@cli, @mod, @serie, @col, @pla, @km)";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@cli", v.IdCliente);
                cmd.Parameters.AddWithValue("@mod", v.IdModelo);
                cmd.Parameters.AddWithValue("@serie", v.NumSerie);
                cmd.Parameters.AddWithValue("@col", v.Color);
                cmd.Parameters.AddWithValue("@pla", v.Placa);
                cmd.Parameters.AddWithValue("@km", v.KmActual);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                CargarListas();
                return View(v);
            }
        }

        // Métodos de Editar y Eliminar siguen la misma lógica de los anteriores...

        private void CargarListas()
        {
            ViewBag.Clientes = ObtenerClientes();
            ViewBag.Modelos = ObtenerModelos();
        }

        private List<SelectListItem> ObtenerClientes()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            var connection = ConexionBD.Instancia.ObtenerConexion();
            using (var cmd = new MySqlCommand("SELECT Id_Cliente, Nombre, Apellido FROM cliente", connection))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    lista.Add(new SelectListItem
                    {
                        Value = r["Id_Cliente"].ToString(),
                        Text = $"{r["Nombre"]} {r["Apellido"]}"
                    });
            }
            return lista;
        }

        private List<SelectListItem> ObtenerModelos()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string sql = "SELECT m.Id_Modelo, m.Nombre, ma.Nombre as Marca FROM modelo m INNER JOIN marca ma ON m.Id_Marca = ma.Id_Marca";
            using (var cmd = new MySqlCommand(sql, connection))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                    lista.Add(new SelectListItem
                    {
                        Value = r["Id_Modelo"].ToString(),
                        Text = $"{r["Marca"]} - {r["Nombre"]}"
                    });
            }
            return lista;
        }

        // GET: Vista para editar (Carga datos actuales y Dropdowns)
        public IActionResult Editar(int id)
        {
            Vehiculo vehiculo = null;
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT * FROM vehiculo WHERE Id_Vehiculo = @id";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    vehiculo = new Vehiculo
                    {
                        IdVehiculo = Convert.ToInt32(reader["Id_Vehiculo"]),
                        IdCliente = Convert.ToInt32(reader["Id_Cliente"]),
                        IdModelo = Convert.ToInt32(reader["Id_Modelo"]),
                        NumSerie = reader["Num_Serie"].ToString(),
                        Color = reader["Color"].ToString(),
                        Placa = reader["Placa"].ToString(),
                        KmActual = Convert.ToInt32(reader["Km_Actual"])
                    };
                }
            }

            if (vehiculo == null) return NotFound();

            CargarListas(); // Cargamos Clientes y Modelos para los Dropdowns
            return View(vehiculo);
        }

        // POST: Procesar la actualización
        [HttpPost]
        public IActionResult Editar(Vehiculo v)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = @"UPDATE vehiculo SET Id_Cliente=@cli, Id_Modelo=@mod, 
                         Num_Serie=@serie, Color=@col, Placa=@pla, Km_Actual=@km 
                         WHERE Id_Vehiculo=@id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@cli", v.IdCliente);
                cmd.Parameters.AddWithValue("@mod", v.IdModelo);
                cmd.Parameters.AddWithValue("@serie", v.NumSerie);
                cmd.Parameters.AddWithValue("@col", v.Color);
                cmd.Parameters.AddWithValue("@pla", v.Placa);
                cmd.Parameters.AddWithValue("@km", v.KmActual);
                cmd.Parameters.AddWithValue("@id", v.IdVehiculo);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                CargarListas();
                return View(v);
            }
        }

        // GET: Confirmación de eliminación
        public IActionResult Eliminar(int id)
        {
            Vehiculo vehiculo = null;
            var connection = ConexionBD.Instancia.ObtenerConexion();

            // Traemos también los nombres para que la vista de confirmación sea clara
            string query = @"SELECT v.*, c.Nombre, c.Apellido, m.Nombre as NomMod 
                     FROM vehiculo v
                     INNER JOIN cliente c ON v.Id_Cliente = c.Id_Cliente
                     INNER JOIN modelo m ON v.Id_Modelo = m.Id_Modelo
                     WHERE v.Id_Vehiculo = @id";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    vehiculo = new Vehiculo
                    {
                        IdVehiculo = Convert.ToInt32(reader["Id_Vehiculo"]),
                        Placa = reader["Placa"].ToString(),
                        Dueño = new Cliente { Nombre = reader["Nombre"].ToString(), Apellido = reader["Apellido"].ToString() },
                        ModeloVehiculo = new Modelo { Nombre = reader["NomMod"].ToString() }
                    };
                }
            }

            if (vehiculo == null) return NotFound();
            return View(vehiculo);
        }

        // POST: Eliminar definitivo
        [HttpPost, ActionName("Eliminar")]
        public IActionResult ConfirmarEliminar(int id)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "DELETE FROM vehiculo WHERE Id_Vehiculo = @id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se puede eliminar el vehículo porque tiene servicios registrados en el historial.";
                return RedirectToAction("Index");
            }
        }

    }
}