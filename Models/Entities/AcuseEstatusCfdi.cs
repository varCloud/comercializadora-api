namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Respuesta del servicio público del SAT "Consulta de Estatus de CFDI"
    /// (<c>ConsultaEstatusFactura4</c> en el legado). Mapea el tipo <c>Acuse</c> del WSDL
    /// (namespace <c>Sat.Cfdi.Negocio.ConsultaCfdi.Servicio</c>).
    /// </summary>
    public class AcuseEstatusCfdi
    {
        public string? CodigoEstatus { get; set; }
        public string? EsCancelable { get; set; }
        public string? Estado { get; set; }
        public string? EstatusCancelacion { get; set; }
        public string? ValidacionEfos { get; set; }
    }
}
