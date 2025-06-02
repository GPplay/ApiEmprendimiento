

namespace ApiEmprendimiento.Dtos
{
    public class UsuarioCreateDto
    {
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string Contrasena { get; set; }
    
    // Cambia a string en lugar de Guid?
    public string? EmprendimientoId { get; set; }
    
    public EmprendimientoCreateDto? NuevoEmprendimiento { get; set; }
    }
}
