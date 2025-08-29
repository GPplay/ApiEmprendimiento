using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Propiedad de navegación para el Inventario (relación uno a uno)
        public Inventario? Inventario { get; set; }

        // Colección de productos asociados a este emprendimiento (relación uno a muchos)
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

        // Colección de ventas asociadas a este emprendimiento (relación uno a muchos)
        public required ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        // ¡NUEVO! Colección de usuarios asociados a este emprendimiento (relación uno a muchos)
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        // En Emprendimiento.cs
        public ICollection<ReporteFinancieroMensual> ReportesFinancierosMensuales { get; set; } = new List<ReporteFinancieroMensual>();
    }
}