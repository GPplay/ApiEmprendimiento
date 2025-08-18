using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmprendimiento.Models
{
    // Representa una transacción de venta individual
    public class Venta
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // ID único para la venta

        [Required]
        public DateTimeOffset FechaVenta { get; set; } = DateTimeOffset.UtcNow; // Fecha y hora de la venta

        // Precio total de la venta (suma de todos los detalles de venta)
        [Required]
        [Range(0, 99999999999.99)] // Rango para valores monetarios
        public decimal TotalVenta { get; set; }

        // Clave foránea para el emprendimiento al que pertenece esta venta
        [Required]
        public Guid EmprendimientoId { get; set; }

        [ForeignKey(nameof(EmprendimientoId))]
        public required Emprendimiento Emprendimiento { get; set; } // Propiedad de navegación

        // Colección de detalles de venta asociados a esta venta (relación uno a muchos)
        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}