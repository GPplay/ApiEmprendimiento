using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Models
{
    public class Inventario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductoId { get; set; }

        [ForeignKey(nameof(ProductoId))]
        public Producto? Producto { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        public DateTimeOffset FechaActualizacion { get; set; } = DateTimeOffset.UtcNow;
    }
}
