using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del reporte "Producción Líquidos": paginación/orden (heredados de
    /// <see cref="PagedQuery"/>) + filtros propios (rol, usuario, rango de fechas). Sin `q` de
    /// texto libre — el reporte no tiene buscador genérico, solo filtros estructurados. Se
    /// enlaza con [FromQuery]. Equivale a <c>FiltroLiquidos</c> del legado
    /// (sin <c>idTipoMovimiento</c>: el legado nunca lo expone en el form).
    /// </summary>
    public class ProduccionLiquidosQuery : PagedQuery
    {
        public int? IdRol { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
