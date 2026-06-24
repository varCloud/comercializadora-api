using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de proveedor (POST/PUT). Envuelve SP_INSERTA_ACTUALIZA_PROVEEDORES.
    /// </summary>
    public class GuardarProveedorRequest
    {
        /// <summary>0 o null = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdProveedor { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public string Direccion { get; set; } = string.Empty;

        /// <summary>Estatus activo/inactivo. En alta el SP lo fuerza a activo.</summary>
        public bool Activo { get; set; } = true;
    }
}
