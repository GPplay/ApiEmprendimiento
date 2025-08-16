using System;
using System.Threading.Tasks;
using ApiEmprendimiento.Context;
using ApiEmprendimiento.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEmprendimiento.Services
{
    public class EmprendimientoService
    {
        private readonly AppDbContext _context;

        public EmprendimientoService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un emprendimiento junto con su inventario asociado.
        /// </summary>
        /// <param name="nombre">Nombre del emprendimiento</param>
        /// <param name="descripcion">Descripción del emprendimiento</param>
        /// <returns>El emprendimiento creado</returns>
        public async Task<Emprendimiento> CrearEmprendimientoConInventario(string nombre, string descripcion)
        {
            var emprendimientoId = Guid.NewGuid();

            var emprendimiento = new Emprendimiento
            {
                Id = emprendimientoId,
                Nombre = nombre,
                Descripcion = descripcion
            };

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

            return emprendimiento;
        }

        /// <summary>
        /// Busca un emprendimiento por Id.
        /// </summary>
        public async Task<Emprendimiento?> ObtenerEmprendimiento(Guid id)
        {
            return await _context.Emprendimientos
                .Include(e => e.Usuarios)
                .Include(e => e.Inventarios)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Elimina un emprendimiento y su inventario asociado.
        /// </summary>
        public async Task<bool> EliminarEmprendimiento(Guid id)
        {
            var emprendimiento = await _context.Emprendimientos.FindAsync(id);
            if (emprendimiento == null)
                return false;

            // Si existe inventario, lo eliminamos también
            var inventario = await _context.Inventarios.FirstOrDefaultAsync(i => i.EmprendimientoId == id);
            if (inventario != null)
                _context.Inventarios.Remove(inventario);

            _context.Emprendimientos.Remove(emprendimiento);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
