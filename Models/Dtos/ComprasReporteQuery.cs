using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Filtros del listado de "Reportes &gt; Compras". Hereda paginación/orden de
    /// <see cref="PagedQuery"/>. Se enlaza con <c>[FromQuery]</c>.
    /// </summary>
    /// <remarks>
    /// Nombrado <c>ComprasReporteQuery</c> (no <c>ComprasQuery</c>, como sugería la HU original)
    /// porque <see cref="Models.Dtos.ComprasQuery"/> ya existe: es el DTO del módulo CRUD de
    /// Compras (<c>Repositories/Compras/ComprasRepository.cs</c>), con un set de filtros distinto
    /// (sin <see cref="IdLineaProducto"/>, con <c>FechaInicio</c> en vez de <see cref="FechaIni"/>
    /// para coincidir con el contrato legado de esa pantalla). Reusar el nombre habría
    /// sobrescrito ese archivo. Ver nota en <c>.claude/memory/</c> de este repo.
    /// </remarks>
    public class ComprasReporteQuery : PagedQuery
    {
        public int? IdProveedor { get; set; }
        public int? IdLineaProducto { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdStatusCompra { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
