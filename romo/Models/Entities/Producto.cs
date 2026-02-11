using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace romo.API.Models.Entities
{
    [Table("Productos")]
    public class Producto
    {
        [Key]
        public int ProductoID { get; set; }
        public string SKU { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal IVA_Porcentaje { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }
    }
}