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
        public async Task<ActionResult<IEnumerable<Emprendimiento>>> GetEmprendimientos()
        {
            return await _context.Emprendimientos.ToListAsync();
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
        public async Task<ActionResult<Emprendimiento>> PostEmprendimiento([FromBody] EmprendimientoCreateDto emprendimientoDto) // Usar el DTO
        {
            // Validación automática gracias a [ApiController] y DataAnnotations
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Mapear el DTO a la entidad Emprendimiento
            var emprendimiento = new Emprendimiento
            {
                Id = Guid.NewGuid(), // Generar nuevo ID
                Nombre = emprendimientoDto.Nombre,
                Descripcion = emprendimientoDto.Descripcion
                // Dejar el resto de propiedades con valores por defecto
            };

            _context.Emprendimientos.Add(emprendimiento);
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
