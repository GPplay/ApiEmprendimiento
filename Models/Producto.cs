using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
        public decimal Precio { get; set; }

        [Required]
        public Guid EmprendimientoId { get; set; }

        [ForeignKey(nameof(EmprendimientoId))]
        public required Emprendimiento Emprendimiento { get; set; }

        [Required]
        public DateTimeOffset FechaCreacion { get; set; } = DateTimeOffset.UtcNow;

        // Clave foránea hacia Inventario
        [Required]
        public Guid InventarioId { get; set; }

        [ForeignKey(nameof(InventarioId))]
        public Inventario Inventario { get; set; } = null!;

        // Relación con Detalles de venta (si aplica)
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    }
}
