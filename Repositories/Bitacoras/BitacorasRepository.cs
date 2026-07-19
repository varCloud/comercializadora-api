using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Bitacoras
{
    /// <summary>
    /// Repositorio del reporte "Bitácoras" (Repository + Dapper + Stored Procedures). Migra
    /// <c>BitacoraDAO</c> del legado (métodos <c>ObtenerPedidosInternos</c>,
    /// <c>ObtenerDetallePedidosInternos</c>, <c>ObtenerStatusPedidosInternos</c>). El listado y el
    /// detalle usan SP_V2_* nuevos (paginado / doble resultset) sin tocar los SP legados; el
    /// catálogo de estatus reutiliza el SP legado <c>SP_CONSULTA_ESTATUS_PEDIDOS_INTERNOS</c>
    /// (dos resultsets: cabecera + datos idStatus/descripcion). Solo lectura. Las regiones
    /// <c>SP_APP_*</c> del DAO legado (app móvil handheld) quedan fuera de alcance.
    /// </summary>
    public sealed class BitacorasRepository : BaseRepository, IBitacorasRepository
    {
        private const string SpListado = "SP_V2_CONSULTA_PEDIDOS_INTERNOS";
        private const string SpDetalle = "SP_V2_CONSULTA_DETALLE_PEDIDOS_INTERNOS";
        private const string SpEstatus = "SP_CONSULTA_ESTATUS_PEDIDOS_INTERNOS";

        public BitacorasRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<Bitacora>> ListarAsync(BitacorasQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idPedidoInterno", query.IdPedidoInterno);
            p.Add("@idEstatusPedidoInterno", query.IdEstatusPedidoInterno);
            p.Add("@idAlmacenOrigen", query.IdAlmacenOrigen);
            p.Add("@idAlmacenDestino", query.IdAlmacenDestino);
            p.Add("@idUsuario", query.IdUsuario);
            p.Add("@idProducto", query.IdProducto);
            p.Add("@fechaIni", query.FechaIni);
            p.Add("@fechaFin", query.FechaFin);
            // @idTipoPedidoInterno usa su default (1) del SP: el reporte web solo muestra tipo 1.
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<Bitacora>(SpListado, p);
        }

        public Task<Notificacion<IEnumerable<BitacoraDetalle>>> ObtenerDetalleAsync(int idPedidoInterno)
        {
            var p = new DynamicParameters();
            p.Add("@idPedidoInterno", idPedidoInterno);
            return ConsultarAsync<BitacoraDetalle>(SpDetalle, p);
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
        {
            // SP legado: cabecera (status, mensaje) + resultset (idStatus, descripcion) con los
            // estatus de CatEstatusPedidoInterno. La UI antepone la opción "TODOS".
            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpEstatus, null, commandType: CommandType.StoredProcedure);

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
                    Id = Convert.ToInt32(f["idStatus"]),
                    Descripcion = f.TryGetValue("descripcion", out var d) ? d?.ToString() : null
                })
                .ToList();

            return notificacion;
        }
    }
}
