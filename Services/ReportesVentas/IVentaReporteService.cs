using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ReportesVentas
{
    /// <summary>Reglas del reporte de Ventas. Por ahora delega directo en el repositorio (sin lógica de negocio adicional).</summary>
    public interface IVentaReporteService
    {
        /// <summary>Listado paginado en memoria (ver <see cref="Repositories.ReportesVentas.IVentaReporteRepository"/>).</summary>
        Task<RawPage<VentaReporteItem>> ListarAsync(VentaReporteQuery filtros);

        /// <summary>Todas las filas que cumplen los filtros, sin paginar (usado por <c>/exportar</c>).</summary>
        Task<Notificacion<IEnumerable<VentaReporteItem>>> ExportarAsync(VentaReporteQuery filtros);
    }
}
