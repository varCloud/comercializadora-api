using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ReportesDevolucion
{
    /// <summary>Lógica del reporte "Devoluciones y Complementos". 100% lectura.</summary>
    public interface IDevolucionReporteService
    {
        Task<RawPage<DevolucionItem>> ListarAsync(DevolucionQuery filtros);
        Task<Notificacion<IEnumerable<DevolucionItem>>> ExportarAsync(DevolucionQuery filtros);
    }
}
