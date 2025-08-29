using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmprendimiento.Models
{
    // Esta clase representa la tabla de unión para la relación Muchos a Muchos entre Inventario y Producto.
    // Contendrá la cantidad específica de un producto en un inventario dado.
    public class InventarioProducto
    {
        // Clave primaria para esta tabla de unión. Se usa un Guid como identificador único.
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Clave foránea que enlaza con la tabla de Inventarios.
        // Indica a qué inventario pertenece esta entrada de producto.
        [Required]
        public Guid InventarioId { get; set; }

        // Clave foránea que enlaza con la tabla de Productos.
        // Indica a qué producto se refiere esta entrada.
        [Required]
        public Guid ProductoId { get; set; }

        // Cantidad del producto específico en este inventario.
        // Asegura que la cantidad sea un valor no negativo.
        [Required]
        [Range(0, int.MaxValue)]
        public int Cantidad { get; set; }

        // Fecha y hora de la última actualización de la cantidad.
        // Se establece automáticamente a la hora UTC en el momento de la creación.
        [Required]
        public DateTimeOffset FechaActualizacion { get; set; } = DateTimeOffset.UtcNow;

        // ¡NUEVO! Propiedad para almacenar el costo actual de las unidades de este producto en stock.
        // Esto es para propósitos informativos sobre el valor del inventario en tiempo real.
        [Required]
        [Range(0, 99999999999.99)] // Rango para valores monetarios
        [Column(TypeName = "decimal(18,2)")] // Asegura precisión en la base de datos
        public decimal CostoActualEnStock { get; set; } = 0; // Inicializar en 0

        // Propiedad de navegación para el Inventario asociado.
        // Permite a Entity Framework Core cargar el objeto Inventario relacionado.
        [ForeignKey(nameof(InventarioId))]
        public Inventario Inventario { get; set; } = null!;

        // Propiedad de navegación para el Producto asociado.
        // Permite a Entity Framework Core cargar el objeto Producto relacionado.
        [ForeignKey(nameof(ProductoId))]
        public Producto Producto { get; set; } = null!;
    }
}
