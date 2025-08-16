using ApiEmprendimiento.Context;
using ApiEmprendimiento.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ApiEmprendimiento.Services
{
    public class EmprendimientoService
    {
        private readonly AppDbContext _context;

        public EmprendimientoService(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Emprendimiento> CrearEmprendimientoConInventario(string nombre, string? descripcion)
        {
            var emprendimiento = new Emprendimiento
            {
                Id = Guid.NewGuid(),
                Nombre = nombre,
                Descripcion = descripcion
            };

            var inventario = new Inventario
            {
                Id = Guid.NewGuid(),
                EmprendimientoId = emprendimiento.Id,
                Emprendimiento = emprendimiento,
                Cantidad = 0,
                FechaActualizacion = DateTimeOffset.UtcNow
            };

            _context.Emprendimientos.Add(emprendimiento);
            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();

            emprendimiento.Inventario = inventario;
            return emprendimiento;
        }
    }
}