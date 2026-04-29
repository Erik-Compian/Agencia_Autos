using Microsoft.AspNetCore.Mvc;

namespace AgenciaMVC1.Models
{
    public class Administrador
    {
        public int IdAdmin { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
    }
}