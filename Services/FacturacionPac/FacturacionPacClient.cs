using System.Text;
using System.Xml.Linq;
using comercializadora_api.Models.Entities;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Services.FacturacionPac
{
    /// <summary>
    /// Implementación HTTP cruda de <see cref="IFacturacionPacClient"/>. Ver interfaz para el
    /// contexto de por qué no se usan proxies generados (rpc/encoded no es soportado por WCF).
    ///
    /// ⚠️ No verificado contra el PAC/SAT reales en esta migración (instrucción explícita: solo
    /// build, no cancelar facturas reales). El envelope de <see cref="EnviarAcuseCancelacionAsync"/>
    /// se reconstruyó a partir de los atributos <c>SoapRpcMethodAttribute</c> del proxy ASMX
    /// legado (<c>Web References\cancelaCFDI4Prod\Reference.cs</c>): wrapper
    /// <c>CallenviaAcuseCancelacion</c> (nombre del método .NET, NO el nombre de operación del
    /// WSDL "enviaAcuseCancelacion" — el proxy legado los desacopla), namespace
    /// <c>http://edifact.com.mx/xsd</c>, parámetro <c>xmlFile</c>, respuesta con un elemento
    /// <c>return</c>. Antes de usarlo en producción: probar con un XML de prueba contra
    /// <c>UrlCancelacionPruebas</c> y ajustar el envelope si el PAC lo rechaza.
    /// </summary>
    public sealed class FacturacionPacClient : IFacturacionPacClient
    {
        private const string EdifactNs = "http://edifact.com.mx/xsd";
        private const string SoapEnvNs = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string SatTempuriNs = "http://tempuri.org/";

        private readonly HttpClient _http;
        private readonly FacturacionPacOptions _opciones;

        public FacturacionPacClient(HttpClient http, IOptions<FacturacionPacOptions> opciones)
        {
            _http = http;
            _opciones = opciones.Value;
            _http.Timeout = TimeSpan.FromSeconds(_opciones.TimeoutSegundos <= 0 ? 30 : _opciones.TimeoutSegundos);
        }

        public async Task<int> EnviarAcuseCancelacionAsync(string xmlFirmado, CancellationToken cancellationToken = default)
        {
            string url = _opciones.UsarProduccion ? _opciones.UrlCancelacionProduccion : _opciones.UrlCancelacionPruebas;
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException(
                    "Falta configurar FacturacionPac:UrlCancelacionProduccion/UrlCancelacionPruebas.");

            var envelope = new XElement(XNamespace.Get(SoapEnvNs) + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soap", SoapEnvNs),
                new XElement(XNamespace.Get(SoapEnvNs) + "Body",
                    new XElement(XNamespace.Get(EdifactNs) + "CallenviaAcuseCancelacion",
                        new XAttribute("xmlns", EdifactNs),
                        new XElement("xmlFile", xmlFirmado))));

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(envelope.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml")
            };
            request.Headers.Add("SOAPAction", $"\"{url}/enviaAcuseCancelacion\"");

            using var response = await _http.SendAsync(request, cancellationToken);
            string cuerpo = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            // La respuesta trae, dentro de <return>, el XML del acuse (<Acuse Fecha="" RfcEmisor="">
            // <Folios><UUID/><EstatusUUID/></Folios>...</Acuse>). Se extrae por local-name para no
            // depender del prefijo/namespace exacto que use el PAC.
            var respuestaDoc = XDocument.Parse(cuerpo);
            string? acuseXml = respuestaDoc.Descendants()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, "return", StringComparison.OrdinalIgnoreCase))
                ?.Value;

            if (string.IsNullOrWhiteSpace(acuseXml))
                throw new InvalidOperationException("El PAC no devolvió un acuse de cancelación reconocible.");

            var acuse = XDocument.Parse(acuseXml);
            string? estatusUuid = acuse.Descendants()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, "EstatusUUID", StringComparison.OrdinalIgnoreCase))
                ?.Value;

            if (!int.TryParse(estatusUuid, out int resultado))
                throw new InvalidOperationException($"Acuse de cancelación sin EstatusUUID reconocible: {acuseXml}");

            return resultado;
        }

        public async Task<AcuseEstatusCfdi> ConsultarEstatusAsync(string expresionImpresa, CancellationToken cancellationToken = default)
        {
            string url = _opciones.UrlConsultaEstatusSat;

            var envelope = new XElement(XNamespace.Get(SoapEnvNs) + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soap", SoapEnvNs),
                new XElement(XNamespace.Get(SoapEnvNs) + "Body",
                    new XElement(XNamespace.Get(SatTempuriNs) + "Consulta",
                        new XAttribute("xmlns", SatTempuriNs),
                        new XElement("expresionImpresa", expresionImpresa))));

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(envelope.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "text/xml")
            };
            request.Headers.Add("SOAPAction", "\"http://tempuri.org/IConsultaCFDIService/Consulta\"");

            using var response = await _http.SendAsync(request, cancellationToken);
            string cuerpo = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            var doc = XDocument.Parse(cuerpo);
            string? Leer(string nombre) => doc.Descendants()
                .FirstOrDefault(e => string.Equals(e.Name.LocalName, nombre, StringComparison.OrdinalIgnoreCase))
                ?.Value;

            return new AcuseEstatusCfdi
            {
                CodigoEstatus = Leer("CodigoEstatus"),
                EsCancelable = Leer("EsCancelable"),
                Estado = Leer("Estado"),
                EstatusCancelacion = Leer("EstatusCancelacion"),
                ValidacionEfos = Leer("ValidacionEFOS")
            };
        }
    }
}
