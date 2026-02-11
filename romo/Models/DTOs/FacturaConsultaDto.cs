using System;
using System.Collections.Generic;

namespace romo.Models.DTOs
{
    public class FacturaConsultaDto
    {
        public int FacturaID { get; set; }
        public string NumeroFactura { get; set; }
        public DateTime Fecha { get; set; }
        public string ClienteNombre { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        // Aquí está la anidación
        public List<DetalleConsultaDto> Detalles { get; set; }
    }

    public class DetalleConsultaDto
    {
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }
    }
}