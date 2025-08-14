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
                .HasMany<DetalleVenta>() // no tienes lista en Usuario, así que es navegación sin inversa
                .WithOne(dv => dv.Usuario)
                .HasForeignKey(dv => dv.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Inventario -> Producto
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Inventario)
                .WithMany(i => i.Productos)
                .HasForeignKey(p => p.InventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Usuario -> Emprendimiento
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Emprendimiento)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmprendimientoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Producto -> Emprendimiento
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Emprendimiento)
                .WithMany(e => e.Productos)
                .HasForeignKey(p => p.EmprendimientoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Inventario -> Emprendimiento
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Emprendimiento)
                .WithMany(e => e.Inventarios)
                .HasForeignKey(i => i.EmprendimientoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
