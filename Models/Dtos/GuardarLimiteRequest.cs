namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload para crear/actualizar el límite mín/máx de un producto en un almacén.
    /// El idUsuario NO viaja aquí: se toma del JWT en el controller.
    /// </summary>
    public class GuardarLimiteRequest
    {
        public int IdProducto { get; set; }
        public int IdAlmacen { get; set; }
        public int Minimo { get; set; }
        public int Maximo { get; set; }
    }
}
