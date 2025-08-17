using ApiEmprendimiento.Context;
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

        // GET: api/Inventarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetInventarios()
        {
            // Obtener claim de EmprendimientoId
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim) || !Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El token no contiene un EmprendimientoId válido.");
                return Unauthorized(new { message = "No se encontró un EmprendimientoId válido en el token." });
            }

            _logger.LogInformation("Obteniendo inventarios para EmprendimientoId: {EmprendimientoId}", parsedEmprendimientoId);

            var inventarios = await _context.Inventarios
                .Where(i => i.EmprendimientoId == parsedEmprendimientoId)
                .Select(i => new
                {
                    i.Id,
                    i.Cantidad,
                    i.FechaActualizacion
                })
                .ToListAsync();

            return inventarios;
        }

        // GET: api/Inventarios/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetInventario(Guid id)
        {
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim) || !Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                return Unauthorized(new { message = "No se encontró un EmprendimientoId válido en el token." });
            }

            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.Id == id && i.EmprendimientoId == parsedEmprendimientoId);

            if (inventario == null)
            {
                _logger.LogWarning("Inventario con ID: {InventarioId} no encontrado para EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
                return NotFound(new { message = $"Inventario con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            return new
            {
                inventario.Id,
                inventario.Cantidad,
                inventario.FechaActualizacion
            };
        }

        // PUT: api/Inventarios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventario(Guid id, [FromBody] Inventario inventario)
        {
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim) || !Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                return Unauthorized(new { message = "No se encontró un EmprendimientoId válido en el token." });
            }

            if (id != inventario.Id)
            {
                _logger.LogWarning("El ID proporcionado ({Id}) no coincide con el ID del inventario ({InventarioId}).", id, inventario.Id);
                return BadRequest(new { message = "El ID del inventario no coincide con el proporcionado." });
            }

            var inventarioExistente = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.Id == id && i.EmprendimientoId == parsedEmprendimientoId);

            if (inventarioExistente == null)
            {
                return NotFound(new { message = $"Inventario con ID {id} no encontrado o no pertenece a tu emprendimiento." });
            }

            // Actualizamos solo los campos permitidos
            inventarioExistente.Cantidad = inventario.Cantidad;
            inventarioExistente.FechaActualizacion = DateTimeOffset.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inventario con ID: {InventarioId} actualizado correctamente para EmprendimientoId {EmprendimientoId}.", id, parsedEmprendimientoId);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Conflicto de concurrencia al actualizar el inventario." });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar inventario con ID: {InventarioId}.", id);
                return BadRequest(new { message = "Error al actualizar el inventario. Verifica las claves foráneas." });
            }

            return NoContent();
        }

        private bool InventarioExists(Guid id)
        {
            return _context.Inventarios.Any(e => e.Id == id);
        }
    }
}
