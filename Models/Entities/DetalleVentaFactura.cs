namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Detalle de la venta para el modal de reenvío de factura (cliente, forma de pago, uso
    /// CFDI y totales). Mapea los resultsets de receptor + conceptos de
    /// <c>SP_FACTURACION_OBTENER_DETALLE_VENTA</c>; <see cref="Total"/> se calcula sumando
    /// <see cref="ConceptoVentaFactura.Importe"/> (el SP no trae un total directo).
    /// </summary>
    public class DetalleVentaFactura
    {
        public string? Nombre { get; set; }
        public string? Rfc { get; set; }
        public string? Correo { get; set; }
        public string? SociedadMercantil { get; set; }
        public string? Domicilio { get; set; }
        public string? UsoCfdi { get; set; }
        public string? DescripcionUsoCfdi { get; set; }
        public string? FormaPago { get; set; }
        public string? DescripcionFormaPago { get; set; }
        public string? DomicilioFiscalReceptor { get; set; }
        public string? RegimenFiscalReceptor { get; set; }
        public string? DescripcionRegimenFiscalReceptor { get; set; }

        public List<ConceptoVentaFactura> Conceptos { get; set; } = new();

        /// <summary>Suma de <c>Conceptos[].Importe</c> (calculado en el repositorio).</summary>
        public decimal Total { get; set; }
    }
}
