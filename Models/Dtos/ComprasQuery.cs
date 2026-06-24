using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del listado de Compras: paginación/búsqueda/orden (heredados de
    /// <see cref="PagedQuery"/>) + filtros propios (proveedor, estatus, usuario, rango de fechas).
    /// Se enlaza con [FromQuery]. Equivale a los filtros del formulario de búsqueda del legado.
    /// </summary>
    public class ComprasQuery : PagedQuery
    {
        public int? IdProveedor { get; set; }
        public int? IdStatusCompra { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
