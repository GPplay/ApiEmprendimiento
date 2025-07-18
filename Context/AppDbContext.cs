using ApiEmprendimiento.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEmprendimiento.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Emprendimiento> Emprendimientos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Usuario> usuarios { get; set; }
        public DbSet<Venta> Ventas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de precisión para campos decimales
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2); // decimal(18,2)
            modelBuilder.Entity<Producto>()
                .Property(p => p.CostoFabricacion)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Inventario>()
                .Property(i => i.Cantidad)
                .HasPrecision(18, 2); // decimal(18,2)

            modelBuilder.Entity<DetalleVenta>()
                .Property(dv => dv.Precio)
                .HasPrecision(18, 2); // decimal(18,2)

            modelBuilder.Entity<Venta>()
                .Property(v => v.cantidad)
                .HasPrecision(18, 2); // decimal(18,2)

            // Relaciones y eliminación en cascada
            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Emprendimiento)
                .WithMany(e => e.Inventarios)
                .HasForeignKey(i => i.EmprendimientoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Emprendimiento)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmprendimientoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Inventario)
                .WithMany(i => i.Productos)
                .HasForeignKey(p => p.InventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(dv => dv.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(dv => dv.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(dv => dv.ProductoId)
                .OnDelete(DeleteBehavior.Restrict); // Evita eliminación accidental

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull); // Permite null

            // Índices para optimización
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.EmprendimientoId);

            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.EmprendimientoId);

            modelBuilder.Entity<DetalleVenta>()
                .HasIndex(dv => dv.ProductoId);

            modelBuilder.Entity<DetalleVenta>()
                .HasIndex(dv => dv.VentaId);

            modelBuilder.Entity<Venta>()
                .HasIndex(v => v.UsuarioId);
        }


    }
}
