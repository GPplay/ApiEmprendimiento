using ApiEmprendimiento.Context;
using ApiEmprendimiento.Dtos;
using ApiEmprendimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize] // Asegura que solo usuarios autenticados accedan
    public class DetalleVentasController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DetalleVentasController> _logger; // Añadido ILogger

        public DetalleVentasController(AppDbContext context, ILogger<DetalleVentasController> logger) // Inyectar ILogger
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleVenta>>> GetDetallesVenta()
        {
            _logger.LogInformation("Obteniendo todos los detalles de venta.");
            // Considera filtrar esto por EmprendimientoId si solo quieres los del usuario actual
            return await _context.DetallesVenta
                                 .Include(dv => dv.Ventas) // Incluir la Venta padre
                                 .Include(dv => dv.Producto) // Incluir el Producto
                                 .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleVenta>> GetDetalleVenta(Guid id)
        {
            _logger.LogInformation("Obteniendo detalle de venta con ID: {DetalleVentaId}.", id);

            // Obtener el EmprendimientoId del token para filtrar
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            // Busca el detalle de venta y asegúrate de que pertenezca al emprendimiento del usuario
            var detalleVenta = await _context.DetallesVenta
                                .Include(dv => dv.Ventas) // Incluir la Venta padre para el filtro por EmprendimientoId
                                .Include(dv => dv.Producto)
                                .FirstOrDefaultAsync(dv => dv.Id == id && dv.Ventas.EmprendimientoId == parsedEmprendimientoId);

            if (detalleVenta == null)
            {
                _logger.LogWarning("Detalle de venta con ID: {DetalleVentaId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Detalle de venta con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            return Ok(detalleVenta);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalleVenta(Guid id, DetalleVenta detalleVenta)
        {
            _logger.LogInformation("Actualizando detalle de venta con ID: {DetalleVentaId}.", id);

            if (id != detalleVenta.Id)
            {
                _logger.LogWarning("El ID de la ruta ({RouteId}) no coincide con el ID del DetalleVenta en el cuerpo ({BodyId}).", id, detalleVenta.Id);
                return BadRequest(new { message = "El ID del detalle de venta en la URL no coincide con el proporcionado en el cuerpo de la solicitud." });
            }

            // Obtener el EmprendimientoId del token para filtrar
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            // Asegurarse de que el detalle de venta a actualizar pertenezca al emprendimiento del usuario
            var existingDetalleVenta = await _context.DetallesVenta
                                        .Include(dv => dv.Ventas) // Necesario para filtrar por EmprendimientoId
                                        .AsNoTracking() // Importante para evitar problemas de seguimiento
                                        .FirstOrDefaultAsync(dv => dv.Id == id && dv.Ventas.EmprendimientoId == parsedEmprendimientoId);

            if (existingDetalleVenta == null)
            {
                _logger.LogWarning("Detalle de venta con ID: {DetalleVentaId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId} para actualización.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Detalle de venta con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            // El objeto detalleVenta del body tiene que ser válido, y sus propiedades de navegación (Ventas, Producto)
            // deben estar establecidas si son 'required' y si no se van a cargar de la DB.
            // La mejor práctica para PUT sería usar un DTO que solo contenga los campos modificables.
            // Para simplificar aquí, vamos a cargar el existente y actualizarlo.
            existingDetalleVenta = await _context.DetallesVenta
                                        .Include(dv => dv.Ventas)
                                        .Include(dv => dv.Producto)
                                        .FirstOrDefaultAsync(dv => dv.Id == id);

            // Actualizar campos permitidos (ej. Cantidad, Precio - si el precio puede cambiar post-venta)
            existingDetalleVenta.Cantidad = detalleVenta.Cantidad;
            existingDetalleVenta.Precio = detalleVenta.Precio;
            existingDetalleVenta.FechaCreacion = DateTimeOffset.UtcNow; // O ajustar si la fecha de creación no debe cambiar

            _context.Entry(existingDetalleVenta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Detalle de venta con ID: {DetalleVentaId} actualizado correctamente.", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetalleVentaExists(id))
                {
                    _logger.LogWarning("Error de concurrencia: Detalle de venta con ID: {DetalleVentaId} no encontrado durante la actualización.", id);
                    return NotFound(new { message = $"Detalle de venta con ID {id} no encontrado." });
                }
                else
                {
                    _logger.LogError("Error de concurrencia al actualizar detalle de venta con ID: {DetalleVentaId}.", id);
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar detalle de venta con ID: {DetalleVentaId}.", id);
                return BadRequest(new { message = "Error al actualizar el detalle de venta. Verifica los datos y las restricciones de la base de datos." });
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<DetalleVenta>> PostDetalleVenta(DetallesVetnasCreateDTO dto)
        {
            _logger.LogInformation("Intentando crear un nuevo detalle de venta.");

            // Validar ModelState del DTO
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud POST de DetalleVenta.");
                return BadRequest(ModelState);
            }

            // Obtener el ID del emprendimiento del usuario autenticado
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            // 1. Obtener la Venta padre (usando VentaId del DTO)
            var ventaPadre = await _context.Ventas
                                    .Include(v => v.Emprendimiento)
                                    .FirstOrDefaultAsync(v => v.Id == dto.VentaId && v.EmprendimientoId == parsedEmprendimientoId);

            if (ventaPadre == null)
            {
                _logger.LogWarning("Venta con ID: {VentaId} no encontrada o no pertenece al EmprendimientoId {EmprendimientoId}.", dto.VentaId, parsedEmprendimientoId);
                return NotFound(new { message = $"Venta con ID {dto.VentaId} no encontrada o no pertenece a tu emprendimiento." });
            }

            // 2. Obtener el Producto asociado (usando ProductoId del DTO)
            var productoAsociado = await _context.Productos
                                        .FirstOrDefaultAsync(p => p.Id == dto.ProductoId && p.EmprendimientoId == parsedEmprendimientoId);

            if (productoAsociado == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId}.", dto.ProductoId, parsedEmprendimientoId);
                return NotFound(new { message = $"Producto con ID {dto.ProductoId} no encontrado o no pertenece a tu emprendimiento." });
            }

            // Crear el registro de DetalleVenta, asignando las propiedades de navegación requeridas
            var detalleVenta = new DetalleVenta
            {
                Id = Guid.NewGuid(),
                VentaId = dto.VentaId,
                Ventas = ventaPadre, // Asignar el objeto Venta completo
                ProductoId = dto.ProductoId,
                Producto = productoAsociado, // Asignar el objeto Producto completo
                Cantidad = dto.Cantidad,
                Precio = dto.Precio,
                FechaCreacion = DateTimeOffset.UtcNow // Usar FechaCreacion, como en tu modelo
            };

            _context.DetallesVenta.Add(detalleVenta);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Detalle de venta con ID: {DetalleVentaId} creado correctamente para VentaId: {VentaId}.", detalleVenta.Id, detalleVenta.VentaId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar el detalle de venta para ProductoId: {ProductoId} y VentaId: {VentaId}.", dto.ProductoId, dto.VentaId);
                return StatusCode(500, new { message = "Error al guardar el detalle de venta. Verifica los datos y las restricciones de la base de datos." });
            }

            return CreatedAtAction(nameof(GetDetalleVenta), new { id = detalleVenta.Id }, detalleVenta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalleVenta(Guid id)
        {
            _logger.LogInformation("Eliminando detalle de venta con ID: {DetalleVentaId}.", id);

            // Obtener el EmprendimientoId del token para filtrar
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            var detalleVenta = await _context.DetallesVenta
                                .Include(dv => dv.Ventas) // Necesario para filtrar por EmprendimientoId
                                .FirstOrDefaultAsync(dv => dv.Id == id && dv.Ventas.EmprendimientoId == parsedEmprendimientoId);

            if (detalleVenta == null)
            {
                _logger.LogWarning("Detalle de venta con ID: {DetalleVentaId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId} para eliminación.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Detalle de venta con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            _context.DetallesVenta.Remove(detalleVenta);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Detalle de venta con ID: {DetalleVentaId} eliminado correctamente.", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar detalle de venta con ID: {DetalleVentaId}.", id);
                return BadRequest(new { message = "Error al eliminar el detalle de venta debido a restricciones de base de datos." });
            }

            return NoContent();
        }

        private bool DetalleVentaExists(Guid id)
        {
            return _context.DetallesVenta.Any(e => e.Id == id);
        }
    }
}
