using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;
using Dapper;

namespace comercializadora_api.Repositories.ReportesInventario
{
    /// <summary>
    /// Implementación de <see cref="IInventarioReporteRepository"/> (Repository + Dapper +
    /// Stored Procedures). Migra <c>ReportesDAO.ObtenerInventario</c> /
    /// <c>ReportesDAO.ObtenerReporteGeneral</c> del legado, unificados en un solo SP nuevo
    /// (<c>SP_V2_CONSULTA_INVENTARIO</c>) que decide listado-paginado vs. export-completo con
    /// <c>@exportar</c>.
    /// </summary>
    public sealed class InventarioReporteRepository : BaseRepository, IInventarioReporteRepository
    {
        private const string StoredProcedure = "SP_V2_CONSULTA_INVENTARIO";

        public InventarioReporteRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<InventarioReporteItem>> ListarAsync(InventarioReporteQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idLineaProducto", query.IdLineaProducto);
            p.Add("@idAlmacen", query.IdAlmacen);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@fechaIni", query.FechaIni);
            p.Add("@fechaFin", query.FechaFin);
            p.Add("@page", query.Page);
            p.Add("@perPage", query.PerPage);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@exportar", false);
            p.Add("@tipo", 1);
            return ConsultarPaginaAsync<InventarioReporteItem>(StoredProcedure, p);
        }

        public Task<Notificacion<IEnumerable<InventarioReporteExportItem>>> ExportarAsync(int tipo)
        {
            var p = new DynamicParameters();
            p.Add("@exportar", true);
            p.Add("@tipo", tipo);
            return ConsultarAsync<InventarioReporteExportItem>(StoredProcedure, p);
        }
    }
}
