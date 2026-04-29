using Microsoft.AspNetCore.Mvc;

namespace AgenciaMVC1.Models
{
    public class Refaccion
    {
        public int IdRefaccion { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}