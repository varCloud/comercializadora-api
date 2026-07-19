using System.Xml.Linq;

namespace comercializadora_api.Services.FacturacionPac
{
    /// <summary>
    /// Construye el XML de solicitud de cancelación (elemento raíz &lt;Cancelacion&gt;, namespace
    /// <c>http://cancelacfd.sat.gob.mx</c>) que el legado armaba serializando
    /// <c>Models.Facturacion.CancelarCFDI40</c>. Se reconstruye a mano con <see cref="XElement"/>
    /// en vez de portar las clases de <c>XmlSerializer</c> (innecesarias para esta única forma).
    /// </summary>
    public static class CfdiCancelacionXml
    {
        private static readonly XNamespace Ns = "http://cancelacfd.sat.gob.mx";

        /// <summary>Motivo de cancelación fijo usado por el legado ("03" = comprobante emitido con errores sin relación).</summary>
        public const string MotivoCancelacion = "03";

        public static string Construir(string rfcEmisor, string uuid)
        {
            var doc = new XElement(Ns + "Cancelacion",
                new XAttribute("xmlns", Ns.NamespaceName),
                new XAttribute("Fecha", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                new XAttribute("RfcEmisor", rfcEmisor),
                new XElement(Ns + "Folios",
                    new XElement(Ns + "Folio",
                        new XAttribute("UUID", uuid),
                        new XAttribute("Motivo", MotivoCancelacion))));

            return doc.ToString(SaveOptions.DisableFormatting);
        }
    }
}
