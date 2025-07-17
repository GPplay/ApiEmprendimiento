using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiEmprendimiento.Context;
using ApiEmprendimiento.Models;
using ApiEmprendimiento.Dtos;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmprendimientosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmprendimientosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Emprendimientos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEmprendimientos()
        {
            return await _context.Emprendimientos
         .Include(e => e.Usuarios) // Cargar usuarios relacionados
         .Select(e => new
         {
             e.Id,
             Nombre = e.Nombre, // Corregir nombre de propiedad
             e.Descripcion,
             Usuarios = e.Usuarios.Select(u => u.Nombre).ToList() // Solo nombres
         })
         .ToListAsync();
        }

        // GET: api/Emprendimientos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Emprendimiento>> GetEmprendimiento(Guid id)
        {
            var emprendimiento = await _context.Emprendimientos.FindAsync(id);

            if (emprendimiento == null)
            {
                return NotFound();
            }

            return emprendimiento;
        }

        // PUT: api/Emprendimientos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmprendimiento(Guid id, Emprendimiento emprendimiento)
        {
            if (id != emprendimiento.Id)
            {
                return BadRequest();
            }

            _context.Entry(emprendimiento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmprendimientoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Emprendimientos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Emprendimiento>> PostEmprendimiento([FromBody] EmprendimientoCreateDto emprendimientoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emprendimientoId = Guid.NewGuid();

            var emprendimiento = new Emprendimiento
            {
                Id = emprendimientoId,
                Nombre = emprendimientoDto.Nombre,
                Descripcion = emprendimientoDto.Descripcion
            };

            // Crear inventario automáticamente para el nuevo emprendimiento
            var inventario = new Inventario
            {
                Id = Guid.NewGuid(),
                EmprendimientoId = emprendimientoId,
                Cantidad = 0,
                FechaActualizacion = DateTimeOffset.UtcNow
            };

            _context.Emprendimientos.Add(emprendimiento);
            _context.Inventarios.Add(inventario);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmprendimiento", new { id = emprendimiento.Id }, emprendimiento);
        }

        // DELETE: api/Emprendimientos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmprendimiento(Guid id)
        {
            var emprendimiento = await _context.Emprendimientos.FindAsync(id);
            if (emprendimiento == null)
            {
                return NotFound();
            }

            _context.Emprendimientos.Remove(emprendimiento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmprendimientoExists(Guid id)
        {
            return _context.Emprendimientos.Any(e => e.Id == id);
        }
    }
}
