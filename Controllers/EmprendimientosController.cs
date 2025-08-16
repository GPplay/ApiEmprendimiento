using ApiEmprendimiento.Context;
using ApiEmprendimiento.Dtos;
using ApiEmprendimiento.Models;
using ApiEmprendimiento.Services;
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
    public class EmprendimientosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmprendimientoService _emprendimientoService;
        private readonly ILogger<EmprendimientosController> _logger;

        public EmprendimientosController(AppDbContext context, EmprendimientoService emprendimientoService, ILogger<EmprendimientosController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emprendimientoService = emprendimientoService ?? throw new ArgumentNullException(nameof(emprendimientoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEmprendimientos()
        {
            _logger.LogInformation("Obteniendo lista de emprendimientos.");
            return await _context.Emprendimientos
                .Include(e => e.Usuarios)
                .Include(e => e.Inventario)
                .Select(e => new
                {
                    e.Id,
                    e.Nombre,
                    e.Descripcion,
                    Usuarios = e.Usuarios.Select(u => u.Nombre).ToList(),
                    InventarioId = e.Inventario.Id
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Emprendimiento>> GetEmprendimiento(Guid id)
        {
            var emprendimiento = await _context.Emprendimientos
                .Include(e => e.Inventario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprendimiento == null)
            {
                _logger.LogWarning("Emprendimiento con ID: {EmprendimientoId} no encontrado.", id);
                return NotFound(new { message = $"Emprendimiento con ID {id} no encontrado." });
            }

            return emprendimiento;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmprendimiento(Guid id, Emprendimiento emprendimiento)
        {
            if (id != emprendimiento.Id)
            {
                _logger.LogWarning("El ID proporcionado ({Id}) no coincide con el ID del emprendimiento ({EmprendimientoId}).", id, emprendimiento.Id);
                return BadRequest(new { message = "El ID del emprendimiento no coincide con el proporcionado." });
            }

            _context.Entry(emprendimiento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Emprendimiento con ID: {EmprendimientoId} actualizado correctamente.", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmprendimientoExists(id))
                {
                    _logger.LogWarning("Emprendimiento con ID: {EmprendimientoId} no encontrado durante la actualización.", id);
                    return NotFound(new { message = $"Emprendimiento con ID {id} no encontrado." });
                }
                else
                {
                    _logger.LogError("Error de concurrencia al actualizar emprendimiento con ID: {EmprendimientoId}.", id);
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar emprendimiento con ID: {EmprendimientoId}.", id);
                return BadRequest(new { message = "Error al actualizar el emprendimiento. Verifica las claves foráneas." });
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Emprendimiento>> PostEmprendimiento([FromBody] EmprendimientoCreateDto emprendimientoDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Datos inválidos en la solicitud de creación de emprendimiento.");
                return BadRequest(ModelState);
            }

            try
            {
                var emprendimiento = await _emprendimientoService.CrearEmprendimientoConInventario(
                    emprendimientoDto.Nombre,
                    emprendimientoDto.Descripcion
                );
                _logger.LogInformation("Emprendimiento creado con ID: {EmprendimientoId} y Inventario ID: {InventarioId}.", emprendimiento.Id, emprendimiento.Inventario.Id);
                return CreatedAtAction(nameof(GetEmprendimiento), new { id = emprendimiento.Id }, emprendimiento);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar el emprendimiento.");
                return StatusCode(500, new { message = "Error al guardar el emprendimiento." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmprendimiento(Guid id)
        {
            var emprendimiento = await _context.Emprendimientos
                .Include(e => e.Inventario)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprendimiento == null)
            {
                _logger.LogWarning("Emprendimiento con ID: {EmprendimientoId} no encontrado.", id);
                return NotFound(new { message = $"Emprendimiento con ID {id} no encontrado." });
            }

            if (emprendimiento.Inventario != null)
            {
                _logger.LogWarning("No se puede eliminar el emprendimiento con ID: {EmprendimientoId} porque tiene un inventario asociado.", id);
                return BadRequest(new { message = "No se puede eliminar el emprendimiento porque tiene un inventario asociado." });
            }

            try
            {
                _context.Emprendimientos.Remove(emprendimiento);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Emprendimiento con ID: {EmprendimientoId} eliminado correctamente.", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar emprendimiento con ID: {EmprendimientoId}.", id);
                return BadRequest(new { message = "Error al eliminar el emprendimiento debido a restricciones de claves foráneas." });
            }

            return NoContent();
        }

        private bool EmprendimientoExists(Guid id)
        {
            return _context.Emprendimientos.Any(e => e.Id == id);
        }
    }
}