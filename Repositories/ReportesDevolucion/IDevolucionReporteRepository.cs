using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ReportesDevolucion
{
    /// <summary>
    /// Acceso a datos del reporte "Devoluciones y Complementos". Migra
    /// <c>ReportesDAO.ObtenerDevolucionesyComplementos</c> del legado. Listado/exportación usan
    /// <c>SP_CONSULTA_DEVOLUCIONES_Y_COMPLEMENTOS</c> tal cual (sin <c>SP_V2_*</c>, decisión de la
    /// HU).
    /// </summary>
    public interface IDevolucionReporteRepository
    {
        /// <summary>Listado paginado en memoria (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<DevolucionItem>> ListarAsync(DevolucionQuery filtros);

        /// <summary>Usado por <c>/exportar</c>: TODAS las filas que cumplen los filtros, sin paginar.</summary>
        Task<Notificacion<IEnumerable<DevolucionItem>>> ExportarAsync(DevolucionQuery filtros);
    }
}
