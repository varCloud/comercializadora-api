using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Filtros del listado de "Reportes &gt; Ventas" (<c>ReportesController.BuscarVentas</c> del
    /// legado). Corrección post-aprobación (2026-07-19): pasa a heredar <see cref="PagedQuery"/>
    /// (agrega <c>Page</c>/<c>PerPage</c>/<c>Order</c>/<c>Sort</c>) porque
    /// <c>SP_CONSULTA_VENTAS</c> no se modifica (no puede paginar a nivel SQL) y un rango de
    /// fechas amplio puede devolver miles de filas en una sola respuesta; se pagina en memoria,
    /// mismo patrón que <see cref="InventarioReporteQuery"/> /
    /// <c>LimitesInventarioRepository.ListarAsync</c>. <c>Order</c>/<c>Sort</c> no tienen
    /// whitelist propia: no se pide ordenar explícitamente y el resultset conserva el orden
    /// natural que ya devuelve el SP (por fecha/folio de venta, igual que el legado). Se enlaza
    /// con <c>[FromQuery]</c>.
    /// </summary>
    public class VentaReporteQuery : PagedQuery
    {
        public int? IdLineaProducto { get; set; }
        public int? IdCliente { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
