using System.Collections.Generic;

namespace romo.Models.DTOs
{
    public class FacturaDto
    {
        public int ClienteID { get; set; }
        public List<DetalleDto> Detalles { get; set; }
    }

    public class DetalleDto
    {
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
    }
}