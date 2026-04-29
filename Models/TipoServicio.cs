using Microsoft.AspNetCore.Mvc;

namespace AgenciaMVC1.Models
{
    public class TipoServicio
    {
        public int IdTipoServ { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }
}