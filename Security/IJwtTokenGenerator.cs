using comercializadora_api.Models.Auth;

namespace comercializadora_api.Security
{
    /// <summary>Emite tokens JWT a partir de una sesión validada.</summary>
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Sesion sesion);
    }
}
