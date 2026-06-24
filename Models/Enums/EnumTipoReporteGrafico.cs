namespace comercializadora_api.Models.Enums
{
    /// <summary>
    /// Periodo del reporte de los gráficos del dashboard. Migra EnumTipoReporteGrafico del
    /// legado (se renombran los valores plurales legados a singular para mayor claridad,
    /// conservando los mismos valores numéricos que esperan los SP).
    /// </summary>
    public enum EnumTipoReporteGrafico
    {
        Semanal = 1,
        Mensual = 2,
        Anual = 3,
        Dia = 4
    }
}
