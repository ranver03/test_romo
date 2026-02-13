using System.Web.Http;
using romo.Models.DTOs;
using romo.Services;
using System.Linq;

namespace romo.Controllers
{
    [RoutePrefix("api/facturas")]
    public class FacturasController : ApiController
    {
        private FacturacionService _service = new FacturacionService();

        [HttpPost]
        [Route("crear")]
        public IHttpActionResult Crear(FacturaDto dto)
        {
            try
            {
                var resultado = _service.CrearFactura(dto);
                return Ok(new { Success = true, FacturaID = resultado.FacturaID });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("listar")]
        public IHttpActionResult Listar()
        {
            using (var db = new romo.API.Data.FacturacionContext())
            {
                var facturas = db.Facturas
                    .OrderByDescending(f => f.Fecha)
                    .Select(f => new FacturaConsultaDto
                    {
                        FacturaID = f.FacturaID,
                        NumeroFactura = f.NumeroFactura,
                        Fecha = f.Fecha,
                        ClienteNombre = f.Cliente.RazonSocial,
                        Total = f.Total,
                        Estado = f.Estado,
                        Detalles = f.Detalles.Select(d => new DetalleConsultaDto
                        {
                            ProductoNombre = d.Producto.Nombre,
                            Cantidad = d.Cantidad,
                            PrecioUnitario = d.PrecioUnitarioHistorico,
                            SubtotalLinea = d.SubtotalLinea
                        }).ToList()
                    }).ToList();

                return Ok(facturas);
            }
        }

        [HttpGet]
        [Route("obtener/{id}")]
        public IHttpActionResult ObtenerPorId(int id)
        {
            try
            {
                using (var db = new romo.API.Data.FacturacionContext())
                {
                    var factura = db.Facturas
                        .Include("Detalles")
                        .Include("Detalles.Producto")
                        .Include("Cliente")
                        .FirstOrDefault(f => f.FacturaID == id);

                    if (factura == null) return NotFound();

                    var resultado = new
                    {
                        factura.FacturaID,
                        factura.ClienteID,
                        ClienteNombre = factura.Cliente != null ? factura.Cliente.RazonSocial : "Cliente Desconocido",
                        factura.Fecha,
                        factura.Estado,
                        factura.Subtotal,
                        factura.IVA_Total,
                        factura.Total,
                        Detalles = factura.Detalles.Select(d => new {
                            d.ProductoID,
                            ProductoNombre = d.Producto != null ? d.Producto.Nombre : "Producto #" + d.ProductoID,
                            d.Cantidad,
                            d.PrecioUnitarioHistorico,
                            d.SubtotalLinea,
                            d.IVALinea,
                            d.TotalLinea
                        }).ToList()
                    };

                    return Ok(resultado);
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- ACTUALIZAR / EDITAR FACTURA ---
        [HttpPut]
        [Route("actualizar/{id}")]
        public IHttpActionResult Actualizar(int id, FacturaDto dto)
        {
            try
            {
                var resultado = _service.ActualizarFactura(id, dto);
                return Ok(new { Success = true, Message = "Factura actualizada correctamente" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- ANULAR FACTURA (Eliminación Lógica) ---
        [HttpDelete]
        [Route("anular/{id}")]
        public IHttpActionResult Anular(int id)
        {
            try
            {
                _service.AnularFactura(id);
                return Ok(new { Success = true, Message = "Factura anulada con éxito" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
    }
}
