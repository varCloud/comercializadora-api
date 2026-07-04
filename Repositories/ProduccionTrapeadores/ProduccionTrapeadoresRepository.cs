using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;
using Dapper;

namespace comercializadora_api.Repositories.ProduccionTrapeadores
{
    /// <summary>
    /// Repositorio del reporte "Producción Trapeadores" (Repository + Dapper + Stored Procedures).
    /// Reutiliza el mismo SP paginado del reporte hermano "Producción Líquidos"
    /// (<c>SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS</c>) sin crear ni modificar ningún SP: el
    /// procedimiento ya es genérico vía <c>@idTipoMovInventario</c> (NULL/0 → IN (26,27) para
    /// líquidos; &gt;0 → coincidencia exacta). Aquí se fija <c>@idTipoMovInventario = 32</c>
    /// (trapeadores) hardcodeado, sin exponerlo como parámetro de la query ni del front. Solo
    /// lectura: no hay alta/edición/baja en este módulo.
    /// </summary>
    public sealed class ProduccionTrapeadoresRepository : BaseRepository, IProduccionTrapeadoresRepository
    {
        private const int IdTipoMovInventarioTrapeadores = 32;

        public ProduccionTrapeadoresRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<CargaMercanciaTrapeadores>> ListarAsync(ProduccionTrapeadoresQuery query)
        {
            var p = new DynamicParameters();
            // Fijo: reporte de Trapeadores = idTipoMovInventario 32 (coincidencia exacta en el SP).
            p.Add("@idTipoMovInventario", IdTipoMovInventarioTrapeadores);
            p.Add("@idRol", query.IdRol);
            p.Add("@idUsuario", query.IdUsuario);
            p.Add("@fechaIni", query.FechaIni);
            p.Add("@fechaFin", query.FechaFin);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<CargaMercanciaTrapeadores>("SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS", p);
        }
    }
}
