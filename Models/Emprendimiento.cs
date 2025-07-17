using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
        [JsonIgnore] // Evitar referencia circular
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        [JsonIgnore] // Evitar referencia circular
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

        public ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();
    }
}

