using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmprendimiento.Models
{
    public class DetalleVenta
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Clave foránea que enlaza con la tabla de Ventas
        [Required]
        public Guid VentaId { get; set; }

        [ForeignKey(nameof(VentaId))]
        // ¡ESTO ES CRUCIAL PARA EL ERROR EN DetalleVenta.Ventas!
        // Debe ser 'Venta' (singular) y no 'Ventas' (plural)
        public required Venta Ventas { get; set; }

        // Clave foránea que enlaza con la tabla de Productos
        [Required]
        public Guid ProductoId { get; set; }

        [ForeignKey(nameof(ProductoId))]
        public required Producto Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue)] // Cantidad vendida debe ser al menos 1
        public int Cantidad { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal Precio { get; set; } // Precio al momento de la venta por unidad

        public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;
    }
}
