using AgenciaMVC1.Models;
using Microsoft.AspNetCore.Mvc;

using System;

namespace AgenciaMVC1.Models
{
    public class ProximoServicio
    {
        public int IdProxServ { get; set; }
        public int Folio { get; set; } // El servicio que detonó esta programación
        public DateTime FechaProg { get; set; }
        public int KmProximo { get; set; }
        public string Notas { get; set; }

        public Servicio ServicioOrigen { get; set; }
    }
}