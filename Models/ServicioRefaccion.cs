using AgenciaMVC1.Models;
using Microsoft.AspNetCore.Mvc;

namespace AgenciaMVC1.Models
{
    public class ServicioRefaccion
    {
        public int Folio { get; set; } // Referencia al Servicio
        public int IdRefaccion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioAplicado { get; set; }

        public Refaccion RefaccionUtilizada { get; set; }
    }
}