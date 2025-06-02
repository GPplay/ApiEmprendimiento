using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class UsuarioUpdateDto
    {
        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? Contrasena { get; set; }
    }
}
