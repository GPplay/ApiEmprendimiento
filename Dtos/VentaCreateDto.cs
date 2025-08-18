using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class VentaCreateDto
    {
        [Required(ErrorMessage = "La lista de productos es requerida.")]
        [MinLength(1, ErrorMessage = "Una venta debe tener al menos un producto.")]
        public List<DetallesVetnasCreateDTO> Items { get; set; } = new List<DetallesVetnasCreateDTO>();
    }
}
