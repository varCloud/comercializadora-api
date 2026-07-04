using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del reporte "Producción Trapeadores": paginación/orden (heredados
    /// de <see cref="PagedQuery"/>) + filtros propios (rol, usuario, rango de fechas). Sin `q` de
    /// texto libre — el reporte no tiene buscador genérico, solo filtros estructurados. Se
    /// enlaza con [FromQuery]. El filtro <c>idTipoMovInventario = 32</c> es fijo en el
    /// repository, no se expone aquí ni en el front.
    /// </summary>
    public class ProduccionTrapeadoresQuery : PagedQuery
    {
        public int? IdRol { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
