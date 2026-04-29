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

    public class ModeloController : Controller
    {
        // READ: Lista de modelos con el nombre de su marca
        public IActionResult Index()
        {
            List<Modelo> listaModelos = new List<Modelo>();
            var connection = ConexionBD.Instancia.ObtenerConexion();

            // Usamos un JOIN para traer el nombre de la marca (POO)
            string query = @"SELECT m.*, ma.Nombre as NombreMarca 
                             FROM modelo m 
                             INNER JOIN marca ma ON m.Id_Marca = ma.Id_Marca";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    listaModelos.Add(new Modelo
                    {
                        IdModelo = Convert.ToInt32(reader["Id_Modelo"]),
                        Nombre = reader["Nombre"].ToString(),
                        Anio = Convert.ToInt32(reader["Anio"]),
                        IdMarca = Convert.ToInt32(reader["Id_Marca"]),
                        MarcaVehiculo = new Marca { Nombre = reader["NombreMarca"].ToString() }
                    });
                }
            }
            return View(listaModelos);
        }

        // GET: Vista Crear con lista de marcas
        public IActionResult Crear()
        {
            ViewBag.Marcas = ObtenerListaMarcas();
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Modelo nuevoModelo)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "INSERT INTO modelo (Id_Marca, Nombre, Anio) VALUES (@idMarca, @nombre, @anio)";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idMarca", nuevoModelo.IdMarca);
                cmd.Parameters.AddWithValue("@nombre", nuevoModelo.Nombre);
                cmd.Parameters.AddWithValue("@anio", nuevoModelo.Anio);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al guardar: " + ex.Message;
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(nuevoModelo);
            }
        }

        // Método auxiliar para llenar el Dropdown de Marcas
        private List<SelectListItem> ObtenerListaMarcas()
        {
            List<SelectListItem> marcas = new List<SelectListItem>();
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT Id_Marca, Nombre FROM marca";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    marcas.Add(new SelectListItem
                    {
                        Value = reader["Id_Marca"].ToString(),
                        Text = reader["Nombre"].ToString()
                    });
                }
            }
            return marcas;
        }
        // GET: Vista para editar (Carga los datos actuales)
        public IActionResult Editar(int id)
        {
            Modelo modelo = null;
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT * FROM modelo WHERE Id_Modelo = @id";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    modelo = new Modelo
                    {
                        IdModelo = Convert.ToInt32(reader["Id_Modelo"]),
                        IdMarca = Convert.ToInt32(reader["Id_Marca"]),
                        Nombre = reader["Nombre"].ToString(),
                        Anio = Convert.ToInt32(reader["Anio"])
                    };
                }
            }

            if (modelo == null) return NotFound();

            ViewBag.Marcas = ObtenerListaMarcas(); // Cargamos el Dropdown
            return View(modelo);
        }

        // POST: Procesar la edición
        [HttpPost]
        public IActionResult Editar(Modelo modelo)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "UPDATE modelo SET Id_Marca = @idMarca, Nombre = @nombre, Anio = @anio WHERE Id_Modelo = @id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@idMarca", modelo.IdMarca);
                cmd.Parameters.AddWithValue("@nombre", modelo.Nombre);
                cmd.Parameters.AddWithValue("@anio", modelo.Anio);
                cmd.Parameters.AddWithValue("@id", modelo.IdModelo);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(modelo);
            }
        }

        // GET: Confirmación de eliminación
        public IActionResult Eliminar(int id)
        {
            Modelo modelo = null;
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = @"SELECT m.*, ma.Nombre as NombreMarca 
                     FROM modelo m 
                     INNER JOIN marca ma ON m.Id_Marca = ma.Id_Marca 
                     WHERE m.Id_Modelo = @id";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    modelo = new Modelo
                    {
                        IdModelo = Convert.ToInt32(reader["Id_Modelo"]),
                        Nombre = reader["Nombre"].ToString(),
                        Anio = Convert.ToInt32(reader["Anio"]),
                        MarcaVehiculo = new Marca { Nombre = reader["NombreMarca"].ToString() }
                    };
                }
            }

            if (modelo == null) return NotFound();
            return View(modelo);
        }

        // POST: Eliminar definitivo
        [HttpPost, ActionName("Eliminar")]
        public IActionResult ConfirmarEliminar(int id)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "DELETE FROM modelo WHERE Id_Modelo = @id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se puede eliminar el modelo porque ya hay vehículos registrados con este modelo.";
                return RedirectToAction("Index");
            }
        }


    }
}