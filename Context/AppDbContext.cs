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
        public DbSet<InventarioProducto> InventarioProductos { get; set; } // Nueva tabla de unión

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

            // ***************************************************************
            // Se elimina la relación directa Inventario -> Producto (uno a muchos)
            // Ya no es necesaria porque ahora se maneja a través de InventarioProducto
            // ***************************************************************

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

            // Configuración de la relación Muchos a Muchos entre Inventario y Producto
            // a través de la tabla de unión InventarioProducto
            modelBuilder.Entity<InventarioProducto>()
                .HasKey(ip => ip.Id); // Usar la propiedad Id como clave primaria para la tabla de unión

            modelBuilder.Entity<InventarioProducto>()
                .HasOne(ip => ip.Inventario)
                .WithMany(i => i.InventarioProductos)
                .HasForeignKey(ip => ip.InventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InventarioProducto>()
                .HasOne(ip => ip.Producto)
                .WithMany(p => p.InventarioProductos)
                .HasForeignKey(ip => ip.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
