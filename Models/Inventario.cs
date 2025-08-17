using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmprendimiento.Models
{
    public class Inventario
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Se elimina el campo 'Cantidad' de aquí, ya que ahora se gestionará por producto en InventarioProducto.

        [Required]
        public DateTimeOffset FechaActualizacion { get; set; } = DateTimeOffset.UtcNow; // Mantener para el inventario general

        // Relación uno a uno: un inventario pertenece a un emprendimiento
        [Required]
        public Guid EmprendimientoId { get; set; }

        [ForeignKey(nameof(EmprendimientoId))]
        public Emprendimiento Emprendimiento { get; set; } = null!;

        // Nueva relación Muchos a Muchos con productos a través de la tabla InventarioProducto
        // Esta colección contendrá las entradas de los productos específicos en este inventario.
        public ICollection<InventarioProducto> InventarioProductos { get; set; } = new List<InventarioProducto>();
    }
}
