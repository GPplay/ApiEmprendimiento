using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class DetallesVetnasCreateDTO
    {

        [Required(ErrorMessage = "El ID de la venta es requerido.")]
        public Guid VentaId { get; set; } // Añadido para asociar el detalle a una venta existente

        [Required(ErrorMessage = "El ID del producto es requerido.")]
        public Guid ProductoId { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "El precio es requerido.")]
        [Range(0, 99999999.99, ErrorMessage = "El precio debe ser un número positivo.")]
        public decimal Precio { get; set; } // Añadido el precio al DTO

    }
}
