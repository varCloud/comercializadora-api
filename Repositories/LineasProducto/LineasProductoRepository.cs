using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.LineasProducto
{
    /// <summary>
    /// Repositorio del submenú "Líneas de producto" (Repository + Dapper + Stored Procedures).
    /// Migra LineaProductoDAO del legado. Listado paginado con SP_V2_CONSULTA_LINEAS_PRODUCTO;
    /// alta/edición con SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO (unicidad de descripción);
    /// estatus con SP_V2_ACTUALIZA_STATUS_LINEAS_PRODUCTO (bloquea baja con productos asociados).
    /// </summary>
    public sealed class LineasProductoRepository : BaseRepository, ILineasProductoRepository
    {
        public LineasProductoRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<LineaProducto>> ListarAsync(PagedQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idLineaProducto", 0);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<LineaProducto>("SP_V2_CONSULTA_LINEAS_PRODUCTO", p);
        }

        public async Task<Notificacion<LineaProducto>> ObtenerPorIdAsync(int idLineaProducto)
        {
            var p = new DynamicParameters();
            p.Add("@idLineaProducto", idLineaProducto);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_LINEAS_PRODUCTO", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<LineaProducto>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<LineaProducto>();

            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarLineaProductoRequest linea)
        {
            var p = new DynamicParameters();
            p.Add("@idLineaProducto", linea.IdLineaProducto);
            p.Add("@descripcion", linea.Descripcion?.Trim());
            p.Add("@activo", linea.Activo);
            return EjecutarAsync("SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO", p);
        }

        public Task<Notificacion<string>> CambiarEstatusAsync(int idLineaProducto, bool activo)
        {
            var p = new DynamicParameters();
            p.Add("@idLineaProducto", idLineaProducto);
            p.Add("@activo", activo);
            return EjecutarAsync("SP_V2_ACTUALIZA_STATUS_LINEAS_PRODUCTO", p);
        }
    }
}
