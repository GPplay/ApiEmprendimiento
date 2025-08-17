using ApiEmprendimiento.Context;
using ApiEmprendimiento.Dtos;
using ApiEmprendimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Agregado para logging
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductosController> _logger; // Agregado para logging

        public ProductosController(AppDbContext context, ILogger<ProductosController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/Productos
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetProducto()
        {
            _logger.LogInformation("Obteniendo lista de productos del usuario logueado.");

            // Extraer el claim de EmprendimientoId del JWT
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim))
            {
                _logger.LogWarning("No se encontró el claim 'emprendimientoId' en el token JWT.");
                return Unauthorized(new { message = "No se encontró el EmprendimientoId en el token." });
            }

            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El EmprendimientoId '{EmprendimientoId}' en el token es inválido.", emprendimientoIdClaim);
                return Unauthorized(new { message = "EmprendimientoId inválido en el token." });
            }

            // Consultar solo productos de ese emprendimiento
            var productos = await _context.Productos
                .Where(p => p.EmprendimientoId == parsedEmprendimientoId)
                .Select(p => new
                {
                    p.Nombre,
                    p.Descripcion,
                    p.CostoFabricacion,
                    p.PrecioVenta
                })
                .ToListAsync();

            return productos;
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(Guid id)
        {
            _logger.LogInformation("Obteniendo producto con ID: {ProductoId}", id);
            var producto = await _context.Productos
                .Include(p => p.Emprendimiento)
                .Include(p => p.Inventario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado.", id);
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });
            }

            return producto;
        }

        // PUT: api/Productos/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(Guid id, [FromBody] Producto producto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud de actualización de producto.");
                return BadRequest(ModelState);
            }

            // Extraer el EmprendimientoId del token JWT
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim))
            {
                _logger.LogWarning("No se encontró el claim 'emprendimientoId' en el token JWT.");
                return Unauthorized(new { message = "No se encontró el EmprendimientoId en el token." });
            }

            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El EmprendimientoId '{EmprendimientoId}' en el token es inválido.", emprendimientoIdClaim);
                return Unauthorized(new { message = "EmprendimientoId inválido en el token." });
            }

            if (id != producto.Id)
            {
                _logger.LogWarning("El ID proporcionado ({Id}) no coincide con el ID del producto ({ProductoId}).", id, producto.Id);
                return BadRequest(new { message = "El ID del producto no coincide con el proporcionado." });
            }

            // Verificar que el producto exista y pertenezca al emprendimiento del usuario
            var productoExistente = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.EmprendimientoId == parsedEmprendimientoId);

            if (productoExistente == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado o no pertenece al EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Producto con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            // Actualizar solo los campos permitidos
            productoExistente.Nombre = producto.Nombre;
            productoExistente.Descripcion = producto.Descripcion;
            productoExistente.CostoFabricacion = producto.CostoFabricacion;
            productoExistente.PrecioVenta = producto.PrecioVenta;
            productoExistente.FechaCreacion = productoExistente.FechaCreacion; // no lo cambiamos

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto con ID: {ProductoId} actualizado correctamente para EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Error de concurrencia al actualizar producto con ID: {ProductoId}.", id);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar producto con ID: {ProductoId}.", id);
                return BadRequest(new { message = "Error al actualizar el producto. Verifica las claves foráneas y los datos proporcionados." });
            }

            return NoContent();
        }

        // POST: api/Productos
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud de creación de producto.");
                return BadRequest(ModelState);
            }

            // Extraer el claim de EmprendimientoId del JWT
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim))
            {
                _logger.LogWarning("No se encontró el claim 'emprendimientoId' en el token JWT.");
                return Unauthorized(new { message = "No se encontró el EmprendimientoId en el token." });
            }

            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El EmprendimientoId '{EmprendimientoId}' en el token es inválido.", emprendimientoIdClaim);
                return Unauthorized(new { message = "EmprendimientoId inválido en el token." });
            }

            // Validar que el emprendimiento exista
            var emprendimiento = await _context.Emprendimientos
                .Include(e => e.Inventario)
                .FirstOrDefaultAsync(e => e.Id == parsedEmprendimientoId);

            if (emprendimiento == null)
            {
                _logger.LogWarning("Emprendimiento con ID: {EmprendimientoId} no encontrado.", parsedEmprendimientoId);
                return NotFound(new { message = $"Emprendimiento con ID {parsedEmprendimientoId} no encontrado." });
            }

            // Verificar o crear inventario
            Inventario inventario = emprendimiento.Inventario;
            if (inventario == null)
            {
                _logger.LogInformation("No se encontró inventario para EmprendimientoId: {EmprendimientoId}. Creando uno nuevo.", parsedEmprendimientoId);
                inventario = new Inventario
                {
                    Id = Guid.NewGuid(),
                    EmprendimientoId = parsedEmprendimientoId,
                    Emprendimiento = emprendimiento,
                    Cantidad = 0,
                    FechaActualizacion = DateTimeOffset.UtcNow
                };
                _context.Inventarios.Add(inventario);
                await _context.SaveChangesAsync();
            }

            // Crear el producto
            var producto = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CostoFabricacion = dto.CostoFabricacion,
                PrecioVenta = dto.PrecioVenta,
                FechaCreacion = DateTimeOffset.UtcNow,
                EmprendimientoId = parsedEmprendimientoId,
                Emprendimiento = emprendimiento,
                InventarioId = inventario.Id,
                Inventario = inventario
            };

            try
            {
                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto creado con ID: {ProductoId} para EmprendimientoId: {EmprendimientoId}.", producto.Id, parsedEmprendimientoId);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar el producto para EmprendimientoId: {EmprendimientoId}.", parsedEmprendimientoId);
                return StatusCode(500, new { message = "Error al guardar el producto. Verifica los datos y las restricciones de la base de datos." });
            }

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(Guid id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                _logger.LogWarning("Producto con ID: {ProductoId} no encontrado para eliminación.", id);
                return NotFound(new { message = $"Producto con ID {id} no encontrado." });
            }

            try
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Producto con ID: {ProductoId} eliminado correctamente.", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar producto con ID: {ProductoId}.", id);
                return BadRequest(new { message = "No se puede eliminar el producto debido a restricciones de claves foráneas (por ejemplo, DetallesVenta)." });
            }

            return NoContent();
        }

        private bool ProductoExists(Guid id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}