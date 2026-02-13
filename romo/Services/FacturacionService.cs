using romo.API.Data;
using romo.API.Models.Entities;
using romo.Models.DTOs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;

namespace romo.Services
{
    public class FacturacionService
    {
        private FacturacionContext db = new FacturacionContext();

        // --- MÉTODO 1: CREAR FACTURA ---
        public Factura CrearFactura(FacturaDto dto)
        {
            ValidarDto(dto);

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    var nuevaFactura = new Factura
                    {
                        ClienteID = dto.ClienteID,
                        Fecha = DateTime.Now,
                        Estado = "ACTIVA",
                        NumeroFactura = "FAC-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        Subtotal = 0,
                        IVA_Total = 0,
                        Total = 0,
                        Detalles = new List<FacturaDetalle>()
                    };

                    CalcularYProcesarDetalles(nuevaFactura, dto.Detalles);

                    db.Facturas.Add(nuevaFactura);
                    db.SaveChanges();

                    dbTransaction.Commit();
                    return nuevaFactura;
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    // Lanzamos el error interno si existe para debuguear mejor
                    var inner = ex.InnerException != null ? ex.InnerException.Message : "";
                    throw new Exception($"Error al persistir la factura: {ex.Message}. {inner}");
                }
            }
        }

        // --- MÉTODO 2: ACTUALIZAR FACTURA (EDITAR) ---
        public Factura ActualizarFactura(int id, FacturaDto dto)
        {
            ValidarDto(dto);

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Buscamos la factura incluyendo sus detalles actuales
                    var facturaExistente = db.Facturas.Include(f => f.Detalles).FirstOrDefault(f => f.FacturaID == id);

                    if (facturaExistente == null) throw new Exception("La factura no existe.");
                    if (facturaExistente.Estado == "ANULADA") throw new Exception("No se puede editar una factura anulada.");

                    // 1. Actualizar datos básicos de cabecera
                    facturaExistente.ClienteID = dto.ClienteID;

                    // 2. Limpiar detalles antiguos (Eliminación física)
                    db.FacturaDetalles.RemoveRange(facturaExistente.Detalles);
                    facturaExistente.Detalles.Clear();
                    db.SaveChanges(); // Limpiamos antes de reinsertar

                    // 3. Procesar los nuevos detalles y recalcular totales
                    CalcularYProcesarDetalles(facturaExistente, dto.Detalles);

                    db.SaveChanges();
                    dbTransaction.Commit();
                    return facturaExistente;
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    throw new Exception($"Error al actualizar: {ex.Message}");
                }
            }
        }

        // --- MÉTODO 3: ANULAR FACTURA ---
        public void AnularFactura(int id)
        {
            var factura = db.Facturas.Find(id);
            if (factura == null) throw new Exception("Factura no encontrada.");

            factura.Estado = "ANULADA";
            db.SaveChanges();
        }

        // --- MÉTODO PRIVADO: CÁLCULOS DE IVA Y TOTALES (LÓGICA CENTRALIZADA) ---
        private void CalcularYProcesarDetalles(Factura factura, List<DetalleDto> detallesDto)
        {
            decimal totalSubtotal = 0;
            decimal totalIVA = 0;

            foreach (var item in detallesDto)
            {
                if (item.Cantidad <= 0)
                    throw new Exception("La cantidad de cada producto debe ser mayor a cero.");

                var producto = db.Productos.Find(item.ProductoID);
                if (producto == null)
                    throw new Exception($"El producto con ID {item.ProductoID} no existe en la base de datos.");

                decimal precio = producto.PrecioUnitario;
                decimal subtotalLinea = precio * item.Cantidad;
                decimal ivaLinea = subtotalLinea * (producto.IVA_Porcentaje / 100);
                decimal totalLinea = subtotalLinea + ivaLinea;

                var detalleEntity = new FacturaDetalle
                {
                    ProductoID = item.ProductoID,
                    Cantidad = item.Cantidad,
                    PrecioUnitarioHistorico = precio,
                    SubtotalLinea = subtotalLinea,
                    IVALinea = ivaLinea,
                    TotalLinea = totalLinea
                };

                factura.Detalles.Add(detalleEntity);

                totalSubtotal += subtotalLinea;
                totalIVA += ivaLinea;
            }

            // Actualización de la Cabecera
            factura.Subtotal = totalSubtotal;
            factura.IVA_Total = totalIVA;
            factura.Total = totalSubtotal + totalIVA;
        }

        // --- MÉTODO PRIVADO: VALIDACIONES INICIALES ---
        private void ValidarDto(FacturaDto dto)
        {
            if (dto == null) throw new Exception("Los datos de la factura son nulos.");

            // Restricción: No se permiten facturas sin detalles (Página 5)
            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new Exception("No se puede procesar una factura sin productos seleccionados.");

            if (dto.ClienteID <= 0)
                throw new Exception("Debe seleccionar un cliente válido.");
        }
    }
}