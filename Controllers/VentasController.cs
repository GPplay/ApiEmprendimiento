using ApiEmprendimiento.Context;
using ApiEmprendimiento.Dtos;
using ApiEmprendimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Asegura que solo usuarios autenticados puedan acceder a este controlador
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VentasController> _logger;

        public VentasController(AppDbContext context, ILogger<VentasController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper para extraer el EmprendimientoId del token JWT del usuario
        private ActionResult<Guid> GetEmprendimientoIdFromToken()
        {
            var emprendimientoIdClaim = User.FindFirst("EmprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim))
            {
                _logger.LogWarning("No se encontró el claim 'EmprendimientoId' en el token JWT.");
                return Unauthorized(new { message = "No se encontró el EmprendimientoId en el token de autenticación." });
            }

            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El EmprendimientoId '{EmprendimientoId}' en el token es inválido.", emprendimientoIdClaim);
                return Unauthorized(new { message = "El EmprendimientoId proporcionado en el token es inválido." });
            }
            return parsedEmprendimientoId;
        }

        // POST: api/Ventas
        // Registra una nueva venta, descontando los productos del inventario.
        // ¡ACTUALIZADO PARA REGISTRAR GANANCIAS POR VENTAS MENSUALES!
        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] VentaCreateDto ventaDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud de registro de venta.");
                return BadRequest(ModelState);
            }

            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            // Obtener el objeto Emprendimiento para asignarlo a la Venta y al ReporteFinancieroMensual
            var emprendimiento = await _context.Emprendimientos
                                        .FirstOrDefaultAsync(e => e.Id == parsedEmprendimientoId);

            if (emprendimiento == null)
            {
                _logger.LogWarning("Emprendimiento con ID: {EmprendimientoId} no encontrado.", parsedEmprendimientoId);
                return NotFound(new { message = $"Emprendimiento con ID {parsedEmprendimientoId} no encontrado." });
            }

            // Iniciar una transacción de base de datos para asegurar la atomicidad de la venta y la actualización del inventario
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var nuevaVenta = new Venta
                {
                    Id = Guid.NewGuid(),
                    FechaVenta = DateTimeOffset.UtcNow,
                    EmprendimientoId = parsedEmprendimientoId,
                    Emprendimiento = emprendimiento,
                    TotalVenta = 0 // Se calculará en el bucle
                };

                _context.Ventas.Add(nuevaVenta);
                // Guardar la venta antes de procesar los detalles si es necesario para tener su ID
                await _context.SaveChangesAsync();

                decimal totalCalculado = 0;
                var detallesVenta = new List<DetalleVenta>();

                foreach (var itemDto in ventaDto.Items)
                {
                    // 1. Obtener el producto y su entrada en InventarioProducto
                    var inventarioProducto = await _context.InventarioProductos
                        .Include(ip => ip.Producto) // Necesario para CostoFabricacion y PrecioVenta
                        .Include(ip => ip.Inventario)
                        .FirstOrDefaultAsync(ip => ip.ProductoId == itemDto.ProductoId && ip.Inventario.EmprendimientoId == parsedEmprendimientoId);

                    if (inventarioProducto == null)
                    {
                        _logger.LogWarning("Producto con ID: {ProductoId} no encontrado en el inventario del emprendimiento {EmprendimientoId}.", itemDto.ProductoId, parsedEmprendimientoId);
                        await transaction.RollbackAsync();
                        return NotFound(new { message = $"El producto con ID {itemDto.ProductoId} no se encontró en tu inventario." });
                    }

                    if (inventarioProducto.Cantidad < itemDto.Cantidad)
                    {
                        _logger.LogWarning("Stock insuficiente para el producto {ProductoNombre} (ID: {ProductoId}). Cantidad solicitada: {Solicitada}, Disponible: {Disponible}.", inventarioProducto.Producto.Nombre, itemDto.ProductoId, itemDto.Cantidad, inventarioProducto.Cantidad);
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Stock insuficiente para el producto '{inventarioProducto.Producto.Nombre}'. Cantidad disponible: {inventarioProducto.Cantidad}." });
                    }

                    // 2. Descontar la cantidad del inventario
                    inventarioProducto.Cantidad -= itemDto.Cantidad;
                    inventarioProducto.FechaActualizacion = DateTimeOffset.UtcNow; // Actualizar fecha de stock

                    // 3. ¡NUEVO! Recalcular CostoActualEnStock después de cambiar la cantidad
                    // Esto se mantiene, aunque el TotalGastosFabricacionMes sea la métrica clave para la gráfica
                    inventarioProducto.CostoActualEnStock = inventarioProducto.Cantidad * inventarioProducto.Producto.CostoFabricacion;

                    // 4. Crear el detalle de la venta
                    var detalle = new DetalleVenta
                    {
                        Id = Guid.NewGuid(),
                        VentaId = nuevaVenta.Id,
                        Ventas = nuevaVenta,
                        ProductoId = itemDto.ProductoId,
                        Producto = inventarioProducto.Producto,
                        Cantidad = itemDto.Cantidad,
                        Precio = inventarioProducto.Producto.PrecioVenta, // Usar el precio de venta actual del producto
                        FechaCreacion = DateTimeOffset.UtcNow
                    };
                    detallesVenta.Add(detalle);
                    totalCalculado += detalle.Cantidad * detalle.Precio;
                }

                _context.DetallesVenta.AddRange(detallesVenta);
                nuevaVenta.TotalVenta = totalCalculado; // Asignar el total calculado a la venta
                _context.Ventas.Update(nuevaVenta); // Marcar la venta para actualización con el total

                // 5. ¡NUEVO! Actualizar el TotalGananciasVentasMes en ReporteFinancieroMensual
                var ahora = DateTimeOffset.UtcNow;
                var anoActual = ahora.Year;
                var mesActual = ahora.Month;

                var reporteMensual = await _context.ReportesFinancierosMensuales
                    .FirstOrDefaultAsync(r => r.EmprendimientoId == parsedEmprendimientoId && r.Ano == anoActual && r.Mes == mesActual);

                if (reporteMensual == null)
                {
                    reporteMensual = new ReporteFinancieroMensual
                    {
                        Id = Guid.NewGuid(),
                        EmprendimientoId = parsedEmprendimientoId,
                        Emprendimiento = emprendimiento, // Asignar la propiedad de navegación
                        Ano = anoActual,
                        Mes = mesActual,
                        TotalGastosFabricacionMes = 0, // Se inicializa en 0
                        TotalGananciasVentasMes = 0, // Se inicializa en 0
                        FechaUltimaActualizacion = ahora
                    };
                    _context.ReportesFinancierosMensuales.Add(reporteMensual);
                    _logger.LogInformation("Creado nuevo ReporteFinancieroMensual para EmprendimientoId: {EmprendimientoId}, Año: {Ano}, Mes: {Mes}.", parsedEmprendimientoId, anoActual, mesActual);
                }

                // Sumar el total de la venta al total de ganancias del mes
                reporteMensual.TotalGananciasVentasMes += nuevaVenta.TotalVenta;
                reporteMensual.FechaUltimaActualizacion = ahora; // Actualizar la fecha de actualización

                _context.ReportesFinancierosMensuales.Update(reporteMensual); // Marcar para actualización

                await _context.SaveChangesAsync(); // Guardar todos los cambios: DetalleVenta, InventarioProducto y ReporteFinancieroMensual

                await transaction.CommitAsync(); // Confirmar la transacción
                _logger.LogInformation("Venta con ID: {VentaId} registrada correctamente y ganancias actualizadas para EmprendimientoId: {EmprendimientoId}. Total: {TotalVenta}", nuevaVenta.Id, parsedEmprendimientoId, nuevaVenta.TotalVenta);

                return CreatedAtAction(nameof(GetVentaPorId), new { id = nuevaVenta.Id }, new { ventaId = nuevaVenta.Id, total = nuevaVenta.TotalVenta, fecha = nuevaVenta.FechaVenta });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Revertir la transacción si algo falla
                _logger.LogError(ex, "Error al registrar la venta y actualizar los reportes financieros para EmprendimientoId: {EmprendimientoId}.", parsedEmprendimientoId);
                return StatusCode(500, new { message = "Ocurrió un error al procesar la venta y registrar las ganancias. La transacción ha sido revertida." });
            }
        }

        // GET: api/Ventas
        // Obtiene todas las ventas para el emprendimiento del usuario autenticado.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetVentasDelEmprendimiento()
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            _logger.LogInformation("Solicitud para obtener ventas para EmprendimientoId: {EmprendimientoId}", parsedEmprendimientoId);

            var ventas = await _context.Ventas
                .Where(v => v.EmprendimientoId == parsedEmprendimientoId)
                .OrderByDescending(v => v.FechaVenta) // Ordenar por fecha las más recientes primero
                .Select(v => new
                {
                    v.Id,
                    v.FechaVenta,
                    v.TotalVenta,
                    v.EmprendimientoId,
                    CantidadDetalles = v.DetallesVenta.Count // Opcional: número de ítems en la venta
                })
                .ToListAsync();

            if (!ventas.Any())
            {
                _logger.LogInformation("No se encontraron ventas para EmprendimientoId: {EmprendimientoId}.", parsedEmprendimientoId);
                return NotFound(new { message = "No se encontraron ventas para tu emprendimiento." });
            }

            return Ok(ventas);
        }

        // GET: api/Ventas/5
        // Obtiene los detalles de una venta específica para el emprendimiento del usuario.
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetVentaPorId(Guid id)
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            _logger.LogInformation("Solicitud para obtener venta con ID: {VentaId} para EmprendimientoId: {EmprendimientoId}", id, parsedEmprendimientoId);

            var venta = await _context.Ventas
                .Include(v => v.DetallesVenta)
                .ThenInclude(dv => dv.Producto) // Incluir los detalles del producto en cada detalle de venta
                .Where(v => v.Id == id && v.EmprendimientoId == parsedEmprendimientoId)
                .Select(v => new
                {
                    v.Id,
                    v.FechaVenta,
                    v.TotalVenta,
                    v.EmprendimientoId,
                    Items = v.DetallesVenta.Select(dv => new
                    {
                        dv.Id,
                        ProductoId = dv.Producto.Id,
                        ProductoNombre = dv.Producto.Nombre,
                        CantidadVendida = dv.Cantidad,
                        PrecioUnitario = dv.Precio,
                        Subtotal = dv.Cantidad * dv.Precio
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (venta == null)
            {
                _logger.LogWarning("Venta con ID: {VentaId} no encontrada o no pertenece al EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Venta con ID {id} no encontrada o no pertenece a tu emprendimiento." });
            }

            return Ok(venta);
        }

        // DELETE: api/Ventas/5
        // Elimina una venta y revierte el stock de los productos.
        // CUIDADO: La eliminación de ventas podría tener implicaciones contables.
        // No afecta los reportes financieros mensuales una vez registrados.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(Guid id)
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.DetallesVenta)
                    .ThenInclude(dv => dv.Producto) // Cargar productos para acceder a inventarioProducto
                    .Where(v => v.Id == id && v.EmprendimientoId == parsedEmprendimientoId)
                    .FirstOrDefaultAsync();

                if (venta == null)
                {
                    _logger.LogWarning("Venta con ID: {VentaId} no encontrada o no pertenece al EmprendimientoId {EmprendimientoId} para eliminación.", id, parsedEmprendimientoId);
                    await transaction.RollbackAsync();
                    return NotFound(new { message = $"Venta con ID {id} no encontrada o no pertenece a tu emprendimiento." });
                }

                // Revertir el stock de cada producto en la venta
                foreach (var detalle in venta.DetallesVenta)
                {
                    var inventarioProducto = await _context.InventarioProductos
                        .Include(ip => ip.Inventario)
                        .Include(ip => ip.Producto) // Necesario para CostoFabricacion
                        .FirstOrDefaultAsync(ip => ip.ProductoId == detalle.ProductoId && ip.Inventario.EmprendimientoId == parsedEmprendimientoId);

                    if (inventarioProducto != null)
                    {
                        inventarioProducto.Cantidad += detalle.Cantidad; // Sumar la cantidad vendida de nuevo al stock
                        inventarioProducto.FechaActualizacion = DateTimeOffset.UtcNow; // Actualizar fecha de stock

                        // Recalcular CostoActualEnStock
                        inventarioProducto.CostoActualEnStock = inventarioProducto.Cantidad * inventarioProducto.Producto.CostoFabricacion;
                    }
                    else
                    {
                        _logger.LogError("Error de consistencia de datos: InventarioProducto no encontrado para ProductoId: {ProductoId} al revertir venta {VentaId}.", detalle.ProductoId, venta.Id);
                    }
                }

                // NOTA: La eliminación de una venta NO revierte los ReporteFinancieroMensual.
                // Esto es consistente con la lógica de que los gastos/ganancias son "incurridos" en el mes.
                // Si se necesita revertir los reportes, se necesitaría lógica adicional aquí.

                _context.Ventas.Remove(venta); // Eliminar la venta y sus detalles (debido a OnDelete(Cascade))
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _logger.LogInformation("Venta con ID: {VentaId} eliminada correctamente y stock revertido para EmprendimientoId: {EmprendimientoId}.", id, parsedEmprendimientoId);
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al eliminar la venta con ID: {VentaId} o al revertir el stock.", id);
                return StatusCode(500, new { message = "Ocurrió un error al eliminar la venta y revertir el stock. La transacción ha sido revertida." });
            }
        }
    }
}
