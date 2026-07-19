namespace comercializadora_api.Services.FacturacionPac
{
    /// <summary>Firma XML-DSig (enveloped) de la solicitud de cancelación CFDI con el CSD del emisor.</summary>
    public interface ICfdiXmlSignerService
    {
        /// <summary>
        /// Firma el XML de cancelación (elemento raíz &lt;Cancelacion&gt;) y devuelve el documento
        /// completo con el nodo &lt;Signature&gt; anexado, serializado a string.
        /// </summary>
        string FirmarCancelacion(string xmlSinFirmar);
    }
}
