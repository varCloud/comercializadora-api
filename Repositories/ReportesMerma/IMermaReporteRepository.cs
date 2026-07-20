using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ReportesMerma
{
    /// <summary>
    /// Acceso a datos del reporte "Merma". Migra <c>ReportesDAO.ObtenerMerma/ObtenerAnios/
    /// ObtenerMeses</c> del legado. El listado/exportación usan <c>SP_CONSULTA_MERMA</c> tal
    /// cual (sin <c>SP_V2_*</c>); los catálogos de años/meses reutilizan los SP legados
    /// <c>SP_CONSULTA_ANIOS</c>/<c>SP_CONSULTA_MESES</c> sin modificarlos.
    /// </summary>
    public interface IMermaReporteRepository
    {
        /// <summary>Listado paginado en memoria (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<MermaItem>> ListarAsync(MermaQuery filtros);

        /// <summary>Usado por <c>/exportar</c>: TODAS las filas que cumplen los filtros, sin paginar.</summary>
        Task<Notificacion<IEnumerable<MermaItem>>> ExportarAsync(MermaQuery filtros);

        /// <summary>Catálogo de años disponibles para el filtro (SP_CONSULTA_ANIOS, sin parámetros).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAniosAsync();

        /// <summary>Catálogo de meses disponibles para un año (SP_CONSULTA_MESES; null/0 = año actual).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMesesAsync(int? anio);
    }
}
