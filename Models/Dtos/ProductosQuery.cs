using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros del listado de Productos: paginación/búsqueda/orden (heredados de
    /// <see cref="PagedQuery"/>) + filtro propio por línea de producto. Se enlaza con [FromQuery].
    /// </summary>
    public class ProductosQuery : PagedQuery
    {
        /// <summary>Filtro por línea de producto (0 = todas).</summary>
        public int IdLineaProducto { get; set; }
    }
}
