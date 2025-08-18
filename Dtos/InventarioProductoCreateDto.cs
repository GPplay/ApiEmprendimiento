using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class InventarioProductoCreateDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido.")]
        public Guid ProductoId { get; set; } // El ID del producto que se añadirá/asociará al inventario

        [Required(ErrorMessage = "La cantidad es requerida.")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un número positivo.")]
        public int Cantidad { get; set; } // La cantidad inicial o a añadir de este producto
    }
}