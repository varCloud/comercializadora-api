using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ReportesInventario
{
    /// <summary>Reglas del reporte de Inventario. Por ahora delega directo en el repositorio (sin lógica de negocio adicional).</summary>
    public interface IInventarioReporteService
    {
        Task<RawPage<InventarioReporteItem>> ListarAsync(InventarioReporteQuery query);

        Task<Notificacion<IEnumerable<InventarioReporteExportItem>>> ExportarAsync(int tipo);
    }
}
