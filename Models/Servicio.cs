using AgenciaMVC1.Models;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;

namespace AgenciaMVC1.Models
{
    public class Servicio
    {
        public int Folio { get; set; }
        public int IdVehiculo { get; set; }
        public int IdAdmin { get; set; }

        // Atributo clave que pide el gerente: ¿Quién trajo el auto? (Ej. Mauricio, Mireya, etc.)
        public string QuienEntrego { get; set; }

        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaSalida { get; set; } // El ? permite que sea nulo si aún no sale
        public EstatusServicio Estatus { get; set; }
        public string Descripcion { get; set; }

        // --- Propiedades de Navegación Orientadas a Objetos ---
        public Vehiculo VehiculoAtendido { get; set; }
        public Administrador AtendidoPor { get; set; }

        // Lista de refacciones utilizadas en este servicio específico
        public List<ServicioRefaccion> Refacciones { get; set; } = new List<ServicioRefaccion>();
    }
}