using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros del listado de Límites de Inventario: paginación/búsqueda/orden (heredados de
    /// <see cref="PagedQuery"/>) + filtros propios. Se enlaza con [FromQuery].
    /// </summary>
    public class LimitesInventarioQuery : PagedQuery
    {
        /// <summary>Filtro por almacén (0 = todos).</summary>
        public int IdAlmacen { get; set; }

        /// <summary>Filtro por línea de producto (0 = todas).</summary>
        public int IdLineaProducto { get; set; }

        /// <summary>Filtro por estatus de límite (1 dentro / 2 sobre máx / 3 bajo mín; 0 = todos).</summary>
        public int IdEstatusLimiteInv { get; set; }
    }
}
