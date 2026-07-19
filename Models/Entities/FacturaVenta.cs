namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del listado de facturas de venta (pantalla "Facturas Ventas"). Mapea el resultset
    /// de página de <c>SP_V2_CONSULTA_FACTURAS</c> (migración paginada de
    /// <c>SP_CONSULTA_FACTURAS</c>). <see cref="PathArchivoFactura"/> ya viene con el dominio
    /// de archivos aplicado (ver <c>FacturaRepository.AplicarUrlDominio</c>), listo para
    /// <c>window.open</c> en el front.
    /// </summary>
    public class FacturaVenta
    {
        public long IdFactura { get; set; }
        public long IdVenta { get; set; }
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

        /// <summary>Mensaje de error de facturación/cancelación (tooltip en el listado legado).</summary>
        public string? MensajeError { get; set; }

        public string? CodigoBarras { get; set; }
        public decimal MontoTotal { get; set; }
        public string? PathArchivoFactura { get; set; }
    }
}
