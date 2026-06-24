namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Resultset de SP_DASHBOARD_CONSULTA_TOTAL_VENTAS_POR_ESTACION. Subconjunto útil de la
    /// entidad Estacion del legado, centrado en los montos de venta por periodo (día/semana/
    /// mes/año) que alimentan los KPIs del dashboard.
    /// </summary>
    public class EstacionVenta
    {
        public int IdEstacion { get; set; }
        public string? Nombre { get; set; }
        public int Numero { get; set; }
        public int IdAlmacen { get; set; }
        public string? NombreAlmacen { get; set; }
        public float MontoTotalDia { get; set; }
        public float MontoTotalSemana { get; set; }
        public float MontoTotalMes { get; set; }
        public float MontoTotalAnio { get; set; }
        public int IdSucursal { get; set; }
    }
}
