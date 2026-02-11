using romo.API.Data;
using romo.API.Models.Entities;
using romo.Models.DTOs;
using System;
using System.Linq;

namespace romo.Services
{
    public class FacturacionService
    {
        private FacturacionContext db = new FacturacionContext();

        public Factura CrearFactura(FacturaDto dto)
        {
            // Iniciamos la Transacción
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Crear la instancia de la Factura
                    var nuevaFactura = new Factura
                    {
                        ClienteID = dto.ClienteID,
                        Fecha = DateTime.Now,
                        Estado = "ACTIVA",
                        Subtotal = 0,
                        IVA_Total = 0,
                        Total = 0
                    };

                    db.Facturas.Add(nuevaFactura);
                    db.SaveChanges(); // Guardamos para obtener el FacturaID

                    decimal acumuladoSubtotal = 0;
                    decimal acumuladoIVA = 0;

                    // 2. Procesar detalles
                    foreach (var item in dto.Detalles)
                    {
                        var producto = db.Productos.Find(item.ProductoID);
                        if (producto == null) throw new Exception($"Producto {item.ProductoID} no existe");

                        decimal subtotalLinea = producto.PrecioUnitario * item.Cantidad;
                        decimal ivaLinea = subtotalLinea * (producto.IVA_Porcentaje / 100);

                        var detalle = new FacturaDetalle
                        {
                            FacturaID = nuevaFactura.FacturaID,
                            ProductoID = item.ProductoID,
                            Cantidad = item.Cantidad,
                            PrecioUnitarioHistorico = producto.PrecioUnitario,
                            SubtotalLinea = subtotalLinea
                        };

                        db.FacturaDetalles.Add(detalle);

                        acumuladoSubtotal += subtotalLinea;
                        acumuladoIVA += ivaLinea;
                    }

                    // 3. Actualizar totales de la cabecera
                    nuevaFactura.Subtotal = acumuladoSubtotal;
                    nuevaFactura.IVA_Total = acumuladoIVA;
                    nuevaFactura.Total = acumuladoSubtotal + acumuladoIVA;

                    db.SaveChanges();

                    // 4. Confirmar todo
                    dbTransaction.Commit();
                    return nuevaFactura;
                }
                catch (Exception)
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }
        }
    }
}