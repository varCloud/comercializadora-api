namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del listado de facturas de pedidos especiales (pantalla "Facturas Pedidos Esp").
    /// Mapea el resultset de página de <c>SP_V2_CONSULTA_FACTURAS_PEDIDOS_ESPECIALES</c>
    /// (migración paginada de <c>SP_FACTURACION_OBTENER_FACTURAS_PEDIDOS_ESPECIALES</c>).
    /// Espejo de <see cref="FacturaVenta"/> con <c>IdPedidoEspecial</c> en lugar de <c>IdVenta</c>.
    /// </summary>
    public class FacturaPedidoEspecial
    {
        public long IdFacturaPedidoEspecial { get; set; }
        public long IdPedidoEspecial { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaTimbrado { get; set; }
        public string? Uuid { get; set; }

        /// <summary>1 Facturada, 2 Cancelada, 3 Error, 4 En proceso de cancelación (FacCatEstatusFactura).</summary>
        public int IdEstatusFactura { get; set; }
        public string? Descripcion { get; set; }

        public string? NombreCliente { get; set; }
        public string? NombreUsuarioFacturacion { get; set; }
        public string? NombreUsuarioCancelacion { get; set; }
        public DateTime? FechaCancelacion { get; set; }

        /// <summary>Mensaje de error de facturación/cancelación (tooltip en el listado).</summary>
        public string? MensajeError { get; set; }

        public string? CodigoBarras { get; set; }
        public decimal MontoTotal { get; set; }
        public string? PathArchivoFactura { get; set; }
    }
}
