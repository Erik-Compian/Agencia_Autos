using AgenciaMVC1.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaMVC1.Models
{
    public class Modelo
    {
        public int IdModelo { get; set; }
        public int IdMarca { get; set; } // Llave foránea
        public string Nombre { get; set; }
        public int Anio { get; set; }

        // Propiedad de navegación (POO)
        public Marca MarcaVehiculo { get; set; }
    }
}