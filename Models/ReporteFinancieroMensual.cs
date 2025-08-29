using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmprendimiento.Models
{
    // Esta tabla consolidará los gastos de fabricación y las ganancias por ventas mensualmente.
    public class ReporteFinancieroMensual
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // ID único para el registro del reporte

        [Required]
        public Guid EmprendimientoId { get; set; } // Clave foránea al emprendimiento

        [ForeignKey(nameof(EmprendimientoId))]
        public required Emprendimiento Emprendimiento { get; set; } // Propiedad de navegación

        [Required]
        [Range(2000, 3000)] // Rango de años razonable
        public int Ano { get; set; } // Año del reporte (ej. 2025)

        [Required]
        [Range(1, 12)] // Mes del reporte (1 para Enero, 12 para Diciembre)
        public int Mes { get; set; }

        // Acumulador de todos los costos de fabricación incurridos en este mes
        [Required]
        [Range(0, 99999999999.99)] // Valor monetario, puede ser cero
        [Column(TypeName = "decimal(18,2)")] // Asegura precisión en la base de datos
        public decimal TotalGastosFabricacionMes { get; set; } = 0;

        // Acumulador de los ingresos totales de todas las ventas realizadas en este mes
        [Required]
        [Range(0, 99999999999.99)] // Valor monetario, puede ser cero
        [Column(TypeName = "decimal(18,2)")] // Asegura precisión en la base de datos
        public decimal TotalGananciasVentasMes { get; set; } = 0;

        [Required]
        public DateTimeOffset FechaUltimaActualizacion { get; set; } = DateTimeOffset.UtcNow; // Fecha de la última actualización de este registro

        // Constructor opcional para facilitar la inicialización
        public ReporteFinancieroMensual()
        {
            // Propiedades con valores por defecto se inicializan automáticamente
        }
    }
}
