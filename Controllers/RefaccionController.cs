using Microsoft.AspNetCore.Mvc;
using AgenciaMVC1.Data;
using AgenciaMVC1.Models;
using AgenciaMVC1.Filters; // Para nuestra seguridad
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;

namespace AgenciaMVC1.Controllers
{
    [ValidarSesion]
    public class RefaccionController : Controller
    {
        // READ: Listado de refacciones
        public IActionResult Index()
        {
            List<Refaccion> lista = new List<Refaccion>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT * FROM refaccion";

            using (var cmd = new MySqlCommand(query, conexion))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Refaccion
                    {
                        IdRefaccion = Convert.ToInt32(reader["Id_Refaccion"]),
                        Nombre = reader["Nombre"].ToString(),
                        Codigo = reader["Codigo"].ToString(),
                        Precio = Convert.ToDecimal(reader["Precio"]),
                        Stock = Convert.ToInt32(reader["Stock"])
                    });
                }
            }
            return View(lista);
        }

        // GET: Vista Crear
        public IActionResult Crear() => View();

        // POST: Guardar Refacción
        [HttpPost]
        public IActionResult Crear(Refaccion r)
        {
            try
            {
                var conexion = ConexionBD.Instancia.ObtenerConexion();
                string query = "INSERT INTO refaccion (Nombre, Codigo, Precio, Stock) VALUES (@nom, @cod, @pre, @stk)";

                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nom", r.Nombre);
                    cmd.Parameters.AddWithValue("@cod", r.Codigo);
                    cmd.Parameters.AddWithValue("@pre", r.Precio);
                    cmd.Parameters.AddWithValue("@stk", r.Stock);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(r);
            }
        }

        // GET: Vista Editar
        public IActionResult Editar(int id)
        {
            Refaccion r = null;
            var conexion = ConexionBD.Instancia.ObtenerConexion();
            using (var cmd = new MySqlCommand("SELECT * FROM refaccion WHERE Id_Refaccion = @id", conexion))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        r = new Refaccion
                        {
                            IdRefaccion = Convert.ToInt32(reader["Id_Refaccion"]),
                            Nombre = reader["Nombre"].ToString(),
                            Codigo = reader["Codigo"].ToString(),
                            Precio = Convert.ToDecimal(reader["Precio"]),
                            Stock = Convert.ToInt32(reader["Stock"])
                        };
                    }
                }
            }
            return r == null ? NotFound() : View(r);
        }

        // POST: Actualizar Refacción
        [HttpPost]
        public IActionResult Editar(Refaccion r)
        {
            try
            {
                var conexion = ConexionBD.Instancia.ObtenerConexion();
                string query = "UPDATE refaccion SET Nombre=@nom, Codigo=@cod, Precio=@pre, Stock=@stk WHERE Id_Refaccion=@id";
                using (var cmd = new MySqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nom", r.Nombre);
                    cmd.Parameters.AddWithValue("@cod", r.Codigo);
                    cmd.Parameters.AddWithValue("@pre", r.Precio);
                    cmd.Parameters.AddWithValue("@stk", r.Stock);
                    cmd.Parameters.AddWithValue("@id", r.IdRefaccion);
                    cmd.ExecuteNonQuery();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error: " + ex.Message;
                return View(r);
            }
        }

        // GET y POST para Eliminar (Lógica simplificada para avanzar rápido)
        public IActionResult Eliminar(int id) => Editar(id);

        [HttpPost, ActionName("Eliminar")]
        public IActionResult ConfirmarEliminar(int id)
        {
            try
            {
                var conexion = ConexionBD.Instancia.ObtenerConexion();
                using (var cmd = new MySqlCommand("DELETE FROM refaccion WHERE Id_Refaccion = @id", conexion))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                TempData["Error"] = "No se puede eliminar la refacción porque ya se usó en un servicio.";
            }
            return RedirectToAction("Index");
        }
    }
}