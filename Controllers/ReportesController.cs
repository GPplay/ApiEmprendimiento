using ApiEmprendimiento.Context;
using ApiEmprendimiento.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiEmprendimiento.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Asegura que solo usuarios autenticados accedan a este controlador
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(AppDbContext context, ILogger<ReportesController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper para extraer el EmprendimientoId del token JWT del usuario
        private ActionResult<Guid> GetEmprendimientoIdFromToken()
        {
            var emprendimientoIdClaim = User.FindFirst("EmprendimientoId")?.Value;
            if (string.IsNullOrEmpty(emprendimientoIdClaim))
            {
                _logger.LogWarning("No se encontró el claim 'EmprendimientoId' en el token JWT.");
                return Unauthorized(new { message = "No se encontró el EmprendimientoId en el token de autenticación." });
            }

            if (!Guid.TryParse(emprendimientoIdClaim, out var parsedEmprendimientoId))
            {
                _logger.LogWarning("El EmprendimientoId '{EmprendimientoId}' en el token es inválido.", emprendimientoIdClaim);
                return Unauthorized(new { message = "El EmprendimientoId proporcionado en el token es inválido." });
            }
            return parsedEmprendimientoId;
        }

        /// <summary>
        /// Obtiene los reportes financieros mensuales para el emprendimiento del usuario autenticado.
        /// </summary>
        /// <param name="ano">Opcional: Permite filtrar los reportes por un año específico.</param>
        /// <returns>Una lista de ReporteFinancieroMensual.</returns>
        [HttpGet("financieros/mensual")]
        public async Task<ActionResult<IEnumerable<ReporteFinancieroMensual>>> GetReportesFinancierosMensuales(
            [FromQuery] int? ano // Parámetro de consulta opcional para filtrar por año
        )
        {
            var emprendimientoIdResult = GetEmprendimientoIdFromToken();
            if (emprendimientoIdResult.Result is UnauthorizedResult)
            {
                return emprendimientoIdResult.Result;
            }
            var parsedEmprendimientoId = emprendimientoIdResult.Value;

            _logger.LogInformation("Solicitud para obtener reportes financieros mensuales para EmprendimientoId: {EmprendimientoId}, Año: {Ano}", parsedEmprendimientoId, ano);

            // Consulta base filtrada por el EmprendimientoId del usuario
            var query = _context.ReportesFinancierosMensuales
                .Where(r => r.EmprendimientoId == parsedEmprendimientoId);

            // Aplica filtro por año si se proporciona
            if (ano.HasValue)
            {
                query = query.Where(r => r.Ano == ano.Value);
            }

            // Ordena los reportes por año y luego por mes para una secuencia lógica en la respuesta
            var reportes = await query.OrderBy(r => r.Ano)
                                      .ThenBy(r => r.Mes)
                                      .ToListAsync();

            if (!reportes.Any())
            {
                _logger.LogInformation("No se encontraron reportes financieros mensuales para EmprendimientoId: {EmprendimientoId} en el año {Ano}.", parsedEmprendimientoId, ano ?? 0);
                return NotFound(new { message = "No se encontraron reportes financieros mensuales para tu emprendimiento con los criterios especificados." });
            }

            return Ok(reportes);
        }

        // Puedes añadir más métodos de reporte aquí en el futuro, por ejemplo:
        // [HttpGet("financieros/anual")] para un resumen anual
        // [HttpGet("inventario/valoracion")] para el valor total del inventario
    }
}
