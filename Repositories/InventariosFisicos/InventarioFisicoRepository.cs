using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.InventariosFisicos
{
    /// <summary>
    /// Repositorio del módulo "Inventario Físico" (Repository + Dapper + Stored Procedures).
    /// Migra la parte web de <c>InventarioFisicoDAO</c> del legado (los métodos móviles
    /// <c>SP_APP_*</c> quedan fuera de alcance). El listado usa
    /// <c>SP_V2_CONSULTA_INVENTARIO_FISICO</c> (paginado, nuevo); guardar/estatus/ajustes
    /// reutilizan los SP legados sin modificarlos. Los SP legados de escritura devuelven la
    /// cabecera en columnas <c>Estatus</c>/<c>Mensaje</c> (mayúscula inicial), por lo que se
    /// leen con un diccionario case-insensitive en <see cref="EjecutarLegadoAsync"/> (mismo
    /// precedente que ProduccionAgranelRepository).
    /// </summary>
    public sealed class InventarioFisicoRepository : BaseRepository, IInventarioFisicoRepository
    {
        private const string SpListado = "SP_V2_CONSULTA_INVENTARIO_FISICO";
        private const string SpGuardar = "SP_INSERTA_ACTUALIZA_INVENTARIO_FISICO";
        private const string SpEstatus = "SP_ACTUALIZA_ESTATUS_INVENTARIO_FISICO";
        private const string SpAjustes = "SP_CONSULTA_AJUSTE_INVENTARIO";

        public InventarioFisicoRepository(IDbConnectionFactory factory) : base(factory) { }

        public async Task<RawPage<InventarioFisico>> ListarAsync(InventarioFisicoQuery query, int idSucursal)
        {
            var p = new DynamicParameters();
            p.Add("@idSucursal", idSucursal == 0 ? (int?)null : idSucursal);
            p.Add("@idTipoInventario", query.IdTipoInventario is null or 0 ? null : query.IdTipoInventario);
            p.Add("@fechaIni", query.FechaIni);
            p.Add("@fechaFin", query.FechaFin);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpListado, p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            if ((int)cabecera.status != 200)
                return RawPage<InventarioFisico>.Empty();

            // Multi-mapping sucursal/estatus anidados, igual que el DAO legado (que además
            // mapeaba Usuario, no requerido por la pantalla nueva). GridReader solo expone el
            // multi-mapping en su versión síncrona; los datos ya están leídos.
            var items = multi.Read<InventarioFisico, Sucursal, Status, InventarioFisico>(
                (inventario, sucursal, estatus) =>
                {
                    inventario.Sucursal = sucursal;
                    inventario.Estatus = estatus;
                    return inventario;
                },
                splitOn: "idSucursal,idStatus").ToList();

            return new RawPage<InventarioFisico> { Items = items, Total = (int)cabecera.total };
        }

        public Task<Notificacion<string>> GuardarAsync(int idInventarioFisico, string nombre, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idInventarioFisico", idInventarioFisico);
            p.Add("@nombre", nombre);
            p.Add("@idUsuario", idUsuario);
            return EjecutarLegadoAsync(SpGuardar, p);
        }

        public Task<Notificacion<string>> ActualizarEstatusAsync(
            int idInventarioFisico, int idEstatus, string? observaciones, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idInventarioFisico", idInventarioFisico);
            p.Add("@idEstatusInventarioFisico", idEstatus);
            p.Add("@idUsuario", idUsuario);
            p.Add("@observaciones", observaciones ?? string.Empty);
            return EjecutarLegadoAsync(SpEstatus, p);
        }

        public async Task<Notificacion<IEnumerable<AjusteInventarioFisico>>> ObtenerAjustesAsync(
            int idInventarioFisico, AjustesInventarioQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idInventarioFisico", idInventarioFisico);
            // @idProducto no se envía (default null en el SP): el diálogo no filtra por producto.
            p.Add("@idLineaProducto", query.IdLineaProducto is null or 0 ? null : query.IdLineaProducto);
            p.Add("@idAlmacen", query.IdAlmacen is null or 0 ? null : query.IdAlmacen);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpAjustes, p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<AjusteInventarioFisico>>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje,
                Modelo = new List<AjusteInventarioFisico>()
            };

            // El SP devuelve -1 + "No se encontraron resultados." cuando no hay filas (el DAO
            // legado lo traducía a lista vacía); se pasa tal cual con Modelo = [] para que el
            // front decida (tabla vacía, no error).
            if (!notificacion.EsExitoso)
                return notificacion;

            // Multi-mapping producto anidado (splitOn idProducto). Las columnas finales
            // idUsuario/nombreCompleto del SP se ignoran: el diálogo migrado no las muestra
            // (el DAO legado las mapeaba a un tercer objeto Usuario).
            notificacion.Modelo = multi.Read<AjusteInventarioFisico, ProductoAjusteInventario, AjusteInventarioFisico>(
                (ajuste, producto) =>
                {
                    ajuste.Producto = producto;
                    return ajuste;
                },
                splitOn: "idProducto").ToList();

            return notificacion;
        }

        /// <summary>
        /// Ejecuta un SP legado cuya fila de estatus viene en columnas <c>Estatus</c>/<c>Mensaje</c>
        /// (mayúscula inicial, y con columnas extra error_procedure/error_line). Se lee a un
        /// diccionario case-insensitive para no depender del casing exacto del SP (el
        /// <c>EjecutarAsync</c> de BaseRepository espera <c>status</c>/<c>mensaje</c> en minúsculas).
        /// </summary>
        private async Task<Notificacion<string>> EjecutarLegadoAsync(string storedProcedure, object parametros)
        {
            using IDbConnection db = CreateConnection();
            var fila = await db.QueryFirstAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            var columnas = new Dictionary<string, object>(
                (IDictionary<string, object>)fila, StringComparer.OrdinalIgnoreCase);

            return new Notificacion<string>
            {
                Estatus = Convert.ToInt32(columnas["estatus"]),
                Mensaje = columnas.TryGetValue("mensaje", out var m) ? m?.ToString() : null
            };
        }
    }
}
