using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace romo.API.Models.Entities
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int UsuarioID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string NombreCompleto { get; set; }
    }
}