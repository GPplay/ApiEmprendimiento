using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Models
{
    public class Inventario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Range(0, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        public DateTimeOffset FechaActualizacion { get; set; } = DateTimeOffset.UtcNow;

        // Relación uno a uno: un inventario pertenece a un emprendimiento
        [Required]
        public Guid EmprendimientoId { get; set; }

        [ForeignKey(nameof(EmprendimientoId))]
        public Emprendimiento Emprendimiento { get; set; } = null!;

        // Relación con productos
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
