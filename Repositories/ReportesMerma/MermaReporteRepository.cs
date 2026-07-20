using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.ReportesMerma
{
    /// <summary>
    /// Implementación de <see cref="IMermaReporteRepository"/> (Repository + Dapper + Stored
    /// Procedure). Migra <c>ReportesDAO.ObtenerMerma</c> del legado, reutilizando
    /// <c>SP_CONSULTA_MERMA</c> sin cambios (decisión cerrada de la HU: no hay cambio de
    /// comportamiento que justifique un <c>SP_V2_*</c>).
    ///
    /// Nota de migración (verificado contra el SP real en BD): <c>SP_CONSULTA_MERMA</c> NO pagina
    /// (no soporta OFFSET/FETCH) — solo recibe <c>@mesCalculo</c>/<c>@anioCalculo</c>/<c>@idLinea</c>/
    /// <c>@idAlmacen</c>/<c>@silent</c> (este último se deja en su default 0: en 1 el SP omite hasta
    /// el resultset de cabecera). Paginación en memoria desde el día uno (a diferencia de
    /// <c>reporte_ventas</c>, que la agregó como corrección posterior): mismo patrón ya usado en
    /// <c>VentaReporteRepository</c> — un helper privado llama al SP una sola vez y
    /// <see cref="ListarAsync"/>/<see cref="ExportarAsync"/> lo comparten. El SP tiene un efecto
    /// colateral de cálculo/caché en la tabla <c>ReporteMerma</c> (guardado con <c>IF NOT EXISTS</c>,
    /// idempotente) — no es un problema invocarlo más de una vez por request.
    ///
    /// Comportamiento confirmado sin filtro de año/mes: el propio SP hace
    /// <c>coalesce(@anioCalculo, year(fechaActual))</c> / <c>coalesce(@mesCalculo, month(fechaActual))</c>,
    /// es decir usa el mes/año calendario ACTUAL (no "el más reciente con datos").
    /// </summary>
    public sealed class MermaReporteRepository : BaseRepository, IMermaReporteRepository
    {
        private const string SpMerma = "SP_CONSULTA_MERMA";
        private const string SpAnios = "SP_CONSULTA_ANIOS";
        private const string SpMeses = "SP_CONSULTA_MESES";

        public MermaReporteRepository(IDbConnectionFactory factory) : base(factory) { }

        public async Task<RawPage<MermaItem>> ListarAsync(MermaQuery filtros)
        {
            var todos = await ObtenerTodosAsync(filtros);
            if (!todos.EsExitoso)
                return RawPage<MermaItem>.Empty();

            var lista = todos.Modelo ?? new List<MermaItem>();
            int total = lista.Count;
            int page = filtros.Page < 1 ? 1 : filtros.Page;
            int perPage = filtros.PerPage < 1 ? 10 : filtros.PerPage;
            var items = lista.Skip((page - 1) * perPage).Take(perPage).ToList();

            return new RawPage<MermaItem> { Items = items, Total = total };
        }

        /// <summary>
        /// Usado por <c>/exportar</c>: TODAS las filas que cumplen los filtros, sin paginar (a
        /// diferencia de <see cref="ListarAsync"/>, que sí pagina en memoria para el listado de
        /// pantalla). Comparte la consulta/mapeo vía <see cref="ObtenerTodosAsync"/> para no
        /// invocar <c>SP_CONSULTA_MERMA</c> dos veces por request. Conserva <c>Estatus</c>/
        /// <c>Mensaje</c> del SP (incluido el caso "No se encontraron resultados.").
        /// </summary>
        public async Task<Notificacion<IEnumerable<MermaItem>>> ExportarAsync(MermaQuery filtros)
        {
            var todos = await ObtenerTodosAsync(filtros);
            return new Notificacion<IEnumerable<MermaItem>>
            {
                Estatus = todos.Estatus,
                Mensaje = todos.Mensaje,
                Modelo = todos.Modelo
            };
        }

        /// <summary>
        /// Llama a <c>SP_CONSULTA_MERMA</c> con los 4 filtros de pantalla. 0/null = TODOS (mismo
        /// criterio que <c>ReportesDAO.ObtenerMerma</c> del legado, que también convertía 0 a
        /// <c>DBNull</c>). Los nombres de columna del resultset (<c>idReporteMerma</c>,
        /// <c>inventarioFinalMesAnt</c>, <c>codigoBarras</c>, <c>descripcionProducto</c>,
        /// <c>descripcionLinea</c>, …) coinciden case-insensitive con <see cref="MermaItem"/>, así
        /// que Dapper mapea directo sin una clase de fila intermedia.
        /// </summary>
        private async Task<Notificacion<List<MermaItem>>> ObtenerTodosAsync(MermaQuery filtros)
        {
            var p = new DynamicParameters();
            p.Add("@mesCalculo", filtros.MesCalculo is null or 0 ? null : filtros.MesCalculo);
            p.Add("@anioCalculo", filtros.AnioCalculo is null or 0 ? null : filtros.AnioCalculo);
            p.Add("@idLinea", filtros.IdLineaProducto is null or 0 ? null : filtros.IdLineaProducto);
            p.Add("@idAlmacen", filtros.IdAlmacen is null or 0 ? null : filtros.IdAlmacen);

            var crudo = await ConsultarAsync<MermaItem>(SpMerma, p);
            return new Notificacion<List<MermaItem>>
            {
                Estatus = crudo.Estatus,
                Mensaje = crudo.Mensaje,
                Modelo = crudo.EsExitoso ? (crudo.Modelo ?? Enumerable.Empty<MermaItem>()).ToList() : null
            };
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
        /// un diccionario case-insensitive. Mismo precedente que
        /// <c>ConsumoMplRepository.LeerCatalogoValueTextAsync</c> (SP de catálogo genérico
        /// compartidos entre reportes, incl. el futuro "Costo de Producción") — se duplica el
        /// helper en vez de compartirlo entre repositorios, siguiendo la convención ya
        /// establecida en este repo de no crear abstracciones cruzadas prematuras.
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
