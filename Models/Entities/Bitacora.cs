namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Renglón del reporte "Bitácoras" (consulta de pedidos internos = traspasos de producto
    /// entre almacenes). Mapea el resultset de datos de <c>SP_V2_CONSULTA_PEDIDOS_INTERNOS</c>.
    /// Una fila por producto del pedido (join a <c>PedidosInternosDetalle</c>). Migra el uso de
    /// <c>PedidosInternos</c> en la pantalla legada Bitácoras.
    /// </summary>
    public class Bitacora
    {
        public int IdPedidoInterno { get; set; }
        public DateTime FechaAlta { get; set; }
        public int IdAlmacenOrigen { get; set; }
        public string? AlmacenOrigen { get; set; }
        public int IdAlmacenDestino { get; set; }
        public string? AlmacenDestino { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }
        public int IdStatus { get; set; }
        public string? DescripcionEstatus { get; set; }
        public int IdProducto { get; set; }
        public string? DescripcionProducto { get; set; }
        public decimal Cantidad { get; set; }
    }
}
