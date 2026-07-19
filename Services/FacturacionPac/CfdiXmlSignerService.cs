using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Services.FacturacionPac
{
    /// <summary>
    /// Puerto directo de <c>Utilerias.ProcesaCfdi.GenerateXmlSignature</c> del legado: firma
    /// enveloped (Reference Uri="", transform XmlDsigEnvelopedSignatureTransform) con
    /// <see cref="KeyInfoX509Data"/> (certificado + issuer/serial). Única diferencia real: el
    /// legado cargaba el .pfx como recurso embebido del assembly; aquí se lee de un archivo
    /// configurado (<see cref="CfdiSignatureOptions.RutaCertificadoPfx"/>) para no versionar el
    /// certificado ni su password.
    ///
    /// ⚠️ No verificado end-to-end contra el PAC real (instrucción explícita: no cancelar
    /// facturas reales). Antes de usar en producción, validar con un XML de prueba que el
    /// certificado carga y que la firma resultante es aceptada por <c>enviaAcuseCancelacion</c>.
    /// </summary>
    public sealed class CfdiXmlSignerService : ICfdiXmlSignerService
    {
        private readonly CfdiSignatureOptions _opciones;

        public CfdiXmlSignerService(IOptions<CfdiSignatureOptions> opciones) => _opciones = opciones.Value;

        public string FirmarCancelacion(string xmlSinFirmar)
        {
            if (string.IsNullOrWhiteSpace(_opciones.RutaCertificadoPfx))
                throw new InvalidOperationException(
                    "Falta configurar FacturacionCfdi:RutaCertificadoPfx (ruta al .pfx del CSD) en User Secrets/appsettings.");

            var doc = new XmlDocument { PreserveWhitespace = false };
            doc.LoadXml(xmlSinFirmar);

            using X509Certificate2 certificado = X509CertificateLoader.LoadPkcs12FromFile(
                _opciones.RutaCertificadoPfx, _opciones.PasswordCertificado, X509KeyStorageFlags.MachineKeySet);

            if (certificado.GetRSAPrivateKey() is not RSA llave)
                throw new InvalidOperationException("El certificado configurado no tiene una llave privada RSA.");

            var signedXml = new SignedXml(doc) { SigningKey = llave };

            var reference = new Reference { Uri = string.Empty };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            var keyInfoData = new KeyInfoX509Data(certificado);
            keyInfoData.AddIssuerSerial(certificado.Issuer, certificado.SerialNumber);
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();
            XmlElement firma = signedXml.GetXml();

            doc.DocumentElement!.AppendChild(doc.ImportNode(firma, true));
            return doc.OuterXml;
        }
    }
}
