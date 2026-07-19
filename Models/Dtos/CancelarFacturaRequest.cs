namespace comercializadora_api.Models.Dtos
{
    /// <summary>Cancelación de factura ante el PAC. Migra <c>FacturaController.CancelarFactura</c>.</summary>
    public class CancelarFacturaRequest
    {
        public long IdVenta { get; set; }
    }
}
