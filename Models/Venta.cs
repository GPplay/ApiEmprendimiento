using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Models
{
    public class Venta
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public required Usuario Usuario { get; set; }

        [Required]
        public DateTimeOffset FechaVenta { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Range(0, 99999999.99)]
        public decimal Total { get; set; }

        // Relación
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

    }
}
