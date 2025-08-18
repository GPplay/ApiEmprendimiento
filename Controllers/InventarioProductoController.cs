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
using System.Threading.Tasks;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")] // Define la ruta base para este controlador
    [ApiController] // Indica que es un controlador de API
    [Authorize] // Asegura que solo usuarios autenticados puedan acceder a este controlador
    public class InventarioProductoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InventarioProductoController> _logger;

        public InventarioProductoController(AppDbContext context, ILogger<InventarioProductoController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper para extraer el EmprendimientoId del token JWT del usuario
        private ActionResult<Guid> GetEmprendimientoIdFromToken()
        {
            // Busca el claim "emprendimientoId" en el token JWT del usuario autenticado
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;

            // Si el claim no se encuentra o está vacío, el usuario no está autorizado
            if (string.IsNullOrEmpty(emprendimientoIdClaim))
            {
                _logger.LogWarning("No se encontró el claim 'emprendimientoId' en el token JWT para la solicitud.");
                return Unauthorized(new { message = "No se encontró el EmprendimientoId en el token de autenticación." });
            }

            // Intenta convertir el valor del claim a un GUID
            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El EmprendimientoId '{EmprendimientoId}' en el token es inválido.", emprendimientoIdClaim);
                return Unauthorized(new { message = "El EmprendimientoId proporcionado en el token es inválido." });
            }
            return parsedEmprendimientoId;
        }

        // GET: api/InventarioProducto
        // Este endpoint devuelve una lista de todos los productos en el inventario del emprendimiento del usuario.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetInventarioProductosPorEmprendimiento()
        {
            // Obtener el EmprendimientoId del token del usuario actual
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            // Si el helper devuelve un resultado de error (ej. Unauthorized), retornarlo inmediatamente
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            _logger.LogInformation("Solicitud para obtener InventarioProductos para EmprendimientoId: {EmprendimientoId}", parsedEmprendimientoId);

            // Consulta la tabla InventarioProductos, incluyendo las relaciones con Inventario y Producto.
            // Se filtra para asegurar que solo se obtengan los registros que pertenecen al Inventario
            // asociado al Emprendimiento del usuario autenticado.
            var productosEnInventario = await _context.InventarioProductos
                .Include(ip => ip.Producto) // Incluye los detalles del producto
                .Include(ip => ip.Inventario) // Incluye los detalles del inventario para el filtro
                .Where(ip => ip.Inventario.EmprendimientoId == parsedEmprendimientoId) // **Filtra por el EmprendimientoId del token**
                .Select(ip => new // Proyecta los resultados a un objeto anónimo con solo los campos solicitados
                {
                    // ip.Id, // Se ha eliminado este campo de la proyección según la solicitud
                    ProductoNombre = ip.Producto.Nombre, // Nombre del producto
                    CantidadEnStock = ip.Cantidad, // La cantidad actual del producto en el inventario
                    EmprendimientoId = ip.Inventario.EmprendimientoId, // ID del emprendimiento
                    InventarioId = ip.Inventario.Id // ID del inventario
                    // Otros campos como Descripcion, CostoFabricacion, PrecioVenta, UltimaActualizacionStock
                    // han sido eliminados de la proyección.
                })
                .ToListAsync();

            // Si no se encuentran productos después del filtro, devuelve un 404 Not Found
            if (!productosEnInventario.Any())
            {
                _logger.LogInformation("No se encontraron productos en el inventario para el EmprendimientoId: {EmprendimientoId}.", parsedEmprendimientoId);
                return NotFound(new { message = "No se encontraron productos en el inventario para tu emprendimiento." });
            }

            // Devuelve la lista de productos con sus cantidades en el inventario
            return Ok(productosEnInventario);
        }

        // GET: api/InventarioProducto/5 (Obtener detalles de una entrada específica de InventarioProducto)
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetInventarioProducto(Guid id)
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            _logger.LogInformation("Solicitud para obtener InventarioProducto con ID: {InventarioProductoId} para EmprendimientoId: {EmprendimientoId}", id, parsedEmprendimientoId);

            // Busca el InventarioProducto por su ID, asegurándose de que pertenezca al emprendimiento del usuario.
            var inventarioProducto = await _context.InventarioProductos
                .Include(ip => ip.Producto)
                .Include(ip => ip.Inventario)
                .Where(ip => ip.Id == id && ip.Inventario.EmprendimientoId == parsedEmprendimientoId)
                .Select(ip => new // Proyección simplificada para el detalle individual
                {
                    ip.Id, // El ID de InventarioProducto sigue siendo útil para este endpoint individual
                    ProductoId = ip.Producto.Id, // ID del producto
                    ProductoNombre = ip.Producto.Nombre, // Nombre del producto
                    CantidadEnStock = ip.Cantidad, // Cantidad en stock
                    EmprendimientoId = ip.Inventario.EmprendimientoId, // ID del emprendimiento
                    InventarioId = ip.Inventario.Id, // ID del inventario
                    UltimaActualizacionStock = ip.FechaActualizacion // Fecha de actualización
                })
                .FirstOrDefaultAsync();

            if (inventarioProducto == null)
            {
                _logger.LogWarning("InventarioProducto con ID: {InventarioProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"InventarioProducto con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            return Ok(inventarioProducto);
        }

        // POST: api/InventarioProducto
        // Crea una nueva entrada de InventarioProducto. Útil para añadir stock de un producto existente si no está ya en el inventario.
        [HttpPost]
        public async Task<ActionResult<InventarioProducto>> PostInventarioProducto([FromBody] InventarioProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud POST de InventarioProducto.");
                return BadRequest(ModelState);
            }

            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            // 1. Obtener el inventario del emprendimiento del usuario
            var inventarioDelEmprendimiento = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.EmprendimientoId == parsedEmprendimientoId);

            if (inventarioDelEmprendimiento == null)
            {
                _logger.LogWarning("No se encontró inventario para el EmprendimientoId: {EmprendimientoId} del usuario autenticado.", parsedEmprendimientoId);
                return NotFound(new { message = "No se encontró un inventario asociado a tu emprendimiento." });
            }

            // 2. Validar que el producto exista y pertenezca al emprendimiento del usuario
            var productoExistente = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == dto.ProductoId && p.EmprendimientoId == parsedEmprendimientoId);

            if (productoExistente == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId}.", dto.ProductoId, parsedEmprendimientoId);
                return NotFound(new { message = $"Producto con ID {dto.ProductoId} no encontrado o no pertenece a tu emprendimiento." });
            }

            // 3. Verificar si ya existe una entrada para este producto en este inventario
            var existingInventarioProducto = await _context.InventarioProductos
                .FirstOrDefaultAsync(ip => ip.InventarioId == inventarioDelEmprendimiento.Id && ip.ProductoId == dto.ProductoId);

            if (existingInventarioProducto != null)
            {
                _logger.LogWarning("Ya existe una entrada de InventarioProducto para ProductoId: {ProductoId} en InventarioId: {InventarioId}. Redirigiendo a PUT para actualizar la cantidad.", dto.ProductoId, inventarioDelEmprendimiento.Id);
                return Conflict(new { message = $"Este producto (ID: {dto.ProductoId}) ya está registrado en el inventario. Por favor, utiliza el método PUT en '/api/InventarioProducto/{existingInventarioProducto.Id}' para actualizar su cantidad." });
            }

            // 4. Crear la nueva entrada InventarioProducto
            var newInventarioProducto = new InventarioProducto
            {
                Id = Guid.NewGuid(),
                InventarioId = inventarioDelEmprendimiento.Id,
                ProductoId = dto.ProductoId,
                Cantidad = dto.Cantidad,
                FechaActualizacion = DateTimeOffset.UtcNow
            };

            _context.InventarioProductos.Add(newInventarioProducto);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("InventarioProducto creado con ID: {InventarioProductoId} para ProductoId: {ProductoId} en InventarioId: {InventarioId}.", newInventarioProducto.Id, dto.ProductoId, inventarioDelEmprendimiento.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar InventarioProducto para ProductoId: {ProductoId} en InventarioId: {InventarioId}.", dto.ProductoId, inventarioDelEmprendimiento.Id);
                return StatusCode(500, new { message = "Error al guardar el producto en el inventario. Verifica los datos y las restricciones de la base de datos." });
            }

            return CreatedAtAction(nameof(GetInventarioProducto), new { id = newInventarioProducto.Id }, newInventarioProducto);
        }

        // PUT: api/InventarioProducto/5
        // Actualiza la cantidad de un producto específico en el inventario.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventarioProducto(Guid id, [FromBody] InventarioProductoUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud PUT de InventarioProducto para ID: {InventarioProductoId}.", id);
                return BadRequest(ModelState);
            }

            if (id != dto.Id)
            {
                _logger.LogWarning("El ID de la ruta ({RouteId}) no coincide con el ID del InventarioProducto en el cuerpo ({BodyId}).", id, dto.Id);
                return BadRequest(new { message = "El ID de la entrada de inventario en la URL no coincide con el proporcionado en el cuerpo de la solicitud." });
            }

            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            var existingInventarioProducto = await _context.InventarioProductos
                .Include(ip => ip.Inventario)
                .FirstOrDefaultAsync(ip => ip.Id == id && ip.Inventario.EmprendimientoId == parsedEmprendimientoId);

            if (existingInventarioProducto == null)
            {
                _logger.LogWarning("InventarioProducto con ID: {InventarioProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId} para actualización.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"InventarioProducto con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            existingInventarioProducto.Cantidad = dto.Cantidad;
            existingInventarioProducto.FechaActualizacion = DateTimeOffset.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("InventarioProducto con ID: {InventarioProductoId} actualizado correctamente. Nueva cantidad: {NuevaCantidad}.", id, dto.Cantidad);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventarioProductoExists(id))
                {
                    _logger.LogWarning("Error de concurrencia: InventarioProducto con ID: {InventarioProductoId} no encontrado durante la actualización.", id);
                    return NotFound(new { message = $"InventarioProducto con ID {id} no encontrado." });
                }
                else
                {
                    _logger.LogError("Error de concurrencia al actualizar InventarioProducto con ID: {InventarioProductoId}.", id);
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar InventarioProducto con ID: {InventarioProductoId}. Datos: Cantidad={Cantidad}.", id, dto.Cantidad);
                return BadRequest(new { message = "Error al actualizar la cantidad del producto en el inventario. Verifica los datos proporcionados." });
            }

            return NoContent();
        }

        // DELETE: api/InventarioProducto/5
        // Elimina una entrada específica de InventarioProducto (elimina un producto del inventario).
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventarioProducto(Guid id)
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            var inventarioProducto = await _context.InventarioProductos
                .Include(ip => ip.Inventario)
                .FirstOrDefaultAsync(ip => ip.Id == id && ip.Inventario.EmprendimientoId == parsedEmprendimientoId);

            if (inventarioProducto == null)
            {
                _logger.LogWarning("InventarioProducto con ID: {InventarioProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId} para eliminación.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"InventarioProducto con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            _context.InventarioProductos.Remove(inventarioProducto);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("InventarioProducto con ID: {InventarioProductoId} eliminado correctamente para EmprendimientoId: {EmprendimientoId}.", id, parsedEmprendimientoId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar InventarioProducto con ID: {InventarioProductoId}.", id);
                return BadRequest(new { message = "Error al eliminar el producto del inventario debido a restricciones de base de datos." });
            }

            return NoContent();
        }

        // Método auxiliar para verificar si existe una entrada de InventarioProducto
        private bool InventarioProductoExists(Guid id)
        {
            return _context.InventarioProductos.Any(e => e.Id == id);
        }
    }
}
