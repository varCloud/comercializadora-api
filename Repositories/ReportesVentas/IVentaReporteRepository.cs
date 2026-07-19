using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ReportesVentas
{
    /// <summary>
    /// Repositorio del reporte de Ventas (segundo sub-reporte del módulo "Reportes" legado).
    /// Único método: llama <c>SP_CONSULTA_VENTAS</c> tal cual (sin crear <c>SP_V2_*</c> — no hay
    /// cambio de comportamiento que versionar), replicando <c>ReportesDAO.ObtenerVentas</c>.
    /// Corrección post-aprobación (2026-07-19): pagina en memoria (el SP no soporta
    /// OFFSET/FETCH), mismo patrón que <c>LimitesInventarioRepository.ListarAsync</c>.
    /// </summary>
    public interface IVentaReporteRepository
    {
        /// <summary>
        /// Trae todas las filas del SP con los filtros de pantalla, las mapea y devuelve la
        /// página solicitada (<c>Skip/Take</c> en memoria) + el total real de filas.
        /// </summary>
        Task<RawPage<VentaReporteItem>> ListarAsync(VentaReporteQuery filtros);

        /// <summary>Todas las filas que cumplen los filtros, sin paginar (usado por <c>/exportar</c>).</summary>
        Task<Notificacion<IEnumerable<VentaReporteItem>>> ExportarAsync(VentaReporteQuery filtros);
    }
}
