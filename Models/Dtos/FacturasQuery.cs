using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del listado de Facturas Ventas: paginación/búsqueda/orden
    /// (heredados de <see cref="PagedQuery"/>) + filtros propios (estatus, usuario, rango de
    /// fechas). Se enlaza con [FromQuery]. Equivale a los filtros de la pantalla legada
    /// <c>Facturas.cshtml</c>.
    /// </summary>
    public class FacturasQuery : PagedQuery
    {
        public int? IdStatusFactura { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
