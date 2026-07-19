using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del reporte "Bitácoras" (pedidos internos): paginación/orden
    /// (heredados de <see cref="PagedQuery"/>) + filtros propios (folio, estatus, almacén
    /// origen/destino, usuario, producto, rango de fechas). Sin `q` de texto libre — la pantalla
    /// legada solo tenía filtros estructurados. Equivale al uso de <c>PedidosInternos</c> como
    /// filtro en <c>BitacoraController</c>. El tipo de pedido (siempre 1 en el reporte web) NO se
    /// expone aquí: lo fija el repositorio.
    /// </summary>
    public class BitacorasQuery : PagedQuery
    {
        public int? IdPedidoInterno { get; set; }
        public int? IdEstatusPedidoInterno { get; set; }
        public int? IdAlmacenOrigen { get; set; }
        public int? IdAlmacenDestino { get; set; }
        public int? IdUsuario { get; set; }
        public int? IdProducto { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
