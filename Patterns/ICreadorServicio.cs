using AgenciaMVC1.Models;

namespace AgenciaMVC1.Patterns
{
    public interface ICreadorServicio
    {
        // Método de fábrica definido en tu UML
        Servicio CrearServicio(Vehiculo eVehiculo, int idAdmin, string quienEntrego);
    }
}