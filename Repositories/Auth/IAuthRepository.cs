using comercializadora_api.Models.Auth;
using comercializadora_api.Models.Common;

namespace comercializadora_api.Repositories.Auth
{
    /// <summary>
    /// Acceso a datos de autenticación. Equivale al LoginDAO del backend legado.
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// Valida la credencial contra SP_VALIDA_CONTRASENA. En éxito devuelve la sesión
        /// con permisos y datos de empresa; en error, solo estatus y mensaje.
        /// </summary>
        Task<Notificacion<Sesion>> ValidarUsuarioAsync(LoginRequest login);
    }
}
