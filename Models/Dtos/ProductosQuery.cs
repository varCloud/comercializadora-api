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

        /// <summary>
        /// Filtro opcional por varias líneas de producto (CSV, p. ej. "12,20"). Cuando viene
        /// informado, el SP filtra <c>idLineaProducto IN (csv)</c>. Lo usa Relación Liquidos para
        /// los selectores de producto a granel. null/"" = sin filtro multi-línea.
        /// </summary>
        public string? IdLineasProducto { get; set; }
    }
}
