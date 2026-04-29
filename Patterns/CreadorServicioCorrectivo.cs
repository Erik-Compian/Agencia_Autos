using AgenciaMVC1.Models;
using System;

namespace AgenciaMVC1.Patterns
{
    public class CreadorServicioCorrectivo : ICreadorServicio
    {
        public Servicio CrearServicio(Vehiculo eVehiculo, int idAdmin, string quienEntrego)
        {
            return new Servicio
            {
                IdVehiculo = eVehiculo.IdVehiculo,
                IdAdmin = idAdmin,
                QuienEntrego = quienEntrego,
                FechaIngreso = DateTime.Now,
                Estatus = EstatusServicio.EnEspera,
                Descripcion = "Mantenimiento Correctivo: Reparación de falla reportada por el cliente.",
                VehiculoAtendido = eVehiculo
            };
        }
    }
}