using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using comercializadora_api.Models.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace comercializadora_api.Security
{
    /// <summary>
    /// Genera un access token JWT firmado (HMAC-SHA256) con los claims de la sesión.
    /// </summary>
    public sealed class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _settings;

        public JwtTokenGenerator(IOptions<JwtSettings> settings) => _settings = settings.Value;

        public string GenerateToken(Sesion sesion)
        {
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, sesion.IdUsuario.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("idUsuario", sesion.IdUsuario.ToString()),
                new("idRol", sesion.IdRol.ToString()),
                new("idSucursal", sesion.IdSucursal.ToString()),
                new("idAlmacen", sesion.IdAlmacen.ToString()),
                new("idEstacion", sesion.IdEstacion.ToString()),
                new("usuario", sesion.Usuario ?? string.Empty),
                new("nombre", sesion.Nombre ?? string.Empty),
            };

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiraMinutos),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
