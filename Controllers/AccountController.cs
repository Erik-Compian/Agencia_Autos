using System;
using Microsoft.AspNetCore.Mvc;
using AgenciaMVC1.Data;
using AgenciaMVC1.Models;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http; // Para manejo de sesiones


namespace AgenciaMVC1.Controllers
{
   
    public class AccountController : Controller
    {
        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

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
                        // Creamos el objeto Administrador (POO) con los datos de la DB
                        Administrador admin = new Administrador
                        {
                            IdAdmin = Convert.ToInt32(reader["Id_Admin"]),
                            Usuario = reader["Usuario"].ToString(),
                            Nombre = reader["Nombre"].ToString()
                        };

                        // Guardamos en sesión para cumplir con "Variables globales de sesión" 
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

        // Opción para cerrar sistema (Requisito 18: Seguridad y destruir sesión) 
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Destruye variables de sesión
            ConexionBD.Instancia.CerrarConexion(); // Asegura cierre de base de datos
            return RedirectToAction("Login");
        }

      
    }
}