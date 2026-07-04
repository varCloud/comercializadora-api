namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del reporte "Producción Trapeadores" (carga de mercancía de trapeadores).
    /// Mapea el resultset de datos de <c>SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS</c> filtrado
    /// por <c>idTipoMovInventario = 32</c> (fijo en el repository). Entidad de dominio separada
    /// de <see cref="CargaMercanciaLiquidos"/> aunque comparte el mismo shape/columnas. Solo lectura.
    /// </summary>
    public class CargaMercanciaTrapeadores
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
