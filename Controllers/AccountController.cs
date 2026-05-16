using System;
using Microsoft.AspNetCore.Mvc;
using AgenciaMVC1.Data;
using AgenciaMVC1.Models;
using AgenciaMVC1.Filters;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AgenciaMVC1.Controllers
{
    public class AccountController : Controller
    {
        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string usuario, string password)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "SELECT * FROM administrador WHERE Usuario = @user AND Password = @pass";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@user", usuario);
                cmd.Parameters.AddWithValue("@pass", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Administrador admin = new Administrador
                        {
                            IdAdmin = Convert.ToInt32(reader["Id_Admin"]),
                            Usuario = reader["Usuario"].ToString(),
                            Nombre = reader["Nombre"].ToString()
                        };
                        HttpContext.Session.SetString("UsuarioNombre", admin.Nombre);
                        HttpContext.Session.SetInt32("AdminId", admin.IdAdmin);
                        return RedirectToAction("Index", "Home");
                    }
                }
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error de conexión: " + ex.Message;
                return View();
            }
        }

        // GET: Lista de administradores
        [ValidarSesion]
        public IActionResult Administradores()
        {
            List<Administrador> lista = new List<Administrador>();
            var conexion = ConexionBD.Instancia.ObtenerConexion();

            using (var cmd = new MySqlCommand("SELECT Id_Admin, Usuario, Nombre, Email FROM administrador", conexion))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new Administrador
                    {
                        IdAdmin = Convert.ToInt32(reader["Id_Admin"]),
                        Usuario = reader["Usuario"].ToString(),
                        Nombre = reader["Nombre"].ToString(),
                        Email = reader["Email"].ToString()
                    });
                }
            }
            return View(lista);
        }

        // GET: Formulario nuevo administrador
        [ValidarSesion]
        public IActionResult NuevoAdmin()
        {
            return View();
        }

        // POST: Guardar nuevo administrador
        [HttpPost]
        [ValidarSesion]
        public IActionResult NuevoAdmin(string nombre, string usuario, string password, string email)
        {
            try
            {
                var conexion = ConexionBD.Instancia.ObtenerConexion();

                // Verificar si el usuario ya existe
                using (var cmdCheck = new MySqlCommand("SELECT COUNT(*) FROM administrador WHERE Usuario = @usr", conexion))
                {
                    cmdCheck.Parameters.AddWithValue("@usr", usuario);
                    int existe = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (existe > 0)
                    {
                        ViewBag.Error = "El nombre de usuario ya existe, elige otro.";
                        return View();
                    }
                }

                string sql = "INSERT INTO administrador (Nombre, Usuario, Password, Email) VALUES (@nom, @usr, @pass, @email)";
                using (var cmd = new MySqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@nom", nombre);
                    cmd.Parameters.AddWithValue("@usr", usuario);
                    cmd.Parameters.AddWithValue("@pass", password);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.ExecuteNonQuery();
                }

                TempData["Exito"] = "Administrador creado correctamente.";
                return RedirectToAction("Administradores");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al crear administrador: " + ex.Message;
                return View();
            }
        }

        // POST: Eliminar administrador
        [HttpPost]
        [ValidarSesion]
        public IActionResult EliminarAdmin(int id)
        {
            // No permitir eliminar al admin que está en sesión
            int adminActual = HttpContext.Session.GetInt32("AdminId") ?? 0;
            if (id == adminActual)
            {
                TempData["Error"] = "No puedes eliminar tu propia cuenta.";
                return RedirectToAction("Administradores");
            }

            var conexion = ConexionBD.Instancia.ObtenerConexion();
            using (var cmd = new MySqlCommand("DELETE FROM administrador WHERE Id_Admin = @id", conexion))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }

            TempData["Exito"] = "Administrador eliminado correctamente.";
            return RedirectToAction("Administradores");
        }

        // Cerrar sesión
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            ConexionBD.Instancia.CerrarConexion();
            return RedirectToAction("Login");
        }
    }
}
