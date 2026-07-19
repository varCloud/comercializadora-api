namespace comercializadora_api.Services.FacturacionPac
{
    /// <summary>
    /// Certificado de Sello Digital (CSD) usado para firmar la solicitud de cancelación CFDI
    /// (XML-DSig, igual que <c>ProcesaCfdi.GenerateXmlSignature</c> del legado). El legado tenía
    /// el .pfx embebido como recurso del assembly (<c>Resources/archivo2022_pfx.pfx</c>) y la
    /// contraseña en <c>Web.config</c> (<c>claveGeneraSellolluvia</c>); aquí se externalizan a
    /// archivo + User Secrets (nunca versionados). Ver bloqueo documentado en la memoria del
    /// módulo: el .pfx real y su password deben copiarse manualmente, no se portan automáticamente.
    /// </summary>
    public class CfdiSignatureOptions
    {
        public const string SectionName = "FacturacionCfdi";

        /// <summary>Ruta absoluta (o UNC) al .pfx del CSD. Configurar por ambiente, nunca versionar el archivo.</summary>
        public string RutaCertificadoPfx { get; set; } = string.Empty;

        /// <summary>Password del .pfx. SIEMPRE en User Secrets/variables de entorno.</summary>
        public string PasswordCertificado { get; set; } = string.Empty;
    }
}
