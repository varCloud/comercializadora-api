namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Concepto (renglón) de la venta, tal como lo devuelve el segundo resultset de detalle de
    /// <c>SP_FACTURACION_OBTENER_DETALLE_VENTA</c>. Se usa solo para calcular el total mostrado
    /// en el modal de reenvío; la timbrado/generación de CFDI queda fuera de alcance.
    /// </summary>
    public class ConceptoVentaFactura
    {
        public string? ClaveProdserv { get; set; }
        public string? ClaveUnidad { get; set; }
        public decimal Cantidad { get; set; }
        public string? Unidad { get; set; }
        public string? NoIdentificacion { get; set; }
        public string? Descripcion { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Importe { get; set; }
    }
}
