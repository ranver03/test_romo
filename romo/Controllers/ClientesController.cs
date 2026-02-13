using romo.API.Data;
using romo.API.Models.Entities;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace romo.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/clientes")]
    public class ClientesController : ApiController
    {
        private FacturacionContext db = new FacturacionContext();

        // 1. LISTAR CLIENTES ACTIVOS
        [HttpGet]
        [Route("listar")]
        public IHttpActionResult Listar()
        {
            try
            {
                var clientes = db.Clientes
                    .Where(c => c.Activo)
                    .Select(c => new {
                        c.ClienteID,
                        c.Identificacion,
                        c.RazonSocial,
                        c.Email,
                        c.Activo
                    })
                    .OrderBy(c => c.RazonSocial)
                    .ToList();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // 2. OBTENER UN CLIENTE POR ID
        [HttpGet]
        [Route("obtener/{id}")]
        public IHttpActionResult Obtener(int id)
        {
            var cliente = db.Clientes.Find(id);
            if (cliente == null) return NotFound();

            return Ok(cliente);
        }

        // 3. CREAR CLIENTE
        [HttpPost]
        [Route("crear")]
        public IHttpActionResult Crear(Cliente cliente)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // VALIDACIÓN: Verificar si la identificación ya existe
                bool existe = db.Clientes.Any(c => c.Identificacion == cliente.Identificacion);
                if (existe)
                {
                    return Content(System.Net.HttpStatusCode.Conflict, new
                    {
                        Success = false,
                        Message = "La identificación '" + cliente.Identificacion + "' ya pertenece a un cliente registrado."
                    });
                }

                cliente.Activo = true;
                db.Clientes.Add(cliente);
                db.SaveChanges();

                return Ok(new { Success = true, Message = "Cliente creado exitosamente", ID = cliente.ClienteID });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // 4. ACTUALIZAR CLIENTE
        [HttpPut]
        [Route("actualizar")]
        public IHttpActionResult Actualizar(Cliente cliente)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // VALIDACIÓN: ¿Existe OTRA persona con esta identificación que no sea yo?
            bool existeDuplicado = db.Clientes.Any(c => c.Identificacion == cliente.Identificacion
                                                    && c.ClienteID != cliente.ClienteID);

            if (existeDuplicado)
            {
                return Content(System.Net.HttpStatusCode.Conflict, new
                {
                    Success = false,
                    Message = "No se puede actualizar: La identificación '" + cliente.Identificacion + "' ya está asignada a otro cliente."
                });
            }

            var clienteDb = db.Clientes.Find(cliente.ClienteID);
            if (clienteDb == null) return NotFound();

            try
            {
                clienteDb.Identificacion = cliente.Identificacion;
                clienteDb.RazonSocial = cliente.RazonSocial;
                clienteDb.Email = cliente.Email;
                clienteDb.Activo = cliente.Activo;

                db.Entry(clienteDb).State = EntityState.Modified;
                db.SaveChanges();

                return Ok(new { Success = true, Message = "Cliente actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // 5. ELIMINAR CLIENTE (BORRADO LÓGICO)
        [HttpDelete]
        [Route("eliminar/{id}")]
        public IHttpActionResult Eliminar(int id)
        {
            var cliente = db.Clientes.Find(id);
            if (cliente == null) return NotFound();

            try
            {
                cliente.Activo = false;
                db.Entry(cliente).State = EntityState.Modified;
                db.SaveChanges();

                return Ok(new { Success = true, Message = "Cliente desactivado correctamente" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
