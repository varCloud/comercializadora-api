using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del listado "Inventario Físico": paginación/orden (heredados de
    /// <see cref="PagedQuery"/>; whitelist de orden fecha|nombre|estatus en el SP) + filtros
    /// propios (tipo de inventario y rango de fechas de alta). Sin `q` de texto libre — la
    /// pantalla legada solo tenía filtros estructurados. El idSucursal NO viaja en la query:
    /// se toma del claim JWT (paridad con la sesión legada).
    /// </summary>
    public class InventarioFisicoQuery : PagedQuery
    {
        /// <summary>1 = General, 2 = Individual; null/0 = todos.</summary>
        public int? IdTipoInventario { get; set; }

        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
