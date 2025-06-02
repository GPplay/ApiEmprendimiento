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
                return BadRequest("Datos de usuario inválidos");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar email único
            if (await _context.usuarios.AnyAsync(u => u.Email == usuarioDto.Email))
                return BadRequest("El email ya está registrado");

            Emprendimiento? emprendimiento = null;
            bool usarEmprendimientoExistente = false;
            bool crearNuevoEmprendimiento = false;

            // Manejo de emprendimientoId como string
            if (!string.IsNullOrWhiteSpace(usuarioDto.EmprendimientoId))
            {
                if (Guid.TryParse(usuarioDto.EmprendimientoId, out Guid emprendimientoIdGuid))
                {
                    emprendimiento = await _context.Emprendimientos.FindAsync(emprendimientoIdGuid);

                    if (emprendimiento != null)
                    {
                        usarEmprendimientoExistente = true;
                    }
                    else
                    {
                        return BadRequest($"El emprendimiento con ID {usuarioDto.EmprendimientoId} no existe");
                    }
                }
                else
                {
                    return BadRequest("El ID de emprendimiento no es válido");
                }
            }

            // Validar nuevo emprendimiento
            if (usuarioDto.NuevoEmprendimiento != null)
            {
                if (string.IsNullOrWhiteSpace(usuarioDto.NuevoEmprendimiento.Nombre))
                {
                    return BadRequest("El nombre del nuevo emprendimiento es obligatorio");
                }

                // Solo permitir crear nuevo emprendimiento si no se usará uno existente
                if (!usarEmprendimientoExistente)
                {
                    crearNuevoEmprendimiento = true;
                }
            }

            // Validar que se haya seleccionado una opción válida
            if (!usarEmprendimientoExistente && !crearNuevoEmprendimiento)
            {
                return BadRequest("Debe especificar un emprendimiento existente válido o crear uno nuevo");
            }

            // Crear nuevo emprendimiento si es necesario
            if (crearNuevoEmprendimiento)
            {
                emprendimiento = new Emprendimiento
                {
                    Id = Guid.NewGuid(),
                    Nombre = usuarioDto.NuevoEmprendimiento!.Nombre,
                    Descripcion = usuarioDto.NuevoEmprendimiento.Descripcion
                };

                _context.Emprendimientos.Add(emprendimiento);
                await _context.SaveChangesAsync();
            }

            // Crear usuario
            var nuevoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = usuarioDto.Nombre,
                Email = usuarioDto.Email,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Contrasena),
                EmprendimientoId = emprendimiento!.Id,
                Emprendimiento = emprendimiento
            };

            _context.usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = nuevoUsuario.Id }, new
            {
                nuevoUsuario.Id,
                nuevoUsuario.Nombre,
                nuevoUsuario.Email,
                EmprendimientoId = emprendimiento.Id,
                EmprendimientoNombre = emprendimiento.Nombre
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
