namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila de exportación completa de "Reportes &gt; Inventario" (<c>SP_V2_CONSULTA_INVENTARIO</c>
    /// en modo <c>@exportar = 1</c>). Resultset "unión" que replica
    /// <c>SP_CONSULTA_INVENTARIO_GENERAL_UBICACION</c>: las columnas de ubicación (Almacén/
    /// Pasillo/Raq/Piso) llegan <c>null</c> cuando <c>@tipo = 1</c> (General); se completan
    /// cuando <c>@tipo = 2</c> (Ubicación). El controller decide qué columnas exporta según el
    /// <c>tipo</c> solicitado (ver <c>ColumnaExportable&lt;T&gt;</c> en el controller).
    /// </summary>
    public class InventarioReporteExportItem
    {
        public int IdProducto { get; set; }
        public string? Descripcion { get; set; }
        public decimal UltimoCostoCompra { get; set; }
        public decimal PrecioIndividual { get; set; }
        public decimal PrecioMenudeo { get; set; }
        public double Cantidad { get; set; }
        public int? IdPasillo { get; set; }
        public int? IdRaq { get; set; }
        public int? IdPiso { get; set; }
        public string? Almacen { get; set; }
        public string? Pasillo { get; set; }
        public string? Raq { get; set; }
        public string? Piso { get; set; }
    }
}
