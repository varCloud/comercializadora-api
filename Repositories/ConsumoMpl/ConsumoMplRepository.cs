using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.ConsumoMpl
{
    /// <summary>
    /// Repositorio del reporte "Consumo de MPL" (Repository + Dapper + Stored Procedures). Migra
    /// las partes de <c>ReportesDAO</c> relacionadas con el indicador de costo de producción a
    /// granel. El listado usa <c>SP_V2_CONSULTA_COSTO_PRODUCCION</c> (paginado, nuevo, preserva
    /// la lógica de cálculo/caché del original); los catálogos de años/meses reutilizan los SP
    /// legados tal cual.
    /// </summary>
    public sealed class ConsumoMplRepository : BaseRepository, IConsumoMplRepository
    {
        private const string SpListado = "SP_V2_CONSULTA_COSTO_PRODUCCION";
        private const string SpAnios   = "SP_CONSULTA_ANIOS";
        private const string SpMeses   = "SP_CONSULTA_MESES";

        public ConsumoMplRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<CostoProduccionAgranel>> ListarAsync(ConsumoMplQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@mesCalculo", query.MesCalculo is null or 0 ? null : query.MesCalculo);
            p.Add("@anioCalculo", query.AnioCalculo is null or 0 ? null : query.AnioCalculo);
            p.Add("@idLinea", query.IdLineaProducto is null or 0 ? null : query.IdLineaProducto);
            p.Add("@idAlmacen", query.IdAlmacen is null or 0 ? null : query.IdAlmacen);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<CostoProduccionAgranel>(SpListado, p);
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAniosAsync()
        {
            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpAnios, null, commandType: CommandType.StoredProcedure);
            return await LeerCatalogoValueTextAsync(multi);
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMesesAsync(int? anio)
        {
            var p = new DynamicParameters();
            p.Add("@anio", anio is null or 0 ? null : anio);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpMeses, p, commandType: CommandType.StoredProcedure);
            return await LeerCatalogoValueTextAsync(multi);
        }

        /// <summary>
        /// SP_CONSULTA_ANIOS / SP_CONSULTA_MESES devuelven cabecera (status, mensaje, +
        /// error_procedure/error_line que se ignoran) y datos en columnas "Value"/"Text" (patrón
        /// SelectListItem del legado, no "id"/"descripcion"), por lo que no se puede reusar
        /// BaseRepository.ConsultarCatalogoAsync/ConsultarAsync tal cual; se lee manualmente con
        /// un diccionario case-insensitive, mismo precedente que
        /// ProduccionAgranelRepository.ObtenerEstatusAsync.
        /// </summary>
        private static async Task<Notificacion<IEnumerable<CatalogoItem>>> LeerCatalogoValueTextAsync(
            SqlMapper.GridReader multi)
        {
            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<CatalogoItem>>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (!notificacion.EsExitoso)
                return notificacion;

            var filas = (await multi.ReadAsync())
                .Select(f => new Dictionary<string, object>(
                    (IDictionary<string, object>)f, StringComparer.OrdinalIgnoreCase));

            notificacion.Modelo = filas
                .Select(f => new CatalogoItem
                {
                    Id = Convert.ToInt32(f["value"]),
                    Descripcion = f.TryGetValue("text", out var t) ? t?.ToString() : null
                })
                .ToList();

            return notificacion;
        }
    }
}
