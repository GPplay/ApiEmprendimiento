using ApiEmprendimiento.Context;
using ApiEmprendimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InventariosController> _logger;

        public InventariosController(AppDbContext context, ILogger<InventariosController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventario>>> GetInventarios()
        {
            _logger.LogInformation("Obteniendo lista de inventarios.");
            return await _context.Inventarios
                .Include(i => i.Emprendimiento)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Inventario>> GetInventario(Guid id)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Emprendimiento)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
            {
                _logger.LogWarning("Inventario con ID: {InventarioId} no encontrado.", id);
                return NotFound(new { message = $"Inventario con ID {id} no encontrado." });
            }

            return inventario;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventario(Guid id, Inventario inventario)
        {
            if (id != inventario.Id)
            {
                _logger.LogWarning("El ID proporcionado ({Id}) no coincide con el ID del inventario ({InventarioId}).", id, inventario.Id);
                return BadRequest(new { message = "El ID del inventario no coincide con el proporcionado." });
            }

            _context.Entry(inventario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inventario con ID: {InventarioId} actualizado correctamente.", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventarioExists(id))
                {
                    _logger.LogWarning("Inventario con ID: {InventarioId} no encontrado durante la actualización.", id);
                    return NotFound(new { message = $"Inventario con ID {id} no encontrado." });
                }
                else
                {
                    _logger.LogError("Error de concurrencia al actualizar inventario con ID: {InventarioId}.", id);
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar inventario con ID: {InventarioId}.", id);
                return BadRequest(new { message = "Error al actualizar el inventario. Verifica las claves foráneas." });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventario(Guid id)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Productos)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
            {
                _logger.LogWarning("Inventario con ID: {InventarioId} no encontrado.", id);
                return NotFound(new { message = $"Inventario con ID {id} no encontrado." });
            }

            if (inventario.Productos.Any())
            {
                _logger.LogWarning("No se puede eliminar el inventario con ID: {InventarioId} porque tiene productos asociados.", id);
                return BadRequest(new { message = "No se puede eliminar el inventario porque tiene productos asociados." });
            }

            try
            {
                _context.Inventarios.Remove(inventario);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inventario con ID: {InventarioId} eliminado correctamente.", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar inventario con ID: {InventarioId}.", id);
                return BadRequest(new { message = "Error al eliminar el inventario debido a restricciones de claves foráneas." });
            }

            return NoContent();
        }

        private bool InventarioExists(Guid id)
        {
            return _context.Inventarios.Any(e => e.Id == id);
        }
    }
}