using BCrypt.Net; // Importante
using romo.API.Data;
using romo.Models;
using romo.API.Models.Entities;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace romo.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private FacturacionContext db = new FacturacionContext();

        // 1. ENDPOINT PARA CREAR USUARIOS (Solo Postman)
        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(LoginRequest request)
        {
            if (db.Usuarios.Any(u => u.Username == request.Username))
                return BadRequest("El usuario ya existe");

            // Hasheamos la contraseña antes de guardar
            string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            string hashedPath = BCrypt.Net.BCrypt.HashPassword(request.Password, salt);

            var nuevoUsuario = new Usuario
            {
                Username = request.Username,
                PasswordHash = hashedPath,
                NombreCompleto = "Usuario Nuevo" // Puedes pedirlo en el DTO si quieres
            };

            db.Usuarios.Add(nuevoUsuario);
            db.SaveChanges();

            return Ok("Usuario creado exitosamente");
        }

        // 2. LOGIN ACTUALIZADO CON VERIFICACIÓN DE HASH
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            var usuario = db.Usuarios.FirstOrDefault(u => u.Username == request.Username);

            // Verificamos si el usuario existe y si la contraseña coincide con el hash
            if (usuario != null && BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                return Ok(new
                {
                    Success = true,
                    Message = "Bienvenido",
                    Usuario = usuario.NombreCompleto
                });
            }

            return Unauthorized();
        }
    }
}