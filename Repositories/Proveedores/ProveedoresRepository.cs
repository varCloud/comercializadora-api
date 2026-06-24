using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Proveedores
{
    /// <summary>
    /// Repositorio del módulo de Proveedores (Repository + Dapper + Stored Procedures).
    /// Migra ProveedorDAO del legado. El listado usa SP_V2_CONSULTA_PROVEEDORES (paginado);
    /// guardar/estatus reutilizan los SP legados sin modificarlos.
    /// </summary>
    public sealed class ProveedoresRepository : BaseRepository, IProveedoresRepository
    {
        public ProveedoresRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<Proveedor>> ListarAsync(PagedQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idProveedor", 0);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<Proveedor>("SP_V2_CONSULTA_PROVEEDORES", p);
        }

        public async Task<Notificacion<Proveedor>> ObtenerPorIdAsync(int idProveedor)
        {
            var p = new DynamicParameters();
            p.Add("@idProveedor", idProveedor);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_PROVEEDORES", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Proveedor>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<Proveedor>();

            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarProveedorRequest proveedor)
        {
            var p = new DynamicParameters();
            p.Add("@idProveedor", proveedor.IdProveedor);
            p.Add("@nombre", proveedor.Nombre);
            p.Add("@descripcion", proveedor.Descripcion);
            p.Add("@telefono", proveedor.Telefono);
            p.Add("@direccion", proveedor.Direccion);
            p.Add("@activo", proveedor.Activo);
            return EjecutarAsync("SP_INSERTA_ACTUALIZA_PROVEEDORES", p);
        }

        public Task<Notificacion<string>> CambiarEstatusAsync(int idProveedor, bool activo)
        {
            var p = new DynamicParameters();
            p.Add("@idProveedor", idProveedor);
            p.Add("@activo", activo);
            return EjecutarAsync("SP_ACTUALIZA_STATUS_PROVEEDOR", p);
        }
    }
}
