using comercializadora_api.Models.Auth;
using comercializadora_api.Models.Common;
using comercializadora_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Autenticación: inicio de sesión (emite JWT) y consulta de la identidad actual.
    /// Migra el WsLoginController del backend legado.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// Valida usuario/contraseña contra SP_VALIDA_CONTRASENA y, si es correcto, devuelve
        /// la sesión con un token JWT. Réplica del login legado + emisión de token.
        /// El resultado de negocio viaja en el <see cref="Notificacion{T}"/> (Estatus/Mensaje).
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public Task<Notificacion<Sesion>> Login([FromBody] LoginRequest login)
            => _authService.LoginAsync(login);

        /// <summary>
        /// Endpoint de prueba protegido: devuelve los claims del token. Sirve para validar
        /// que el esquema JWT Bearer está correctamente configurado.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public Dictionary<string, string> Me()
            => User.Claims.ToDictionary(c => c.Type, c => c.Value);
    }
}
