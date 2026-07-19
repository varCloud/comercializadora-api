namespace comercializadora_api.Services.Facturas
{
    /// <summary>
    /// Resolución de archivos de factura (PDF/XML). El SP guarda <c>pathArchivoFactura</c> como
    /// ruta relativa (p. ej. <c>/Facturas/2026/JUNIO/Factura_768923_...pdf</c>); el legado
    /// (<c>FacturaDAO.ObtenerFacturas</c>) le anteponía <c>urlDominio</c> (appSetting) para armar
    /// la URL pública. <see cref="RutaBaseArchivos"/> es la carpeta física/UNC donde viven esos
    /// mismos PDF/XML, necesaria SOLO para leer el <c>Sello</c> del XML timbrado al construir la
    /// "expresión impresa" de consulta de estatus ante el SAT (ver
    /// <c>FacturacionService.ConsultarEstatusCancelacionAsync</c>).
    /// </summary>
    public class FacturacionArchivosOptions
    {
        public const string SectionName = "FacturacionArchivos";

        /// <summary>Dominio público a anteponer a <c>pathArchivoFactura</c> (por ambiente).</summary>
        public string UrlDominio { get; set; } = string.Empty;

        /// <summary>
        /// Carpeta física/UNC donde viven los PDF/XML de facturas (equivalente al servidor de
        /// archivos del legado). Si no está configurada, la consulta de estatus de cancelación
        /// responde con un error de negocio claro en vez de fallar por IO.
        /// </summary>
        public string RutaBaseArchivos { get; set; } = string.Empty;

        /// <summary>
        /// Lista de correos en copia oculta para todo reenvío de factura (legado:
        /// appSetting <c>correoCCFacturas</c>, separado por comas).
        /// </summary>
        public string CorreoCopiaFacturas { get; set; } = string.Empty;

        public IEnumerable<string> ObtenerCorreosCopia()
            => CorreoCopiaFacturas.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
