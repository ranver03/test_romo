using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace romo.API.Models.Entities
{
    [Table("FacturaDetalles")]
    public class FacturaDetalle
    {
        [Key]
        public int DetalleID { get; set; }
        public int FacturaID { get; set; }
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitarioHistorico { get; set; }
        public decimal SubtotalLinea { get; set; }

        [ForeignKey("FacturaID")]
        public virtual Factura Factura { get; set; }
        [ForeignKey("ProductoID")]
        public virtual Producto Producto { get; set; }
    }
}