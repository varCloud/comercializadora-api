namespace comercializadora_api.Models.Auth
{
    /// <summary>
    /// Datos de la empresa para el comprobante (FactConfiguracionComprobante), 4º resultset
    /// de SP_VALIDA_CONTRASENA. Solo se mapean los campos que la sesión necesita.
    /// </summary>
    public class EmpresaComprobante
    {
        public string? Rfc { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Domicilio { get; set; }
    }
}
