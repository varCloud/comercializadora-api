namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Línea de producto (familia/taxonomía del catálogo). Mapea el resultset de
    /// SP_V2_CONSULTA_LINEAS_PRODUCTO. Cada Producto pertenece a una línea
    /// (Productos.idLineaProducto). Submenú de mantenimiento bajo Productos.
    /// </summary>
    public class LineaProducto
    {
        public int IdLineaProducto { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }
}
