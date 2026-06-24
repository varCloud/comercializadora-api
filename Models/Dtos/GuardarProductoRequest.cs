using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de producto (POST/PUT). Envuelve SP_V2_INSERTA_ACTUALIZA_PRODUCTOS,
    /// que persiste artículo y código de barras como CAMPOS SEPARADOS (migración P1).
    /// </summary>
    public class GuardarProductoRequest
    {
        /// <summary>0 o null = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdProducto { get; set; }

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int IdUnidadMedida { get; set; }

        [Range(1, int.MaxValue)]
        public int IdLineaProducto { get; set; }

        public float CantidadUnidadMedida { get; set; }

        /// <summary>Artículo (clave interna). Distinto del código de barras.</summary>
        [Required]
        public string Articulo { get; set; } = string.Empty;

        /// <summary>Código de barras (opcional, único entre activos).</summary>
        public string? CodigoBarras { get; set; }

        /// <summary>Clave SAT (cadena, p. ej. "47131600").</summary>
        [Required]
        public string ClaveProdServ { get; set; } = string.Empty;

        public int? IdUnidadCompra { get; set; }
        public int? CantidadUnidadCompra { get; set; }

        /// <summary>Estatus. En alta el SP lo fuerza a activo.</summary>
        public bool Activo { get; set; } = true;
    }
}
