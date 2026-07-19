using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ReportesInventario
{
    /// <summary>
    /// Repositorio del reporte de Inventario (primera sub-feature del módulo "Reportes"
    /// legado). Ambos métodos llaman a <c>SP_V2_CONSULTA_INVENTARIO</c>, variando <c>@exportar</c>.
    /// </summary>
    public interface IInventarioReporteRepository
    {
        /// <summary>Listado paginado con filtros de pantalla (<c>@exportar = 0</c>).</summary>
        Task<RawPage<InventarioReporteItem>> ListarAsync(InventarioReporteQuery query);

        /// <summary>
        /// Exportación completa (<c>@exportar = 1</c>): ignora filtros/paginación, devuelve
        /// TODO el inventario con el set de columnas de <paramref name="tipo"/> (1 General / 2 Ubicación).
        /// </summary>
        Task<Notificacion<IEnumerable<InventarioReporteExportItem>>> ExportarAsync(int tipo);
    }
}
