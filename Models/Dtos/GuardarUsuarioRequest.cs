using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de usuario (POST/PUT). Envuelve SP_INSERTA_ACTUALIZA_USUARIOS.
    /// A diferencia del legado, los catálogos viajan como enteros simples (el viejo front
    /// los mandaba como arreglos idRolGuardar[0]/idAlmacenGuardar[0]/idSucursalGuardar[0]).
    /// </summary>
    public class GuardarUsuarioRequest
    {
        /// <summary>0 o null = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdUsuario { get; set; }

        [Required]
        [JsonPropertyName("usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña en texto plano (heredado). En edición puede venir vacía para
        /// "conservar la actual"; el servicio resuelve ese caso.
        /// </summary>
        public string? Contrasena { get; set; }

        public string? Telefono { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un rol.")]
        public int IdRol { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una sucursal.")]
        public int IdSucursal { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un almacén.")]
        public int IdAlmacen { get; set; }

        /// <summary>Estatus activo/inactivo. En alta el SP lo fuerza a activo.</summary>
        public bool Activo { get; set; } = true;
    }
}
