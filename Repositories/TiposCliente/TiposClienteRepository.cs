using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.TiposCliente
{
    /// <summary>
    /// Repositorio del catálogo Tipos de cliente (Repository + Dapper + Stored Procedures).
    /// Migra la parte de tipos/descuentos de ClienteDAO del legado. Listado paginado con
    /// SP_V2_CONSULTA_TIPOS_CLIENTES; guardar/estatus reutilizan los SP legados sin
    /// modificarlos (su cabecera sí es status/mensaje en minúsculas → EjecutarAsync).
    /// </summary>
    public sealed class TiposClienteRepository : BaseRepository, ITiposClienteRepository
    {
        public TiposClienteRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<TipoCliente>> ListarAsync(PagedQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoCliente", 0);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<TipoCliente>("SP_V2_CONSULTA_TIPOS_CLIENTES", p);
        }

        public async Task<Notificacion<TipoCliente>> ObtenerPorIdAsync(int idTipoCliente)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoCliente", idTipoCliente);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_TIPOS_CLIENTES", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<TipoCliente>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<TipoCliente>();

            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarTipoClienteRequest tipoCliente)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoCliente", tipoCliente.IdTipoCliente);
            p.Add("@descripcion", tipoCliente.Descripcion?.Trim());
            p.Add("@descuento", tipoCliente.Descuento);
            p.Add("@activo", tipoCliente.Activo);
            return EjecutarAsync("SP_INSERTA_ACTUALIZA_TIPOS_CLIENTES", p);
        }

        public Task<Notificacion<string>> CambiarEstatusAsync(int idTipoCliente, bool activo)
        {
            var p = new DynamicParameters();
            p.Add("@idTipoCliente", idTipoCliente);
            p.Add("@activo", activo);
            return EjecutarAsync("SP_ACTUALIZA_STATUS_TIPOS_CLIENTES", p);
        }
    }
}
