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

        public async Task<Emprendimiento> CrearEmprendimientoConInventario(string nombre, string descripcion)
        {
            var emprendimiento = new Emprendimiento
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = descripcion
            };

            _context.Emprendimientos.Add(emprendimiento);

            var inventario = new Inventario
            {
                Id = Guid.NewGuid(),
                EmprendimientoId = emprendimiento.Id,
                Cantidad = 0,
                FechaActualizacion = DateTimeOffset.UtcNow
            };

            _context.Inventarios.Add(inventario);

            await _context.SaveChangesAsync();

            return emprendimiento;
        }

        public async Task<Emprendimiento?> GetEmprendimientoById(Guid id)
        {
            return await _context.Emprendimientos
                .Include(e => e.Inventarios)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}