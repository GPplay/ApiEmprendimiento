using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Models
{
    public class Emprendimiento
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public required string Nombre { get; set; }

        public string? Descripcion { get; set; }

        // Relaciones
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}

