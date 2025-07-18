using System.ComponentModel.DataAnnotations;

namespace ApiEmprendimiento.Dtos
{
    public class VentaCreateDto
    {
        public Guid ProductoId { get; set; }

        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
    }
}
