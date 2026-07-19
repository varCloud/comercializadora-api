using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Filtros del listado paginado de "Reportes &gt; Inventario": paginación/búsqueda/orden
    /// (heredados de <see cref="PagedQuery"/>, <c>Q</c> busca sobre descripción/artículo) +
    /// filtros propios de pantalla (línea de producto, almacén, rango de fechas). Se enlaza con
    /// <c>[FromQuery]</c>.
    /// </summary>
    public class InventarioReporteQuery : PagedQuery
    {
        public int? IdLineaProducto { get; set; }
        public int? IdAlmacen { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
