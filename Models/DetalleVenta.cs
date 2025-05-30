using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Models
{
    public class DetalleVenta
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid VentaId { get; set; }

        [ForeignKey(nameof(VentaId))]
        public Venta? Venta { get; set; }

        [Required]
        public Guid ProductoId { get; set; }

        [ForeignKey(nameof(ProductoId))]
        public Producto? Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0, 99999999.99)]
        public decimal Precio { get; set; }
    }
}
