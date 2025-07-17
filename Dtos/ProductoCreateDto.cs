using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class ProductoCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal CostoFabricacion { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal PrecioVenta { get; set; }
    }
}
