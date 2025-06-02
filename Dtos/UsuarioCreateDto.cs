

namespace ApiEmprendimiento.Dtos
{
    public class UsuarioCreateDto
    {
        // Datos del usuario
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Contrasena { get; set; }

        // Para unirse a un emprendimiento existente
        public Guid? EmprendimientoId { get; set; }

        // Para crear uno nuevo
        public EmprendimientoCreateDto? NuevoEmprendimiento { get; set; }
    }
}
