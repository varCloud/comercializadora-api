using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Enums;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Dashboard
{
    /// <summary>
    /// Repositorio del dashboard (Repository + Dapper + Stored Procedures). Migra DashboardDAO
    /// del legado. Los SP devuelven cabecera (status/mensaje) + un resultset de datos, igual
    /// que <see cref="BaseRepository.ConsultarAsync{T}"/>.
    /// </summary>
    /// <remarks>
    /// Los SP que devuelven <see cref="Categoria"/> se leen con un helper propio
    /// (<see cref="ConsultarCategoriasAsync"/>) porque la columna del SP se llama
    /// <c>categoria</c> y la propiedad C# no puede llamarse igual que su tipo (se llama
    /// <see cref="Categoria.NombreCategoria"/>); se proyecta la columna a mano. Los enums se
    /// pasan al SP como int. El @idEstacion se manda como NULL cuando vale 0 (igual que el legado).
    /// </remarks>
    public sealed class DashboardRepository : BaseRepository, IDashboardRepository
    {
        public DashboardRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerVentasPorFechaAsync(
            EnumTipoReporteGrafico tipoReporte, int idEstacion, DateTime? fechaConsulta = null)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoReporte", (int)tipoReporte);
            p.Add("@idEstacion", idEstacion == 0 ? (object?)null : idEstacion);
            // El SP hace coalesce(@fechaConsulta, dbo.FechaActual()): null => periodo actual.
            p.Add("@fechaConsulta", fechaConsulta);
            return ConsultarCategoriasAsync("SP_DASHBOARD_CONSULTA_TOTAL_VENTAS_POR_FECHA", p);
        }

        public Task<Notificacion<IEnumerable<EstacionVenta>>> ObtenerVentasPorEstacionAsync(
            DateTime? fechaIni, DateTime? fechaFin, int idEstacion)
        {
            var p = new DynamicParameters();
            p.Add("@fechaIni", fechaIni);
            p.Add("@fechaFin", fechaFin);
            p.Add("@idEstacion", idEstacion == 0 ? (object?)null : idEstacion);
            return ConsultarAsync<EstacionVenta>("SP_DASHBOARD_CONSULTA_TOTAL_VENTAS_POR_ESTACION", p);
        }

        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerTopTenAsync(
            EnumTipoReporteGrafico tipoReporte, EnumTipoGrafico tipoGrafico, int idEstacion)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoReporte", (int)tipoReporte);
            p.Add("@idTipoGrafico", (int)tipoGrafico);
            p.Add("@idEstacion", idEstacion == 0 ? (object?)null : idEstacion);
            return ConsultarCategoriasAsync("SP_DASHBOARD_CONSULTA_TOP_TEN", p);
        }

        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerInformacionGlobalAsync(
            EnumTipoReporteGrafico tipoReporte, int idEstacion)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoReporte", (int)tipoReporte);
            p.Add("@idEstacion", idEstacion == 0 ? (object?)null : idEstacion);
            return ConsultarCategoriasAsync("SP_DASHBOARD_CONSULTA_INFORMACION_GLOBAL", p);
        }

        public Task<Notificacion<IEnumerable<MermaMensual>>> ObtenerMermaAsync()
            => ConsultarAsync<MermaMensual>("SP_DASHBOARD_MERMA");

        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerIvaAcumuladoAsync(
            EnumTipoReporteGrafico tipoReporte, int idEstacion)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoReporte", (int)tipoReporte);
            p.Add("@idEstacion", idEstacion == 0 ? (object?)null : idEstacion);
            return ConsultarCategoriasAsync("SP_DASHBOARD_OBTENER_IVA_ACUMULADO", p);
        }

        public Task<Notificacion<IEnumerable<CostoProduccionMensual>>> ObtenerCostoProduccionAsync()
            => ConsultarAsync<CostoProduccionMensual>("SP_DASHBOARD_COSTO_PRODUCCION");

        /// <summary>
        /// Lee un SP del dashboard que devuelve cabecera (status/mensaje) + un resultset de
        /// categorías. Proyecta cada fila a <see cref="Categoria"/> a mano porque la columna
        /// <c>categoria</c> no puede mapear por nombre a la propiedad <c>NombreCategoria</c>
        /// (sin modificar el SP). El resto de columnas se leen de forma case-insensitive.
        /// </summary>
        private async Task<Notificacion<IEnumerable<Categoria>>> ConsultarCategoriasAsync(
            string storedProcedure, object? parametros)
        {
            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<Categoria>>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (!notificacion.EsExitoso)
                return notificacion;

            var crudas = (await multi.ReadAsync()).ToList();
            notificacion.Modelo = crudas
                .Select(f => new Dictionary<string, object>(
                    (IDictionary<string, object>)f, StringComparer.OrdinalIgnoreCase))
                .Select(f => new Categoria
                {
                    Id = f.TryGetValue("id", out var id) ? Convert.ToInt32(id) : 0,
                    NombreCategoria = f.TryGetValue("categoria", out var c) ? c?.ToString() : null,
                    Total = f.TryGetValue("total", out var t) ? Convert.ToSingle(t) : 0f,
                    TotalPE = f.TryGetValue("totalPE", out var tpe) ? Convert.ToSingle(tpe) : 0f,
                    FechaIni = f.TryGetValue("fechaIni", out var fi) && fi is not null
                        ? Convert.ToDateTime(fi) : default,
                    FechaFin = f.TryGetValue("fechaFin", out var ff) && ff is not null
                        ? Convert.ToDateTime(ff) : default
                })
                .ToList();

            return notificacion;
        }
    }
}
