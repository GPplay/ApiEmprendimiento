using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class InventarioProductoUpdateDto
    {
        [Required(ErrorMessage = "El ID de la entrada de inventario de producto es requerido.")]
        public Guid Id { get; set; } // El ID de la entrada InventarioProducto que se va a actualizar

        [Required(ErrorMessage = "La nueva cantidad es requerida.")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo.")]
        public int Cantidad { get; set; } // La nueva cantidad total para esta entrada
    }
}
