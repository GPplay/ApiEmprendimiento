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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protege todo el controlador, requiriendo autenticación JWT para todas las acciones
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(AppDbContext context, ILogger<ProductosController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper para extraer el EmprendimientoId del token JWT del usuario
        private ActionResult<Guid> GetEmprendimientoIdFromToken()
        {
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim))
            {
                _logger.LogWarning("No se encontró el claim 'emprendimientoId' en el token JWT.");
                return Unauthorized(new { message = "No se encontró el EmprendimientoId en el token de autenticación." });
            }

            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El EmprendimientoId '{EmprendimientoId}' en el token es inválido.", emprendimientoIdClaim);
                return Unauthorized(new { message = "El EmprendimientoId proporcionado en el token es inválido." });
            }
            return parsedEmprendimientoId;
        }

        // GET: api/Productos
        // Obtiene la lista de productos de un emprendimiento específico, incluyendo su cantidad en inventario.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProductosDelEmprendimiento()
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            _logger.LogInformation("Obteniendo lista de productos del emprendimiento con ID: {EmprendimientoId}", parsedEmprendimientoId);

            var productosEnInventario = await _context.InventarioProductos
                .Include(ip => ip.Producto)
                .Include(ip => ip.Inventario)
                .Where(ip => ip.Inventario.EmprendimientoId == parsedEmprendimientoId)
                .Select(ip => new
                {
                    ip.Producto.Id,
                    ip.Producto.Nombre,
                    ip.Producto.Descripcion,
                    ip.Producto.CostoFabricacion,
                    ip.Producto.PrecioVenta,
                    ip.Cantidad,
                    ip.FechaActualizacion
                })
                .ToListAsync();

            if (!productosEnInventario.Any())
            {
                _logger.LogInformation("No se encontraron productos en el inventario para el EmprendimientoId: {EmprendimientoId}.", parsedEmprendimientoId);
                return NotFound(new { message = "No se encontraron productos en el inventario para tu emprendimiento." });
            }

            return Ok(productosEnInventario);
        }

        // GET: api/Productos/5
        // Obtiene los detalles de un producto específico, incluyendo su cantidad en inventario.
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetProductoPorId(Guid id)
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            _logger.LogInformation("Obteniendo producto con ID: {ProductoId} para EmprendimientoId: {EmprendimientoId}", id, parsedEmprendimientoId);

            var productoEnInventario = await _context.InventarioProductos
                .Include(ip => ip.Producto)
                .Include(ip => ip.Inventario)
                .Where(ip => ip.ProductoId == id && ip.Inventario.EmprendimientoId == parsedEmprendimientoId)
                .Select(ip => new
                {
                    ip.Producto.Id,
                    ip.Producto.Nombre,
                    ip.Producto.Descripcion,
                    ip.Producto.CostoFabricacion,
                    ip.Producto.PrecioVenta,
                    ip.Cantidad,
                    ip.FechaActualizacion
                })
                .FirstOrDefaultAsync();

            if (productoEnInventario == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Producto con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            return Ok(productoEnInventario);
        }

        // PUT: api/Productos/5
        // Actualiza los detalles de un producto existente.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(Guid id, [FromBody] ProductoUpdateDto dto) // <-- ¡CORREGIDO: Usando ProductoUpdateDto!
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud de actualización de producto para ID: {ProductoId}.", id);
                return BadRequest(ModelState);
            }

            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            if (id != dto.Id)
            {
                _logger.LogWarning("El ID de la ruta ({RouteId}) no coincide con el ID del producto en el cuerpo ({BodyId}).", id, dto.Id);
                return BadRequest(new { message = "El ID del producto en la URL no coincide con el proporcionado en el cuerpo de la solicitud." });
            }

            // Busca el producto existente y verifica que pertenezca al emprendimiento del usuario
            var productoExistente = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.EmprendimientoId == parsedEmprendimientoId);

            if (productoExistente == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId} para actualización.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Producto con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            // Actualiza solo los campos modificables del producto desde el DTO
            productoExistente.Nombre = dto.Nombre;
            productoExistente.Descripcion = dto.Descripcion;
            productoExistente.CostoFabricacion = dto.CostoFabricacion;
            productoExistente.PrecioVenta = dto.PrecioVenta;
            // La fecha de creación no debe cambiarse en una actualización

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto con ID: {ProductoId} actualizado correctamente para EmprendimientoId: {EmprendimientoId}.", id, parsedEmprendimientoId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    _logger.LogWarning("Error de concurrencia: Producto con ID: {ProductoId} no encontrado durante la actualización.", id);
                    return NotFound(new { message = $"Producto con ID {id} no encontrado." });
                }
                else
                {
                    _logger.LogError("Error de concurrencia al actualizar producto con ID: {ProductoId}.", id);
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar producto con ID: {ProductoId}.", id);
                return BadRequest(new { message = "Error al actualizar el producto. Verifica los datos y las restricciones de la base de datos." });
            }

            return NoContent();
        }

        // POST: api/Productos
        // Crea un nuevo producto y su entrada inicial en el inventario del emprendimiento del usuario.
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud de creación de producto.");
                return BadRequest(ModelState);
            }

            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            // 1. Validar que el emprendimiento exista y obtener su inventario
            var emprendimiento = await _context.Emprendimientos
                .Include(e => e.Inventario)
                .FirstOrDefaultAsync(e => e.Id == parsedEmprendimientoId);

            if (emprendimiento == null)
            {
                _logger.LogWarning("Emprendimiento con ID: {EmprendimientoId} no encontrado.", parsedEmprendimientoId);
                return NotFound(new { message = $"Emprendimiento con ID {parsedEmprendimientoId} no encontrado." });
            }

            // 2. Verificar o crear el Inventario del emprendimiento si no existe
            Inventario inventario = emprendimiento.Inventario;
            if (inventario == null)
            {
                _logger.LogInformation("No se encontró inventario para EmprendimientoId: {EmprendimientoId}. Creando uno nuevo.", parsedEmprendimientoId);
                inventario = new Inventario
                {
                    Id = Guid.NewGuid(),
                    EmprendimientoId = parsedEmprendimientoId,
                    FechaActualizacion = DateTimeOffset.UtcNow
                };
                _context.Inventarios.Add(inventario);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inventario creado con ID: {InventarioId} para EmprendimientoId: {EmprendimientoId}.", inventario.Id, parsedEmprendimientoId);
            }

            // 3. Crear el Producto
            var producto = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CostoFabricacion = dto.CostoFabricacion,
                PrecioVenta = dto.PrecioVenta,
                FechaCreacion = DateTimeOffset.UtcNow,
                EmprendimientoId = parsedEmprendimientoId,
                Emprendimiento = emprendimiento, // <-- ¡CORRECCIÓN ANTERIOR APLICADA AQUÍ!
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Producto principal creado con ID: {ProductoId} para EmprendimientoId: {EmprendimientoId}.", producto.Id, parsedEmprendimientoId);


            // 4. Crear la entrada en la tabla de unión InventarioProducto
            var inventarioProducto = new InventarioProducto
            {
                Id = Guid.NewGuid(),
                InventarioId = inventario.Id,
                ProductoId = producto.Id,
                Cantidad = dto.CantidadInicial,
                FechaActualizacion = DateTimeOffset.UtcNow
            };

            _context.InventarioProductos.Add(inventarioProducto);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entrada de InventarioProducto creada con ID: {InventarioProductoId} para ProductoId: {ProductoId} en InventarioId: {InventarioId}.", inventarioProducto.Id, producto.Id, inventario.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar la relación InventarioProducto para el ProductoId: {ProductoId} y EmprendimientoId: {EmprendimientoId}.", producto.Id, parsedEmprendimientoId);
                return StatusCode(500, new { message = "Error al guardar el producto y su relación de inventario. Verifica los datos y las restricciones de la base de datos." });
            }

            return CreatedAtAction(nameof(GetProductoPorId), new { id = producto.Id }, producto);
        }

        // DELETE: api/Productos/5
        // Elimina un producto y sus entradas asociadas en el inventario.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(Guid id)
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            var producto = await _context.Productos
                .Include(p => p.InventarioProductos)
                .FirstOrDefaultAsync(p => p.Id == id && p.EmprendimientoId == parsedEmprendimientoId);

            if (producto == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId} para eliminación.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Producto con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            try
            {
                if (producto.InventarioProductos != null && producto.InventarioProductos.Any())
                {
                    _context.InventarioProductos.RemoveRange(producto.InventarioProductos);
                    _logger.LogInformation("Eliminadas {Count} entradas de InventarioProducto para ProductoId: {ProductoId}.", producto.InventarioProductos.Count, id);
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto con ID: {ProductoId} y sus relaciones de inventario eliminados correctamente para EmprendimientoId: {EmprendimientoId}.", id, parsedEmprendimientoId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar producto con ID: {ProductoId}. Asegúrate de que no haya dependencias externas (ej. DetallesVenta).", id);
                return BadRequest(new { message = "No se puede eliminar el producto debido a restricciones de claves foráneas (por ejemplo, DetallesVenta) o un error de base de datos." });
            }

            return NoContent();
        }

        private bool ProductoExists(Guid id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
