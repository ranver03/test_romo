using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace romo.API.Models.Entities
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int ClienteID { get; set; }
        public string Identificacion { get; set; }
        public string RazonSocial { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
    }
}