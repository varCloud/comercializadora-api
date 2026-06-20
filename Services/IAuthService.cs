using comercializadora_api.Models.Auth;
using comercializadora_api.Models.Common;

namespace comercializadora_api.Services
{
    /// <summary>Reglas de negocio de autenticación: valida credenciales y emite el JWT.</summary>
    public interface IAuthService
    {
        Task<Notificacion<Sesion>> LoginAsync(LoginRequest login);
    }
}
