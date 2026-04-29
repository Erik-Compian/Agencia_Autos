using AgenciaMVC1.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaMVC1.Models
{
    public class Vehiculo
    {
        public int IdVehiculo { get; set; }
        public int IdCliente { get; set; }
        public int IdModelo { get; set; }
        public string NumSerie { get; set; }
        public string Color { get; set; }
        public string Placa { get; set; }
        public int KmActual { get; set; }

        // Propiedades de navegación para la POO. 
        // Ej: Aquí cargaríamos la info si fuera un Dodge Dart 2013.
        public Cliente Dueño { get; set; }
        public Modelo ModeloVehiculo { get; set; }
    }
}