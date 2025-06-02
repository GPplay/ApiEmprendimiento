using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class EmprendimientoCreateDto
    {
        [Required]
        [MaxLength(255)]
        public required string Nombre { get; set; }

        public string? Descripcion { get; set; }
    }
}
