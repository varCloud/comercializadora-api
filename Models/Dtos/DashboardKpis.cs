using comercializadora_api.Models.Entities;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Tarjetas/indicadores (KPIs) del dashboard que el legado componía en su acción index().
    /// Agrega totales de venta por periodo, información global del día y los comparativos de
    /// merma y costo de producción (mes actual vs. mes anterior).
    /// </summary>
    public class DashboardKpis
    {
        public float VentasDia { get; set; }
        public float VentasSemana { get; set; }
        public float VentasMes { get; set; }
        public float VentasAnio { get; set; }

        /// <summary>Información global del día (categorías con Id != 1).</summary>
        public List<Categoria> InformacionGlobal { get; set; } = new();

        public MermaMensual? MermaActual { get; set; }
        public MermaMensual? MermaAnterior { get; set; }
        public CostoProduccionMensual? CostoProduccionActual { get; set; }
        public CostoProduccionMensual? CostoProduccionAnterior { get; set; }
    }
}
