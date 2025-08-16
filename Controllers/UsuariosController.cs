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
using ApiEmprendimiento.Services;
using BCrypt.Net;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmprendimientoService _emprendimientoService;

        public UsuariosController(AppDbContext context, EmprendimientoService emprendimientoService)
        {
            _context = context;
            _emprendimientoService = emprendimientoService;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(Guid id, Usuario usuario)
        {
            if (id != usuario.Id)
                return BadRequest();

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Usuarios  
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(UsuarioCreateDto usuarioDto)
        {
            if (usuarioDto == null)
                return BadRequest("Datos de usuario inválidos");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar email único
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioDto.Email))
                return BadRequest("El email ya está registrado");

            Emprendimiento? emprendimiento = null;

            // Caso 1: Usuario selecciona un emprendimiento existente
            if (!string.IsNullOrWhiteSpace(usuarioDto.EmprendimientoId))
            {
                if (Guid.TryParse(usuarioDto.EmprendimientoId, out Guid emprendimientoIdGuid))
                {
                    emprendimiento = await _context.Emprendimientos.FindAsync(emprendimientoIdGuid);

                    if (emprendimiento == null)
                        return BadRequest($"El emprendimiento con ID {usuarioDto.EmprendimientoId} no existe");
                }
                else
                {
                    return BadRequest("El ID de emprendimiento no es válido");
                }
            }
            // Caso 2: Usuario quiere crear un nuevo emprendimiento
            else if (usuarioDto.NuevoEmprendimiento != null)
            {
                if (string.IsNullOrWhiteSpace(usuarioDto.NuevoEmprendimiento.Nombre))
                    return BadRequest("El nombre del nuevo emprendimiento es obligatorio");

                emprendimiento = await _emprendimientoService.CrearEmprendimientoConInventario(
                    usuarioDto.NuevoEmprendimiento.Nombre,
                    usuarioDto.NuevoEmprendimiento.Descripcion
                );
            }
            else
            {
                return BadRequest("Debe especificar un emprendimiento existente o crear uno nuevo");
            }

            // Crear usuario
            var nuevoUsuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = usuarioDto.Nombre,
                Email = usuarioDto.Email,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Contrasena),
                EmprendimientoId = emprendimiento.Id,
                Emprendimiento = emprendimiento
            };

            _context.Usuarios.Add(nuevoUsuario);
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
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(Guid id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
