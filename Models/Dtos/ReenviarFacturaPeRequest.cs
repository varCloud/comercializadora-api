namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Reenvío de la factura de un pedido especial (PDF + XML) por correo. Espejo PE de
    /// <see cref="ReenviarFacturaRequest"/>; request propio (no flag sobre el de ventas) para
    /// que cada endpoint tipa su identificador sin ambigüedad.
    /// </summary>
    public class ReenviarFacturaPeRequest
    {
        public long IdPedidoEspecial { get; set; }

        /// <summary>Correo adicional en copia (opcional).</summary>
        public string? CorreoCopia { get; set; }
    }
}
