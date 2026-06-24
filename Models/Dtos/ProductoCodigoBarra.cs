namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Ítem de la lista que se envía al generador de etiquetas de código de barras
    /// (POST api/productos/codigos-barras/generar). Subconjunto de <c>Producto</c> con lo
    /// que se imprime en la etiqueta. Migra el payload de EvtCodigosBarras.js del legado.
    /// </summary>
    public sealed class ProductoCodigoBarra
    {
        public int IdProducto { get; set; }
        public string? Descripcion { get; set; }
        public string? DescripcionLinea { get; set; }

        /// <summary>Precio rotulado como "Menudeo" en la etiqueta (naming legado).</summary>
        public decimal? PrecioIndividual { get; set; }

        /// <summary>Precio rotulado como "Mayoreo" en la etiqueta (naming legado).</summary>
        public decimal? PrecioMenudeo { get; set; }

        /// <summary>Cadena que se codifica en la barra (CODE_128).</summary>
        public string? CodigoBarras { get; set; }
    }
}
