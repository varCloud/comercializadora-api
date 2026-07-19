namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Renglón del reporte "Costo de Producción Agranel" (menú legado "Consumo de MPL"). Mapea
    /// el resultset de datos de <c>SP_V2_CONSULTA_COSTO_PRODUCCION</c>. Solo trae los campos que
    /// consume la vista legada <c>_ObtenerCostoProduccion.cshtml</c>; el modelo legado
    /// <c>CostoProduccionAgranel.cs</c> traía además <c>idReporteCostoProduccion</c>,
    /// <c>porcCostoProduccion</c>, <c>ultimoDiaMesCalculo</c>/<c>ultimoDiaMesAnterior</c>,
    /// <c>fechaAlta</c>, los ecos de filtro (<c>AnioCalculo</c>/<c>MesCalculo</c>/<c>idAlmacen</c>)
    /// y campos duplicados de otro reporte (<c>nombreUsuario</c>, <c>descripcionEstatus</c>,
    /// <c>cantidad</c>/<c>cantidadAceptada</c>/<c>cantidadRestante</c>) que esta pantalla no usa
    /// y que el SP_V2 tampoco proyecta. "Cantidad Restante" se calcula en el front
    /// (cantidadSolicitadaMesAnt − cantidadAceptadaFinalMesAnt), no viaja en el resultset.
    /// </summary>
    public class CostoProduccionAgranel
    {
        public int IdProducto { get; set; }
        public string? CodigoBarras { get; set; }
        public string? DescripcionProducto { get; set; }
        public int IdLineaProducto { get; set; }
        public string? DescripcionLinea { get; set; }
        public decimal CantidadSolicitadaMesAnt { get; set; }
        public decimal CantidadAceptadaFinalMesAnt { get; set; }
        public decimal UltCostoCompra { get; set; }
        public decimal CostoProduccionMerma { get; set; }
    }
}
