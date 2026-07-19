using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ConsumoMpl
{
    /// <summary>
    /// Acceso a datos del reporte "Consumo de MPL" (Costo de Producción Agranel). Migra
    /// <c>ReportesDAO.ObtenerReporteCostoProduccionAgranel/ObtenerAnios/ObtenerMeses</c> del
    /// legado. El listado usa <c>SP_V2_CONSULTA_COSTO_PRODUCCION</c> (paginado, nuevo); los
    /// catálogos de años/meses reutilizan los SP legados <c>SP_CONSULTA_ANIOS</c>/
    /// <c>SP_CONSULTA_MESES</c> sin modificarlos.
    /// </summary>
    public interface IConsumoMplRepository
    {
        /// <summary>Listado paginado del reporte (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<CostoProduccionAgranel>> ListarAsync(ConsumoMplQuery query);

        /// <summary>Catálogo de años disponibles para el filtro (SP_CONSULTA_ANIOS, sin parámetros).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAniosAsync();

        /// <summary>Catálogo de meses disponibles para un año (SP_CONSULTA_MESES; null/0 = año actual).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMesesAsync(int? anio);
    }
}
