using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace romo.API.Models.Entities
{
    [Table("Facturas")]
    public class Factura
    {
        [Key]
        public int FacturaID { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string NumeroFactura { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteID { get; set; }
        public decimal Subtotal { get; set; }
        public decimal IVA_Total { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }

        public virtual Cliente Cliente { get; set; }
        public virtual ICollection<FacturaDetalle> Detalles { get; set; }
    }
}