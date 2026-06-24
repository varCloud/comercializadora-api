namespace comercializadora_api.Models.Enums
{
    /// <summary>
    /// Tipo de gráfico del dashboard. Migra EnumTipoGrafico del legado (se corrige el typo
    /// "Provedores" -> "Proveedores"), conservando los mismos valores numéricos que esperan
    /// los SP (en particular SP_DASHBOARD_CONSULTA_TOP_TEN vía @idTipoGrafico).
    /// </summary>
    public enum EnumTipoGrafico
    {
        VentasPorFecha = 1,
        TopTenProductos = 2,
        TopTenClientes = 3,
        TopTenProveedores = 4,
        InformacionGlobal = 5
    }
}
