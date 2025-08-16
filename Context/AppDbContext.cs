using ApiEmprendimiento.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEmprendimiento.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Emprendimiento> Emprendimientos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de precisión
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.CostoFabricacion)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleVenta>()
                .Property(dv => dv.Precio)
                .HasPrecision(18, 2);

            // Relación Producto -> DetalleVenta (uno a muchos)
            modelBuilder.Entity<Producto>()
                .HasMany(p => p.DetallesVenta)
                .WithOne(dv => dv.Producto)
                .HasForeignKey(dv => dv.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Usuario -> DetalleVenta (uno a muchos)
            modelBuilder.Entity<Usuario>()
                .HasMany<DetalleVenta>()
                .WithOne(dv => dv.Usuario)
                .HasForeignKey(dv => dv.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Inventario -> Producto (uno a muchos)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Inventario)
                .WithMany(i => i.Productos)
                .HasForeignKey(p => p.InventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Usuario -> Emprendimiento (uno a muchos)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Emprendimiento)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmprendimientoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Producto -> Emprendimiento (uno a muchos)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Emprendimiento)
                .WithMany(e => e.Productos)
                .HasForeignKey(p => p.EmprendimientoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Emprendimiento <-> Inventario (uno a uno)
            modelBuilder.Entity<Emprendimiento>()
                .HasOne(e => e.Inventario)
                .WithOne(i => i.Emprendimiento)
                .HasForeignKey<Inventario>(i => i.EmprendimientoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}