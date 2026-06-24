using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Usuarios
{
    /// <summary>
    /// Repositorio del módulo de Usuarios (Repository + Dapper + Stored Procedures).
    /// Migra UsuarioDAO del legado. El listado usa SP_V2_CONSULTA_USUARIOS (paginado);
    /// el resto reutiliza los SP existentes sin modificarlos.
    /// </summary>
    public sealed class UsuariosRepository : BaseRepository, IUsuariosRepository
    {
        public UsuariosRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<Usuario>> ListarAsync(UsuariosQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idUsuario", 0);
            p.Add("@idAlmacen", query.IdAlmacen);
            p.Add("@idRol", query.IdRol);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<Usuario>("SP_V2_CONSULTA_USUARIOS", p);
        }

        public async Task<Notificacion<Usuario>> ObtenerPorIdAsync(int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idUsuario", idUsuario);
            p.Add("@idAlmacen", 0);
            p.Add("@idRol", 0);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_USUARIOS", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Usuario>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<Usuario>();

            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarUsuarioRequest usuario)
        {
            var p = new DynamicParameters();
            p.Add("@idUsuario", usuario.IdUsuario);
            p.Add("@idRol", usuario.IdRol);
            p.Add("@usuario", usuario.NombreUsuario);
            p.Add("@telefono", usuario.Telefono);
            p.Add("@contrasena", usuario.Contrasena);
            p.Add("@idAlmacen", usuario.IdAlmacen);
            p.Add("@idSucursal", usuario.IdSucursal);
            p.Add("@nombre", usuario.Nombre);
            p.Add("@apellidoPaterno", usuario.ApellidoPaterno ?? string.Empty);
            p.Add("@apellidoMaterno", usuario.ApellidoMaterno ?? string.Empty);
            p.Add("@fecha_alta", DateTime.Now);
            p.Add("@activo", usuario.Activo);
            return EjecutarAsync("SP_INSERTA_ACTUALIZA_USUARIOS", p);
        }

        public Task<Notificacion<string>> CambiarEstatusAsync(int idUsuario, bool activo)
        {
            var p = new DynamicParameters();
            p.Add("@idUsuario", idUsuario);
            p.Add("@activo", activo);
            return EjecutarAsync("SP_ACTUALIZA_STATUS_USUARIO", p);
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRolesAsync()
        {
            // El SP legado solo devuelve datos cuando @idRol = 1 (gate heredado); filtra idRol <> 1.
            var p = new DynamicParameters();
            p.Add("@idRol", 1);
            return ConsultarCatalogoAsync("SP_CONSULTA_ROLES", p, "idRol");
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync()
            => ConsultarCatalogoAsync("SP_CONSULTA_SUCURSALES", null, "idSucursal");

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen)
        {
            var p = new DynamicParameters();
            p.Add("@idSucursal", idSucursal);
            p.Add("@idTipoAlmacen", idTipoAlmacen);
            return ConsultarCatalogoAsync("SP_CONSULTA_ALMACENES", p, "idAlmacen");
        }
    }
}
