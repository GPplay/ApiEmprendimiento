namespace ApiEmprendimiento.Dtos
{
    public class InventarioCreateUpdateDto
    {
        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}
