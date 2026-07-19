namespace comercializadora_api.Models.Dtos
{
    /// <summary>Cancelación ante el PAC de la factura de un pedido especial. Espejo PE de <see cref="CancelarFacturaRequest"/>.</summary>
    public class CancelarFacturaPeRequest
    {
        public long IdPedidoEspecial { get; set; }
    }
}
