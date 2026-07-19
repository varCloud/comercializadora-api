using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Facturas
{
    /// <summary>Repositorio del módulo Facturación (ventas). Ver <c>patron-repository.md</c>.</summary>
    public interface IFacturaRepository
    {
        /// <summary>Listado paginado (SP_V2_CONSULTA_FACTURAS).</summary>
        Task<RawPage<FacturaVenta>> ListarAsync(FacturasQuery query);

        /// <summary>Detalle de venta para el modal de reenvío (SP_FACTURACION_OBTENER_DETALLE_VENTA).</summary>
        Task<Notificacion<DetalleVentaFactura>> ObtenerDetalleVentaAsync(long idVenta);

        /// <summary>Datos de venta/cliente para reenviar por correo (SP_FACTURACION_OBTENER_DATOS_FACTURA).</summary>
        Task<Notificacion<DatosFacturaVenta>> ObtenerDatosFacturaAsync(long idVenta);

        /// <summary>Datos para armar la cancelación CFDI ante el PAC (SP_OBTENER_CANCELACION_FACTURA).</summary>
        Task<Notificacion<CancelacionFactura>> ObtenerCancelacionAsync(long idVenta);

        /// <summary>Registra el resultado de la cancelación (SP_FACTURACION_INSERTA_FACTURA_CANCELADA).</summary>
        Task<Notificacion<string>> CancelarFacturaAsync(long idVenta, int idUsuario, int idEstatusFactura, string mensajeError);

        /// <summary>Path de archivo + UUID por idVenta o idPedidoEspecial (SP_FACTURAS_OBTENER_PATH_ARCHIVO).</summary>
        Task<Notificacion<ArchivoFactura>> ObtenerPathArchivoAsync(long? idVenta, long? idPedidoEspecial);

        /// <summary>Datos fiscales del emisor (SP_FACTURACION_OBTENER_CONFIGURACION_COMPROBANTE).</summary>
        Task<Notificacion<ConfiguracionComprobante>> ObtenerConfiguracionComprobanteAsync();

        // ── Variantes de Pedidos Especiales (feature migracion_facturas_pedidos_esp) ──

        /// <summary>Listado paginado PE (SP_V2_CONSULTA_FACTURAS_PEDIDOS_ESPECIALES).</summary>
        Task<RawPage<FacturaPedidoEspecial>> ListarPedidosEspecialesAsync(FacturasQuery query);

        /// <summary>Detalle del pedido especial para el modal de reenvío (SP_FACTURACION_OBTENER_DETALLE_PEDIDO_ESPECIAL).</summary>
        Task<Notificacion<DetalleVentaFactura>> ObtenerDetallePedidoEspecialAsync(long idPedidoEspecial);

        /// <summary>Datos de pedido/cliente para reenviar por correo (SP_FACTURACION_OBTENER_DATOS_FACTURA_PEDIDO_ESPECIAL).</summary>
        Task<Notificacion<DatosFacturaVenta>> ObtenerDatosFacturaPedidoEspecialAsync(long idPedidoEspecial);

        /// <summary>Datos para armar la cancelación CFDI PE (SP_FACTURACION_OBTENER_CANCELACION_FACTURA).</summary>
        Task<Notificacion<CancelacionFactura>> ObtenerCancelacionPedidoEspecialAsync(long idPedidoEspecial);

        /// <summary>Registra el resultado de la cancelación PE (SP_FACTURACION_INSERTA_FACTURA_CANCELADA_PEDIDOS_ESPECIALES).</summary>
        Task<Notificacion<string>> CancelarFacturaPedidoEspecialAsync(long idPedidoEspecial, int idUsuario, int idEstatusFactura, string mensajeError);
    }
}
