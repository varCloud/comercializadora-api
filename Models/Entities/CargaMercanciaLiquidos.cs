namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del reporte "Producción Líquidos" (carga de mercancía de productos líquidos).
    /// Mapea el resultset de datos de <c>SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS</c>. Migra
    /// <c>FiltroLiquidos</c>/<c>BuscarCargaMercanciaLiquidos</c> del legado. Solo lectura.
    /// </summary>
    public class CargaMercanciaLiquidos
    {
        public int IdProducto { get; set; }
        public string? DescripcionUbicacion { get; set; }
        public string? DescripcionProducto { get; set; }
        public decimal Cantidad { get; set; }
        public string? NombreUsuario { get; set; }
        public DateTime FechaAlta { get; set; }
        public string? DescripcionRol { get; set; }
        public decimal UltimoCostoCompra { get; set; }
        public string? DescTipoMovInventario { get; set; }
    }
}
