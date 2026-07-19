namespace comercializadora_api.Models.Dtos
{
    /// <summary>Reenvío de la factura (PDF + XML) por correo. Migra <c>FacturaController.ReenviarFactura</c>.</summary>
    public class ReenviarFacturaRequest
    {
        public long IdVenta { get; set; }

        /// <summary>Correo adicional en copia (opcional; el legado lo llamaba <c>correoAdicional</c>).</summary>
        public string? CorreoCopia { get; set; }
    }
}
