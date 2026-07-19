namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Datos de la factura + emisor necesarios para armar la solicitud de cancelación CFDI ante
    /// el PAC. Mapea <c>SP_OBTENER_CANCELACION_FACTURA</c> (Facturas + FactConfiguracionComprobante),
    /// equivalente a <c>FacturaDAO.ObtenerCancelacionFactura</c> del legado para facturas de venta
    /// (idPedidoEspecial = 0). La variante de pedidos especiales usa
    /// <c>SP_FACTURACION_OBTENER_CANCELACION_FACTURA</c> (fuera de alcance de esta feature).
    /// </summary>
    public class CancelacionFactura
    {
        public long IdFactura { get; set; }
        public long IdVenta { get; set; }
        public string? Uuid { get; set; }
        public string? PathArchivoFactura { get; set; }

        /// <summary>RFC del emisor (FactConfiguracionComprobante.Rfc), para <c>RfcEmisor</c> del CFDI de cancelación.</summary>
        public string? Rfc { get; set; }
    }
}
