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
            var hash = usuario?.PasswordHash?.Trim();
            if (string.IsNullOrEmpty(hash) || hash.Length < 20)
                return Unauthorized();

            // Normalizar $2y$ -> $2a$ si tu versión de BCrypt no acepta $2y$
            if (hash.StartsWith("$2y$"))
                hash = "$2a$" + hash.Substring(4);

            bool ok;
            try
            {
                ok = BCrypt.Net.BCrypt.Verify(request.Password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // Loguear hash inválido para diagnóstico
                return Unauthorized();
            }

            if (usuario != null && ok)
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