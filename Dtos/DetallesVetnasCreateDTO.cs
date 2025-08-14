namespace ApiEmprendimiento.Dtos
{
    public class DetallesVetnasCreateDTO
    {

        public Guid ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }

    }
}
