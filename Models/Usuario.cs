using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Models
{
    public class Usuario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public required string Nombre { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public required string Email { get; set; }

        [Required]
        public Guid EmprendimientoId { get; set; }

        [ForeignKey(nameof(EmprendimientoId))]
        public required Emprendimiento Emprendimiento { get; set; }

        // Relación opcional con Ventas
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
