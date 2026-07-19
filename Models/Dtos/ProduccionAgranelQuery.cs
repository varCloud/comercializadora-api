using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del listado "Producción a granel": paginación/orden (heredados de
    /// <see cref="PagedQuery"/>) + filtros propios (usuario, estatus del proceso, almacén, rango
    /// de fechas). Sin `q` de texto libre — la pantalla legada solo tenía filtros estructurados.
    /// Equivale a <c>FiltroCostoProduccionAgranel</c> del legado; <c>IdAlmacen</c> cubre el caso
    /// del WS móvil <c>obtenerProductosProduccionAgranel</c> (listado por almacén).
    /// </summary>
    public class ProduccionAgranelQuery : PagedQuery
    {
        public int? IdUsuario { get; set; }
        public int? IdEstatus { get; set; }
        public int? IdAlmacen { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
