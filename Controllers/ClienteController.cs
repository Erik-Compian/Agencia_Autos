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
    public class ClienteController : Controller
    {
        // READ: Lista de todos los clientes
        public IActionResult Index()
        {
            List<Cliente> listaClientes = new List<Cliente>();
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT * FROM cliente";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    listaClientes.Add(new Cliente
                    {
                        IdCliente = Convert.ToInt32(reader["Id_Cliente"]),
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString(),
                        Telefono = reader["Telefono"].ToString(),
                        Email = reader["Email"].ToString()
                    });
                }
            }
            return View(listaClientes);
        }

        // GET: Vista Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Guardar Cliente
        [HttpPost]
        public IActionResult Crear(Cliente cliente)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "INSERT INTO cliente (Nombre, Apellido, Telefono, Email) VALUES (@nom, @ape, @tel, @mail)";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", cliente.Nombre);
                cmd.Parameters.AddWithValue("@ape", cliente.Apellido);
                cmd.Parameters.AddWithValue("@tel", cliente.Telefono);
                cmd.Parameters.AddWithValue("@mail", cliente.Email);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al registrar cliente: " + ex.Message;
                return View(cliente);
            }
        }

        // GET: Vista Editar
        public IActionResult Editar(int id)
        {
            Cliente cliente = null;
            var connection = ConexionBD.Instancia.ObtenerConexion();
            string query = "SELECT * FROM cliente WHERE Id_Cliente = @id";

            MySqlCommand cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    cliente = new Cliente
                    {
                        IdCliente = Convert.ToInt32(reader["Id_Cliente"]),
                        Nombre = reader["Nombre"].ToString(),
                        Apellido = reader["Apellido"].ToString(),
                        Telefono = reader["Telefono"].ToString(),
                        Email = reader["Email"].ToString()
                    };
                }
            }
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        // POST: Actualizar Cliente
        [HttpPost]
        public IActionResult Editar(Cliente cliente)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "UPDATE cliente SET Nombre=@nom, Apellido=@ape, Telefono=@tel, Email=@mail WHERE Id_Cliente=@id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@nom", cliente.Nombre);
                cmd.Parameters.AddWithValue("@ape", cliente.Apellido);
                cmd.Parameters.AddWithValue("@tel", cliente.Telefono);
                cmd.Parameters.AddWithValue("@mail", cliente.Email);
                cmd.Parameters.AddWithValue("@id", cliente.IdCliente);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al actualizar: " + ex.Message;
                return View(cliente);
            }
        }

        // GET: Confirmar Eliminación
        public IActionResult Eliminar(int id)
        {
            return Editar(id); // Reutilizamos la lógica de búsqueda de Editar
        }

        // POST: Eliminar Definitivo
        [HttpPost, ActionName("Eliminar")]
        public IActionResult ConfirmarEliminar(int id)
        {
            try
            {
                var connection = ConexionBD.Instancia.ObtenerConexion();
                string query = "DELETE FROM cliente WHERE Id_Cliente = @id";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "No se puede eliminar el cliente porque tiene vehículos o servicios registrados.";
                return RedirectToAction("Index");
            }
        }

        }
    }
