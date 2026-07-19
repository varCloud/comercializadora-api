using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.FacturacionPac
{
    /// <summary>
    /// Clientes SOAP del PAC (cancelación CFDI) y del SAT (consulta de estatus). Migra
    /// <c>Web References\cancelaCFDI4Prod</c>/<c>cancelaCFDITest</c> (rpc/encoded, SOAP 1.1 —
    /// ASMX <c>SoapHttpClientProtocol</c>, no portable a WCF: .NET no soporta rpc/encoded) y
    /// <c>Connected Services\ConsultaEstatusFactura4</c> (document/literal wrapped) mediante
    /// llamadas SOAP crudas con <see cref="HttpClient"/> en vez de proxies generados, replicando
    /// el mismo binding/endpoint/SOAPAction que el legado (ver comentarios de implementación).
    /// </summary>
    public interface IFacturacionPacClient
    {
        /// <summary>
        /// Envía el XML de cancelación ya firmado (XML-DSig) a <c>enviaAcuseCancelacion</c> y
        /// devuelve el <c>EstatusUUID</c> del acuse (201 = en proceso, 202 = ya enviada,
        /// cualquier otro = error). Migra <c>ProcesaCfdi.CancelarFacturaEdifact</c> +
        /// <c>ObtnerAcuseCancelacionFactura</c>.
        /// </summary>
        Task<int> EnviarAcuseCancelacionAsync(string xmlFirmado, CancellationToken cancellationToken = default);

        /// <summary>
        /// Consulta el estatus del CFDI ante el servicio público del SAT. Migra
        /// <c>ConsultaEstatusFactura4.ConsultaCFDIServiceClient.Consulta</c>.
        /// </summary>
        Task<AcuseEstatusCfdi> ConsultarEstatusAsync(string expresionImpresa, CancellationToken cancellationToken = default);
    }
}
