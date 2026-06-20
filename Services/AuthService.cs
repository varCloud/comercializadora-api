using comercializadora_api.Models.Auth;
using comercializadora_api.Models.Common;
using comercializadora_api.Repositories;
using comercializadora_api.Security;

namespace comercializadora_api.Services
{
    /// <summary>
    /// Orquesta el login: valida la credencial vía repositorio (SP_VALIDA_CONTRASENA) y,
    /// si es exitoso, emite el access token JWT y lo adjunta al modelo de sesión.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthService(IAuthRepository authRepository, IJwtTokenGenerator tokenGenerator)
        {
            _authRepository = authRepository;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<Notificacion<Sesion>> LoginAsync(LoginRequest login)
        {
            var resultado = await _authRepository.ValidarUsuarioAsync(login);

            if (resultado.EsExitoso && resultado.Modelo is not null)
                resultado.Modelo.Token = _tokenGenerator.GenerateToken(resultado.Modelo);

            return resultado;
        }
    }
}
