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
        [JsonIgnore]
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        [JsonIgnore]
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

        // Relación uno a uno con Inventario
        public Inventario Inventario { get; set; } = null!;
    }
}
