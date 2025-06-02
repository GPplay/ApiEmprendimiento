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
using Microsoft.CodeAnalysis.Scripting;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> Getusuarios()
        {
            return await _context.usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(Guid id)
        {
            var usuario = await _context.usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(Guid id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
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

        // POST: api/Usuarios  
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754  
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(UsuarioCreateDto usuarioDto)
        {
            if (usuarioDto == null)
            {
                return BadRequest("El objeto de usuario no puede ser nulo.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar contraseña
            if (string.IsNullOrWhiteSpace(usuarioDto.Contrasena))
            {
                return BadRequest("La contraseña es requerida.");
            }

            Emprendimiento? emprendimiento = null;

            // ... (código existente para obtener/crear emprendimiento) ...

            // CREAR USUARIO CORRECTAMENTE
            var nuevoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = usuarioDto.Nombre,
                Email = usuarioDto.Email,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Contrasena), // Hash correcto
                EmprendimientoId = emprendimiento.Id,
                Emprendimiento = emprendimiento,
                Ventas = new List<Venta>() // Sintaxis corregida
            };

            _context.usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Respuesta sin datos sensibles
            return CreatedAtAction(nameof(GetUsuario), new { id = nuevoUsuario.Id }, new
            {
                nuevoUsuario.Id,
                nuevoUsuario.Nombre,
                nuevoUsuario.Email,
                Emprendimiento = new
                {
                    Id = emprendimiento.Id,
                    Nombre = emprendimiento.Nombre
                }
            });
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(Guid id)
        {
            var usuario = await _context.usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.usuarios.Any(e => e.Id == id);
        }
    }
}
