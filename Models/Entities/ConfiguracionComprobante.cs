namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Datos fiscales del emisor (empresa). Mapea <c>SP_FACTURACION_OBTENER_CONFIGURACION_COMPROBANTE</c>
    /// (tabla singleton <c>FactConfiguracionComprobante</c>). Se usa para obtener el RFC emisor
    /// al construir la "expresión impresa" de consulta de estatus ante el SAT.
    /// </summary>
    public class ConfiguracionComprobante
    {
        public string? Rfc { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Domicilio { get; set; }
        public string? RegimenFiscal { get; set; }
        public string? LugarExpedicion { get; set; }
    }
}
