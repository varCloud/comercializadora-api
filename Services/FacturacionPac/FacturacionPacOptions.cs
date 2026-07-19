namespace comercializadora_api.Services.FacturacionPac
{
    /// <summary>
    /// Endpoints del PAC (edifactmx-pac.com) para cancelación CFDI y del SAT para consulta de
    /// estatus. Reemplaza los appSettings <c>FacturarPro</c> / URLs embebidas en los Web References
    /// <c>cancelaCFDI4Prod</c>/<c>cancelaCFDITest</c> del legado. Sin credenciales aquí: la
    /// autenticación de la cancelación es la firma XML-DSig con el CSD (ver
    /// <see cref="CfdiSignatureOptions"/>), no un usuario/password de SOAP.
    /// </summary>
    public class FacturacionPacOptions
    {
        public const string SectionName = "FacturacionPac";

        /// <summary>true = cancelaCFDI4Prod (producción); false = cancelaCFDITest (pruebas). Igual semántica que "FacturarPro" del legado.</summary>
        public bool UsarProduccion { get; set; }

        public string UrlCancelacionProduccion { get; set; } = "https://www.edifactmx-pac.com:443/serviceCFDI/cancelaCFDI.php";
        public string UrlCancelacionPruebas { get; set; } = string.Empty;

        /// <summary>Servicio público del SAT (ConsultaEstatusFactura4). No requiere credenciales.</summary>
        public string UrlConsultaEstatusSat { get; set; } = "https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc";

        public int TimeoutSegundos { get; set; } = 30;
    }
}
