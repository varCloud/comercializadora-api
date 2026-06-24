namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Producto del catálogo maestro. Mapea el resultset de SP_V2_CONSULTA_PRODUCTOS
    /// (catálogo puro, Fase A) y el de los SP de búsqueda (descripción / código de barras).
    /// Subset de la entidad legada: solo los campos del CRUD core + derivados de consulta.
    /// </summary>
    public class Producto
    {
        public int IdProducto { get; set; }
        public string? Descripcion { get; set; }

        public int IdUnidadMedida { get; set; }
        public string? DescripcionUnidadMedida { get; set; }

        public int IdLineaProducto { get; set; }
        public string? DescripcionLinea { get; set; }

        public float CantidadUnidadMedida { get; set; }

        /// <summary>Artículo (clave interna). Separado del código de barras (migración P1).</summary>
        public string? Articulo { get; set; }

        /// <summary>Código de barras. Separado del artículo (migración P1).</summary>
        public string? CodigoBarras { get; set; }

        /// <summary>Clave SAT (cadena, p. ej. "47131600"); no es id numérico.</summary>
        public string? ClaveProdServ { get; set; }

        public int? IdUnidadCompra { get; set; }
        public string? DescripcionUnidadCompra { get; set; }
        public float? CantidadUnidadCompra { get; set; }

        public decimal? PrecioIndividual { get; set; }
        public decimal? PrecioMenudeo { get; set; }
        public decimal? UltimoCostoCompra { get; set; }

        public bool Activo { get; set; }

        /// <summary>Indica si la línea del producto permite fraccionar (dbo.LineaProductoFraccion).</summary>
        public bool Fraccion { get; set; }
    }
}
