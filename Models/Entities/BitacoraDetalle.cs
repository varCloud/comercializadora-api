namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Paso del timeline de un pedido interno (historial de cambios de estatus). Mapea el
    /// resultset de datos de <c>SP_V2_CONSULTA_DETALLE_PEDIDOS_INTERNOS</c> (fuente
    /// <c>PedidosInternosLog</c>). El "tiempo transcurrido" entre pasos lo calcula el front a
    /// partir de <see cref="FechaAlta"/> (orden cronológico ascendente).
    /// </summary>
    public class BitacoraDetalle
    {
        public int IdPedidoInterno { get; set; }
        public DateTime FechaAlta { get; set; }
        public string? Observacion { get; set; }
        public int IdAlmacenOrigen { get; set; }
        public string? AlmacenOrigen { get; set; }
        public int IdAlmacenDestino { get; set; }
        public string? AlmacenDestino { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }
        public int IdStatus { get; set; }
        public string? DescripcionEstatus { get; set; }
    }
}
