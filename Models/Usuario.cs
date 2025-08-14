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

                // Cambiar la visibilidad de 'Contrasena' a 'public' para cumplir con el requisito de visibilidad.  
        [Required]
        public required string Contrasena { get; set; }

        [Required]
        public Guid EmprendimientoId { get; set; }

        [ForeignKey(nameof(EmprendimientoId))]
        public required Emprendimiento Emprendimiento { get; set; }


    }
}
