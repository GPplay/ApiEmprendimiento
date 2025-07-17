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
        [HttpPost("{emprendimientoId}")]
        public async Task<ActionResult<Producto>> PostProducto(Guid emprendimientoId, [FromBody] ProductoCreateDto dto)
        {
            // Validar modelo recibido
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Obtener el emprendimiento
            var emprendimiento = await _context.Emprendimientos.FindAsync(emprendimientoId);
            if (emprendimiento == null)
                return NotFound("No se encontró el emprendimiento.");

            // Obtener el inventario asociado al emprendimiento
            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.EmprendimientoId == emprendimientoId);

            if (inventario == null)
                return BadRequest("No se encontró un inventario asociado al emprendimiento.");

            // Crear el producto
            var producto = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                CostoFabricacion = dto.CostoFabricacion,
                PrecioVenta = dto.PrecioVenta,
                FechaCreacion = DateTimeOffset.UtcNow,
                EmprendimientoId = emprendimiento.Id,
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
