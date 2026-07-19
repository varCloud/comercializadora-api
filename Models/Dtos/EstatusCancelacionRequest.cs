namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Consulta/actualización del estatus de cancelación ante el SAT. Migra
    /// <c>WsFacturaController.ActualizaEstatusCancelacionFactura</c>. Endpoint compartido:
    /// <see cref="EsPedidoEspecial"/> = false → <see cref="Id"/> es idVenta;
    /// true → <see cref="Id"/> es idPedidoEspecial (feature <c>migracion_facturas_pedidos_esp</c>).
    /// </summary>
    public class EstatusCancelacionRequest
    {
        public long Id { get; set; }
        public bool EsPedidoEspecial { get; set; }
    }
}
