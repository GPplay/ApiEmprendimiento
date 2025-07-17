using ApiEmprendimiento.Context;
using ApiEmprendimiento.Dtos;
using ApiEmprendimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Producto>>> GetProductos()
        {
            return await _context.Productos.ToListAsync();
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetProducto(Guid id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return producto;
        }

        // PUT: api/Productos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProducto(Guid id, Producto producto)
        {
            if (id != producto.Id)
            {
                return BadRequest();
            }

            _context.Entry(producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
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

        // POST: api/Productos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Producto>> PostProducto([FromBody] ProductoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extraer el claim de EmprendimientoId del JWT
            var emprendimientoIdClaim = User.FindFirst("emprendimientoId")?.Value;
            if (emprendimientoIdClaim == null)
                return Unauthorized("No se encontró el EmprendimientoId en el token.");

            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
                return Unauthorized("EmprendimientoId inválido.");

            var emprendimiento = await _context.Emprendimientos.FindAsync(parsedEmprendimientoId);
            if (emprendimiento == null)
                return NotFound("No se encontró el emprendimiento.");

            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.EmprendimientoId == parsedEmprendimientoId);

            if (inventario == null)
                return BadRequest("No se encontró inventario asociado al emprendimiento.");

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

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(Guid id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductoExists(Guid id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
