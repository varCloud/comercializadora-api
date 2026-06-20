using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Auth
{
    /// <summary>
    /// Credenciales de inicio de sesión. Equivale al RequestLogin del backend legado.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contrasena { get; set; } = string.Empty;

        /// <summary>Dirección MAC de la estación. En clientes web no aplica (null).</summary>
        public string? MacAdress { get; set; }
    }
}
