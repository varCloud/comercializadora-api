using comercializadora_api.Models.Entities;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Respuesta del gráfico de ventas por fecha. Cada categoría trae fechaIni/fechaFin para
    /// que el front pueda hacer el drilldown por estación. Las series (Ventas = total,
    /// Ventas PE = totalPE) las arma el front a partir de estas categorías.
    /// </summary>
    public class VentasPorFecha
    {
        public List<Categoria> Categorias { get; set; } = new();
    }
}
