using System.Text.Json.Serialization;

namespace comercializadora_api.Models.Auth
{
    /// <summary>
    /// Sesión del usuario autenticado. Mapea el resultset de datos de SP_VALIDA_CONTRASENA
    /// y se enriquece con permisos, datos de empresa y el token JWT emitido por la API.
    /// </summary>
    public class Sesion
    {
        /// <summary>Token JWT emitido por la API tras validar la credencial.</summary>
        public string? Token { get; set; }

        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string? Usuario { get; set; }

        /// <summary>Contraseña almacenada (devuelta por el SP). No se expone en el JSON.</summary>
        [JsonIgnore]
        public string? Contrasena { get; set; }

        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? Telefono { get; set; }

        public int IdAlmacen { get; set; }
        public string? Almacen { get; set; }
        public int IdSucursal { get; set; }
        public string? Sucursal { get; set; }
        public string? Rol { get; set; }

        public int IdEstacion { get; set; }
        public bool UsuarioValido { get; set; }
        public decimal ComisionBancaria { get; set; }
        public int DiasParaHacerComplementos { get; set; }
        public int DevolucionesPermitidas { get; set; }
        public int AgregarProductosPermitidos { get; set; }

        /// <summary>Módulos a los que tiene acceso el rol.</summary>
        public List<Permiso> PermisosModulo { get; set; } = new();

        public string? DomicilioEmpresa { get; set; }
        public string? TelefonoEmpresa { get; set; }
        public string? RfcEmpresa { get; set; }
    }
}
