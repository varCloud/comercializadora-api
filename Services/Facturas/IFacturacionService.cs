using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Facturas
{
    /// <summary>Lógica de negocio de Facturación (ventas). Ver <c>FacturaController</c>/<c>WsFacturaController</c> del legado.</summary>
    public interface IFacturacionService
    {
        Task<RawPage<FacturaVenta>> ListarAsync(FacturasQuery query);

        Task<Notificacion<DetalleVentaFactura>> ObtenerDetalleVentaAsync(long idVenta);

        Task<Notificacion<string>> ReenviarAsync(ReenviarFacturaRequest request);

        Task<Notificacion<string>> CancelarAsync(CancelarFacturaRequest request, int idUsuario);

        Task<Notificacion<AcuseEstatusCfdi>> ConsultarEstatusCancelacionAsync(EstatusCancelacionRequest request, int idUsuario);

        // ── Variantes de Pedidos Especiales (feature migracion_facturas_pedidos_esp) ──

        Task<RawPage<FacturaPedidoEspecial>> ListarPedidosEspecialesAsync(FacturasQuery query);

        Task<Notificacion<DetalleVentaFactura>> ObtenerDetallePedidoEspecialAsync(long idPedidoEspecial);

        Task<Notificacion<string>> ReenviarPedidoEspecialAsync(ReenviarFacturaPeRequest request);

        Task<Notificacion<string>> CancelarPedidoEspecialAsync(CancelarFacturaPeRequest request, int idUsuario);
    }
}
