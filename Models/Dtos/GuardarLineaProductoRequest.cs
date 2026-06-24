using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de una línea de producto (POST/PUT). Envuelve
    /// SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO, que valida unicidad de la descripción
    /// y devuelve error explícito ante duplicado.
    /// </summary>
    public class GuardarLineaProductoRequest
    {
        /// <summary>0 = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdLineaProducto { get; set; }

        /// <summary>Descripción de la línea (única; columna varchar(50) en BD).</summary>
        [Required]
        [MaxLength(50)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>Estatus. En alta el SP lo fuerza a activo.</summary>
        public bool Activo { get; set; } = true;
    }
}
