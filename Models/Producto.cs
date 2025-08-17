using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmprendimiento.Models
{
    public class Producto
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public required string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal CostoFabricacion { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal PrecioVenta { get; set; }

        [Required]
        public Guid EmprendimientoId { get; set; }

        [ForeignKey(nameof(EmprendimientoId))]
        public required Emprendimiento Emprendimiento { get; set; }

        [Required]
        public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;

        // Se eliminan 'InventarioId' y 'Inventario' directos.
        // La relación con Inventario ahora se gestiona a través de la tabla de unión InventarioProducto.

        // Nueva relación Muchos a Muchos con inventario a través de la tabla InventarioProducto
        // Esta colección contendrá las entradas de este producto en diferentes inventarios (aunque en tu caso sea solo uno).
        public ICollection<InventarioProducto> InventarioProductos { get; set; } = new List<InventarioProducto>();

        // Relación con Detalles de venta (si aplica)
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
