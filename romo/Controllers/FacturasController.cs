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
                // Traemos las facturas con sus detalles y productos (Eager Loading)
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
    }
}
