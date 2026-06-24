namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Resultset de SP_DASHBOARD_COSTO_PRODUCCION: costo de producción a granel mensual
    /// (mes actual y mes de cálculo anterior). Migra CostoProduccionAgranelMensual del legado.
    /// Las columnas del SP mapean directo en Dapper.
    /// </summary>
    public class CostoProduccionMensual
    {
        public string? Descripcion { get; set; }
        public DateTime UltimoDiaMesActual { get; set; }
        public DateTime UltimoDiaMesCalculo { get; set; }
        public float TotalCantidadAceptada { get; set; }
        public float TotalPorcCostoProduccion { get; set; }
        public float TotalCostoProduccion { get; set; }
        public float PromedioCantidadAceptada { get; set; }
        public float PromedioPorcCostoProduccion { get; set; }
        public float PromedioCostoProduccion { get; set; }
    }
}
