namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Renglón del reporte "Merma" (menú legado "Reportes &gt; Merma"). Mapea el resultset de
    /// datos de <c>SP_CONSULTA_MERMA</c> (tabla <c>ReporteMerma</c> + join a
    /// <c>Productos</c>/<c>LineaProducto</c>), reusado tal cual. Nombres de propiedad iguales
    /// (case-insensitive) a las columnas que devuelve el SP para que Dapper las mapee
    /// automáticamente. No incluye <c>errorHumano</c> (columna interna de la tabla, sin uso en
    /// pantalla) ni <c>idAlmacen</c> (es filtro de entrada, no columna del resultset: el SP no
    /// agrega el producto por almacén, solo filtra su existencia en él).
    /// </summary>
    public class MermaItem
    {
        public int IdReporteMerma { get; set; }
        public int IdProducto { get; set; }
        public double InventarioFinalMesAnt { get; set; }
        public double TotalCompras { get; set; }
        public double InventarioSistema { get; set; }
        public double Merma { get; set; }
        public double PorcMerma { get; set; }
        public decimal UltCostoCompra { get; set; }
        public decimal CostoMerma { get; set; }
        public DateTime UltimoDiaMesCalculo { get; set; }
        public DateTime UltimoDiaMesAnterior { get; set; }
        public DateTime FechaAlta { get; set; }
        public string? CodigoBarras { get; set; }
        public string? DescripcionProducto { get; set; }
        public int IdLineaProducto { get; set; }
        public string? DescripcionLinea { get; set; }
    }
}
