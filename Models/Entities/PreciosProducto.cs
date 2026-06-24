namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Precios de un producto: los precios base (tabla Productos) + los rangos de mayoreo
    /// (ProductosPorPrecio). Lo arma SP_V2_CONSULTA_PRECIOS_PRODUCTO (resultsets 2 y 3).
    /// Nota: los nombres de campo son los del legado; las ETIQUETAS correctas se ponen en el
    /// front (precioIndividual = "Precio Menudeo", precioMenudeo = "Precio Mayoreo").
    /// </summary>
    public class PreciosProducto
    {
        public decimal? PrecioIndividual { get; set; }
        public decimal? PrecioMenudeo { get; set; }
        public decimal? UltimoCostoCompra { get; set; }
        public double? PorcUtilidadIndividual { get; set; }
        public double? PorcUtilidadMayoreo { get; set; }

        public IEnumerable<RangoPrecio> Rangos { get; set; } = new List<RangoPrecio>();
    }
}
