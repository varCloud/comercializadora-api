using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Usuarios
{
    /// <summary>
    /// Acceso a datos del módulo de Usuarios. Envuelve los stored procedures del dominio
    /// (SP_V2_CONSULTA_USUARIOS para el listado paginado y los SP legados reutilizados).
    /// </summary>
    public interface IUsuariosRepository
    {
        /// <summary>Listado paginado de usuarios (SP_V2_CONSULTA_USUARIOS). Devuelve filas + total.</summary>
        Task<RawPage<Usuario>> ListarAsync(UsuariosQuery query);

        /// <summary>Obtiene un usuario por id (SP_V2_CONSULTA_USUARIOS con @idUsuario).</summary>
        Task<Notificacion<Usuario>> ObtenerPorIdAsync(int idUsuario);

        /// <summary>Alta o edición (SP_INSERTA_ACTUALIZA_USUARIOS).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarUsuarioRequest usuario);

        /// <summary>Activa/desactiva (borrado lógico) (SP_ACTUALIZA_STATUS_USUARIO).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idUsuario, bool activo);

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRolesAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen);
    }
}
