using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Usuarios
{
    /// <summary>
    /// Lógica de negocio del módulo de Usuarios. Orquesta el repositorio y aplica las reglas
    /// que en el legado vivían en el DAO/controller (normalización de nombres, conservar la
    /// contraseña en edición cuando llega vacía, etc.).
    /// </summary>
    public interface IUsuariosService
    {
        Task<RawPage<Usuario>> ListarAsync(UsuariosQuery query);

        Task<Notificacion<Usuario>> ObtenerPorIdAsync(int idUsuario);
        Task<Notificacion<string>> GuardarAsync(GuardarUsuarioRequest usuario);
        Task<Notificacion<string>> CambiarEstatusAsync(int idUsuario, bool activo);

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRolesAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen);
    }
}
