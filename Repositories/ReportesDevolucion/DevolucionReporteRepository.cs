using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;
using Dapper;

namespace comercializadora_api.Repositories.ReportesDevolucion
{
    /// <summary>
    /// Implementación de <see cref="IDevolucionReporteRepository"/> (Repository + Dapper + Stored
    /// Procedure). Migra <c>ReportesDAO.ObtenerDevolucionesyComplementos</c> del legado, reutilizando
    /// <c>SP_CONSULTA_DEVOLUCIONES_Y_COMPLEMENTOS</c> sin cambios (decisión cerrada de la HU: no hay
    /// cambio de comportamiento que justifique un <c>SP_V2_*</c>).
    ///
    /// Nota de migración (verificado contra el SP real en BD, <c>sys.sql_modules</c>): el SP NO
    /// pagina (no soporta OFFSET/FETCH) — paginación en memoria desde el día uno, mismo patrón que
    /// <c>MermaReporteRepository</c>/<c>VentaReporteRepository</c>: un helper privado llama al SP
    /// una sola vez y <see cref="ListarAsync"/>/<see cref="ExportarAsync"/> lo comparten.
    ///
    /// <c>@idTipoConsulta</c> (filtro <c>TipoTicket</c> en <see cref="DevolucionQuery"/>) NO admite
    /// "todos": el SP arma dos <c>SELECT</c> unidos con <c>UNION</c>, cada uno condicionado a
    /// <c>@idTipoConsulta = 1</c> (Devoluciones) / <c>= 2</c> (Complementos); pasar <c>NULL</c> no
    /// activa ninguna rama y devuelve 0 filas. 0/null en la request se resuelve aquí a <c>1</c>
    /// (Devoluciones), igual que el default propio del SP (<c>@idTipoConsulta int = 1</c>).
    /// <c>@idVenta</c> no se expone como filtro de pantalla (fuera del contrato de la HU); siempre
    /// se envía <c>null</c> (SP trae todas las ventas del rango).
    /// </summary>
    public sealed class DevolucionReporteRepository : BaseRepository, IDevolucionReporteRepository
    {
        private const string StoredProcedure = "SP_CONSULTA_DEVOLUCIONES_Y_COMPLEMENTOS";

        public DevolucionReporteRepository(IDbConnectionFactory factory) : base(factory) { }

        public async Task<RawPage<DevolucionItem>> ListarAsync(DevolucionQuery filtros)
        {
            var todos = await ObtenerTodosAsync(filtros);
            if (!todos.EsExitoso)
                return RawPage<DevolucionItem>.Empty();

            var lista = todos.Modelo ?? new List<DevolucionItem>();
            int total = lista.Count;
            int page = filtros.Page < 1 ? 1 : filtros.Page;
            // El rango 5-100 del contrato de la HU ya lo acota ReportesDevolucionController.Listar
            // (antes de llamar aquí y a IPaginationBuilder, para que ambos usen el mismo valor); este
            // fallback solo cubre el caso defensivo de que el método se invoque sin pasar por ahí.
            int perPage = filtros.PerPage < 1 ? 20 : filtros.PerPage;
            var items = lista.Skip((page - 1) * perPage).Take(perPage).ToList();

            return new RawPage<DevolucionItem> { Items = items, Total = total };
        }

        /// <summary>
        /// Usado por <c>/exportar</c>: TODAS las filas que cumplen los filtros, sin paginar (a
        /// diferencia de <see cref="ListarAsync"/>, que sí pagina en memoria para el listado de
        /// pantalla). Comparte la consulta/mapeo vía <see cref="ObtenerTodosAsync"/> para no invocar
        /// el SP dos veces por request.
        /// </summary>
        public async Task<Notificacion<IEnumerable<DevolucionItem>>> ExportarAsync(DevolucionQuery filtros)
        {
            var todos = await ObtenerTodosAsync(filtros);
            return new Notificacion<IEnumerable<DevolucionItem>>
            {
                Estatus = todos.Estatus,
                Mensaje = todos.Mensaje,
                Modelo = todos.Modelo
            };
        }

        /// <summary>
        /// Llama a <c>SP_CONSULTA_DEVOLUCIONES_Y_COMPLEMENTOS</c> con los filtros de pantalla.
        /// 0/null en <c>IdAlmacen</c>/<c>IdUsuario</c> = TODOS (el SP hace <c>coalesce</c>); ver la
        /// nota de clase sobre <c>TipoTicket</c> (no admite "todos"). Los nombres de columna del
        /// resultset coinciden case-insensitive con <see cref="DevolucionItem"/>, así que Dapper
        /// mapea directo sin una clase de fila intermedia.
        /// </summary>
        private async Task<Notificacion<List<DevolucionItem>>> ObtenerTodosAsync(DevolucionQuery filtros)
        {
            var p = new DynamicParameters();
            p.Add("@idVenta", (int?)null);
            p.Add("@idAlmacen", filtros.IdAlmacen is null or 0 ? null : filtros.IdAlmacen);
            p.Add("@idUsuario", filtros.IdUsuario is null or 0 ? null : filtros.IdUsuario);
            p.Add("@fechaIni", filtros.FechaIni);
            p.Add("@fechaFin", filtros.FechaFin);
            p.Add("@idTipoConsulta", filtros.TipoTicket is null or 0 ? 1 : filtros.TipoTicket);

            var crudo = await ConsultarAsync<DevolucionItem>(StoredProcedure, p);
            return new Notificacion<List<DevolucionItem>>
            {
                Estatus = crudo.Estatus,
                Mensaje = crudo.Mensaje,
                Modelo = crudo.EsExitoso ? (crudo.Modelo ?? Enumerable.Empty<DevolucionItem>()).ToList() : null
            };
        }
    }
}
