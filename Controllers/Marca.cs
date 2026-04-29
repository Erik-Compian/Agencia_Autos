using Microsoft.AspNetCore.Mvc;
using AgenciaMVC1.Data;
using AgenciaMVC1.Models;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;
using AgenciaMVC1.Filters;

namespace AgenciaMVC1.Controllers
{
    [ValidarSesion]
    public class MarcaController : Controller
    {
        // READ: Mostrar la lista de marcas
        public IActionResult Index()
        {
            List<Marca> listaMarcas = new List<Marca>();
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT * FROM marca";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    listaMarcas.Add(new Marca
                    {
                        IdMarca = Convert.ToInt32(reader["Id_Marca"]),
                        Nombre = reader["Nombre"].ToString()
                    });
                }
            }

            return View(listaMarcas); // Mandamos la lista a la vista
        }

        // GET: Vista para crear una nueva marca
        public IActionResult Crear()
        {
            return View();
        }

        // CREATE: Guardar la marca en la base de datos
        [HttpPost]
        public IActionResult Crear(Marca nuevaMarca)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "INSERT INTO marca (Nombre) VALUES (@nombre)";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", nuevaMarca.Nombre);

                cmd.ExecuteNonQuery();

                return RedirectToAction("Index"); // Regresa a la lista
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al guardar: " + ex.Message;
                return View(nuevaMarca);
            }
        }

        // GET: Vista para editar (Carga los datos actuales)
        public IActionResult Editar(int id)
        {
            Marca marca = null;
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT * FROM marca WHERE Id_Marca = @id";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    marca = new Marca
                    {
                        IdMarca = Convert.ToInt32(reader["Id_Marca"]),
                        Nombre = reader["Nombre"].ToString()
                    };
                }
            }

            if (marca == null) return NotFound();
            return View(marca);
        }

        // POST: Procesar la actualización
        [HttpPost]
        public IActionResult Editar(Marca marca)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "UPDATE marca SET Nombre = @nombre WHERE Id_Marca = @id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nombre", marca.Nombre);
                cmd.Parameters.AddWithValue("@id", marca.IdMarca);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                return View(marca);
            }
        }

        // GET: Vista de confirmación para eliminar
        public IActionResult Eliminar(int id)
        {
            // Reutilizamos la lógica de buscar por ID para mostrar qué se va a borrar
            return Editar(id);
        }

        // POST: Confirmar eliminación
        [HttpPost, ActionName("Eliminar")]
        public IActionResult ConfirmarEliminar(int id)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "DELETE FROM marca WHERE Id_Marca = @id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se puede eliminar la marca porque está asociada a un vehículo.";
                return RedirectToAction("Index");
            }
        }

    }
}