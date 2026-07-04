using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;
using Dapper;

namespace comercializadora_api.Repositories.ProduccionLiquidos
{
    /// <summary>
    /// Repositorio del reporte "Producción Líquidos" (Repository + Dapper + Stored Procedures).
    /// Migra <c>ProductosDAO.BuscarCargaMercanciaLiquidos</c> del legado. El SP legado
    /// (<c>SP_CONSULTA_CARGA_MERCANCIA_LIQUIDOS</c>) no pagina; el listado usa
    /// <c>SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS</c> (paginado) sin tocar el legado. Solo
    /// lectura: no hay alta/edición/baja en este módulo.
    /// </summary>
    public sealed class ProduccionLiquidosRepository : BaseRepository, IProduccionLiquidosRepository
    {
        public ProduccionLiquidosRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<CargaMercanciaLiquidos>> ListarAsync(ProduccionLiquidosQuery query)
        {
            var p = new DynamicParameters();
            // El legado nunca expone idTipoMovInventario en el form: siempre viaja NULL (todos los movimientos).
            p.Add("@idTipoMovInventario", null);
            p.Add("@idRol", query.IdRol);
            p.Add("@idUsuario", query.IdUsuario);
            p.Add("@fechaIni", query.FechaIni);
            p.Add("@fechaFin", query.FechaFin);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<CargaMercanciaLiquidos>("SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS", p);
        }
    }
}
