using System.Text.Json.Serialization;

namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Usuario del sistema. Mapea el resultset de datos de SP_V2_CONSULTA_USUARIOS.
    /// Migra el modelo Usuario del backend legado.
    /// </summary>
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }

        /// <summary>
        /// Nombre de login. En el contrato JSON viaja como "usuario" (igual que el legado);
        /// la propiedad usa otro nombre porque C# no permite un miembro igual al tipo.
        /// El SP lo devuelve aliaseado como "nombreUsuario" para que Dapper lo mapee aquí.
        /// </summary>
        [JsonPropertyName("usuario")]
        public string? NombreUsuario { get; set; }

        /// <summary>Contraseña almacenada (texto plano heredado). No se expone en el JSON.</summary>
        [JsonIgnore]
        public string? Contrasena { get; set; }

        public string? Telefono { get; set; }
        public int IdAlmacen { get; set; }
        public int IdSucursal { get; set; }
        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public DateTime? FechaAlta { get; set; }
        public bool Activo { get; set; }

        // Descripciones (joins) para mostrar en la tabla
        public string? DescripcionRol { get; set; }
        public string? DescripcionSucursal { get; set; }
        public string? DescripcionAlmacen { get; set; }
        public string? NombreCompleto { get; set; }
    }
}
