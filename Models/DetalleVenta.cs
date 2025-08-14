using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmprendimiento.Models
{
    public class DetalleVenta
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; }

        [Required]
        public Guid EmprendimientoId { get; set; }

        [Required]
        public Guid ProductoId { get; set; }

        [ForeignKey(nameof(ProductoId))]
        public Producto Producto { get; set; }

        [Required]
        public DateTimeOffset FechaVenta { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal Precio { get; set; }
    }
}
