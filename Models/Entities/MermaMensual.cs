namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Resultset de SP_DASHBOARD_MERMA: merma mensual (mes actual y mes de cálculo anterior).
    /// Migra el modelo MermaMensual del legado. Las columnas del SP mapean directo en Dapper.
    /// </summary>
    public class MermaMensual
    {
        public string? Descripcion { get; set; }
        public DateTime UltimoDiaMesActual { get; set; }
        public DateTime UltimoDiaMesCalculo { get; set; }
        public float TotalMerma { get; set; }
        public float TotalPorcMerma { get; set; }
        public float TotalCostoMerma { get; set; }
        public float PromedioMerma { get; set; }
        public float PromedioPorcMerma { get; set; }
        public float PromedioCostoMerma { get; set; }
    }
}
