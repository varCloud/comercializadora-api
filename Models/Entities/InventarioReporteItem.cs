namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del listado paginado de "Reportes &gt; Inventario". Mapea el resultset de datos de
    /// <c>SP_V2_CONSULTA_INVENTARIO</c> en modo listado (<c>@exportar = 0</c>): paridad de
    /// columnas con el legado <c>SP_CONSULTA_INVENTARIO</c> (Fecha, Almacén, Línea de Producto,
    /// Producto, Código de barras, Cantidad a la fecha, Costo de compra).
    /// </summary>
    public class InventarioReporteItem
    {
        public DateTime Fecha { get; set; }
        public string? Almacen { get; set; }
        public string? DescripcionLinea { get; set; }
        public string? Descripcion { get; set; }
        public string? CodigoBarras { get; set; }
        public double Cantidad { get; set; }
        public decimal Costo { get; set; }
    }
}
