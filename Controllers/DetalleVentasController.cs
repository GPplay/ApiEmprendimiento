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
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Asegura que solo usuarios autenticados accedan
    public class DetalleVentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DetalleVentasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DetalleVenta>>> GetDetallesVenta()
        {
            return await _context.DetallesVenta.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetalleVenta>> GetDetalleVenta(Guid id)
        {
            var detalleVenta = await _context.DetallesVenta.FindAsync(id);

            if (detalleVenta == null)
                return NotFound();

            return detalleVenta;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDetalleVenta(Guid id, DetalleVenta detalleVenta)
        {
            if (id != detalleVenta.Id)
                return BadRequest();

            _context.Entry(detalleVenta).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DetalleVentaExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<DetalleVenta>> PostDetalleVenta(DetallesVetnasCreateDTO dto)
        {
            // Obtener el usuario autenticado desde los claims del JWT
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null)
                return Unauthorized("No se pudo identificar al usuario.");

            var usuarioId = Guid.Parse(usuarioIdClaim);

            // Buscar el usuario para obtener su emprendimiento
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                return NotFound("Usuario no encontrado");

            // Crear el registro de DetalleVenta
            var detalleVenta = new DetalleVenta
            {
                UsuarioId = usuario.Id,
                EmprendimientoId = usuario.EmprendimientoId,
                ProductoId = dto.ProductoId,
                Cantidad = dto.Cantidad,
                Precio = dto.Precio,
                FechaVenta = DateTimeOffset.UtcNow
            };

            _context.DetallesVenta.Add(detalleVenta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDetalleVenta), new { id = detalleVenta.Id }, detalleVenta);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDetalleVenta(Guid id)
        {
            var detalleVenta = await _context.DetallesVenta.FindAsync(id);
            if (detalleVenta == null)
                return NotFound();

            _context.DetallesVenta.Remove(detalleVenta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DetalleVentaExists(Guid id)
        {
            return _context.DetallesVenta.Any(e => e.Id == id);
        }
    }
}
