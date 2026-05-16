using Microsoft.AspNetCore.Mvc;
using AgenciaMVC1.Data;
using AgenciaMVC1.Filters;
using MySql.Data.MySqlClient;
using System;

namespace AgenciaMVC1.Controllers
{
    public class UsuarioController : Controller
    {
        // GET: Usuario/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Usuario/Crear
        [HttpPost]
        public IActionResult Crear(string nombre, string usuario, string correo, string contrasenia, string rol)
        {
            // 1. Validación de campos vacíos (se incluyó 'usuario')
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasenia))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

            if (string.IsNullOrEmpty(rol) || rol == "Administrador")
            {
                rol = "Estandar";
            }

            try
            {
                var conexion = ConexionBD.Instancia.ObtenerConexion();

                // 2. Control de duplicados en la tabla 'usuarios'
                using (var cmdCheck = new MySqlCommand("SELECT COUNT(*) FROM usuarios WHERE Usuario = @usr", conexion))
                {
                    cmdCheck.Parameters.AddWithValue("@usr", usuario);
                    int existe = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (existe > 0)
                    {
                        ViewBag.Error = "El nombre de usuario ya está en uso, elige otro.";
                        return View();
                    }
                }

                // 3. Inserción limpia con columnas separadas
                string sql = @"INSERT INTO usuarios (Nombre, Usuario, Password, Email, Rol) 
                               VALUES (@nombre, @usuario, @password, @email, @rol)";

                using (var cmd = new MySqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@usuario", usuario); // Recibe el apodo/texto corto
                    cmd.Parameters.AddWithValue("@password", contrasenia);
                    cmd.Parameters.AddWithValue("@email", correo); // Guarda el correo por separado
                    cmd.Parameters.AddWithValue("@rol", rol);

                    cmd.ExecuteNonQuery();
                }

                TempData["MensajeExito"] = "¡Usuario creado exitosamente! Ya puedes iniciar sesión.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al registrar en la tabla usuarios: " + ex.Message;
                return View();
            }
        }
    }
}