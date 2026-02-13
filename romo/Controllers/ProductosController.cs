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
    [RoutePrefix("api/productos")]
    public class ProductosController : ApiController
    {
        private FacturacionContext db = new FacturacionContext();

        // 1. LISTAR TODOS LOS PRODUCTOS ACTIVOS
        [HttpGet]
        [Route("listar")]
        public IHttpActionResult Listar()
        {
            try
            {
                var productos = db.Productos
                    .Where(p => p.Activo)
                    .Select(p => new {
                        p.ProductoID,
                        p.SKU,
                        p.Nombre,
                        p.PrecioUnitario,
                        p.IVA_Porcentaje,
                        p.Stock,
                        p.Activo
                    })
                    .OrderBy(p => p.Nombre)
                    .ToList();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // 2. OBTENER UN PRODUCTO POR ID
        [HttpGet]
        [Route("obtener/{id}")]
        public IHttpActionResult Obtener(int id)
        {
            var producto = db.Productos.Find(id);
            if (producto == null) return NotFound();

            return Ok(producto);
        }

        // 3. CREAR NUEVO PRODUCTO
        [HttpPost]
        [Route("crear")]
        public IHttpActionResult Crear(Producto producto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // VALIDACIÓN: Verificar si el SKU ya existe
                bool existe = db.Productos.Any(p => p.SKU == producto.SKU);
                if (existe)
                {
                    return Content(System.Net.HttpStatusCode.Conflict, new
                    {
                        Success = false,
                        Message = "El código SKU '" + producto.SKU + "' ya está en uso por otro producto."
                    });
                }

                // Forzamos el estado activo al crear
                producto.Activo = true;

                db.Productos.Add(producto);
                db.SaveChanges();

                return Ok(new
                {
                    Success = true,
                    Message = "Producto creado exitosamente",
                    ID = producto.ProductoID
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // 4. ACTUALIZAR PRODUCTO EXISTENTE
        [HttpPut]
        [Route("actualizar")]
        public IHttpActionResult Actualizar(Producto producto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var prodDb = db.Productos.Find(producto.ProductoID);
            if (prodDb == null) return NotFound();

            try
            {
                // VALIDACIÓN: ¿Existe OTRO producto con este SKU que no sea el actual?
                bool existeDuplicado = db.Productos.Any(p => p.SKU == producto.SKU
                                                         && p.ProductoID != producto.ProductoID);

                if (existeDuplicado)
                {
                    return Content(System.Net.HttpStatusCode.Conflict, new
                    {
                        Success = false,
                        Message = "Conflicto de código: El SKU '" + producto.SKU + "' ya pertenece a otro producto."
                    });
                }

                // Actualizamos todos los campos del modelo
                prodDb.SKU = producto.SKU;
                prodDb.Nombre = producto.Nombre;
                prodDb.PrecioUnitario = producto.PrecioUnitario;
                prodDb.IVA_Porcentaje = producto.IVA_Porcentaje;
                prodDb.Stock = producto.Stock;
                prodDb.Activo = producto.Activo;

                db.Entry(prodDb).State = EntityState.Modified;
                db.SaveChanges();

                return Ok(new { Success = true, Message = "Producto actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // 5. ELIMINAR (BORRADO LÓGICO)
        [HttpDelete]
        [Route("eliminar/{id}")]
        public IHttpActionResult Eliminar(int id)
        {
            var producto = db.Productos.Find(id);
            if (producto == null) return NotFound();

            try
            {
                // En lugar de borrar de la tabla, desactivamos el registro
                producto.Activo = false;

                db.Entry(producto).State = EntityState.Modified;
                db.SaveChanges();

                return Ok(new { Success = true, Message = "Producto desactivado correctamente" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}