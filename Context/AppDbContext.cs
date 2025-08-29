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
        public DbSet<InventarioProducto> InventarioProductos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        // ¡NUEVO! DbSet para la tabla ReporteFinancieroMensual
        public DbSet<ReporteFinancieroMensual> ReportesFinancierosMensuales { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de precisión para tipos decimal
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Producto>()
                .Property(p => p.CostoFabricacion)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetalleVenta>()
                .Property(dv => dv.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Venta>()
                .Property(v => v.TotalVenta)
                .HasPrecision(18, 2);

            // ¡NUEVO! Configuración de precisión para ReporteFinancieroMensual
            modelBuilder.Entity<ReporteFinancieroMensual>()
                .Property(rfm => rfm.TotalGastosFabricacionMes)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ReporteFinancieroMensual>()
                .Property(rfm => rfm.TotalGananciasVentasMes)
                .HasPrecision(18, 2);


            // Relación Producto -> DetalleVenta (uno a muchos)
            modelBuilder.Entity<Producto>()
                .HasMany(p => p.DetallesVenta)
                .WithOne(dv => dv.Producto)
                .HasForeignKey(dv => dv.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Venta -> DetalleVenta (uno a muchos)
            modelBuilder.Entity<Venta>()
                .HasMany(v => v.DetallesVenta)
                .WithOne(dv => dv.Ventas)
                .HasForeignKey(dv => dv.VentaId)
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
                .HasKey(ip => ip.Id);

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

            // Relación Venta -> Emprendimiento (uno a muchos)
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Emprendimiento)
                .WithMany(e => e.Ventas)
                .HasForeignKey(v => v.EmprendimientoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Usuario -> Emprendimiento (uno a muchos)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Emprendimiento)
                .WithMany(e => e.Usuarios)
                .HasForeignKey(u => u.EmprendimientoId)
                .OnDelete(DeleteBehavior.Restrict); // O Cascade si se eliminan usuarios con el emprendimiento


            // ¡NUEVO! Relación ReporteFinancieroMensual -> Emprendimiento (muchos a uno)
            modelBuilder.Entity<ReporteFinancieroMensual>()
                .HasOne(rfm => rfm.Emprendimiento) // Un reporte pertenece a UN emprendimiento
                .WithMany(e => e.ReportesFinancierosMensuales) // Un emprendimiento tiene MUCHOS reportes (necesitas añadir esta colección en Emprendimiento.cs)
                .HasForeignKey(rfm => rfm.EmprendimientoId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina un emprendimiento, sus reportes también se eliminan.
        }
    }
}
