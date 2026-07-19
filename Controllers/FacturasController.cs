using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Facturas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Pantalla "Facturas Ventas". Migra <c>FacturaController</c> (consulta/detalle/reenvío/
    /// cancelación) y <c>WsFacturaController.ActualizaEstatusCancelacionFactura</c> del legado.
    /// La generación/timbrado de CFDI (<c>GenerarFactura</c>/<c>RegenerarFactura</c>) queda fuera
    /// de alcance de esta feature. Requiere JWT válido.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/facturas")]
    public class FacturasController : ControllerBase
    {
        private readonly IFacturacionService _facturacionService;
        private readonly IPaginationBuilder _pagination;

        public FacturasController(IFacturacionService facturacionService, IPaginationBuilder pagination)
        {
            _facturacionService = facturacionService;
            _pagination = pagination;
        }

        /// <summary>
        /// Listado paginado de facturas de venta. Query: page, perPage, q, order, sort,
        /// idStatusFactura, idUsuario, fechaInicio, fechaFin.
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<FacturaVenta>>> Listar([FromQuery] FacturasQuery query)
        {
            var pagina = await _facturacionService.ListarAsync(query);
            return _pagination.Build(pagina, query, Request);
        }

        /// <summary>Detalle de la venta para el modal de reenvío (cliente, forma de pago, uso CFDI, totales).</summary>
        [HttpGet("detalle/{idVenta:long}")]
        public Task<Notificacion<DetalleVentaFactura>> ObtenerDetalle(long idVenta)
            => _facturacionService.ObtenerDetalleVentaAsync(idVenta);

        /// <summary>Reenvía la factura (PDF + XML) por correo al cliente, con copia opcional.</summary>
        [HttpPost("reenviar")]
        public Task<Notificacion<string>> Reenviar([FromBody] ReenviarFacturaRequest request)
            => _facturacionService.ReenviarAsync(request);

        /// <summary>Cancela la factura ante el PAC (CFDI) y registra el estatus resultante.</summary>
        [HttpPost("cancelar")]
        public Task<Notificacion<string>> Cancelar([FromBody] CancelarFacturaRequest request)
            => _facturacionService.CancelarAsync(request, IdUsuario);

        /// <summary>
        /// Consulta el estatus de cancelación ante el SAT y actualiza la factura si ya fue
        /// cancelada. Compartido por ventas (<c>esPedidoEspecial=false</c>, <c>id</c>=idVenta) y
        /// pedidos especiales (<c>esPedidoEspecial=true</c>, <c>id</c>=idPedidoEspecial).
        /// </summary>
        [HttpPost("estatus-cancelacion")]
        public Task<Notificacion<AcuseEstatusCfdi>> ConsultarEstatusCancelacion([FromBody] EstatusCancelacionRequest request)
            => _facturacionService.ConsultarEstatusCancelacionAsync(request, IdUsuario);

        // ── Pedidos Especiales (feature migracion_facturas_pedidos_esp) ──
        // Rutas PE propias (no flag sobre los endpoints de ventas): cada request tipa su
        // identificador (idPedidoEspecial) sin ambigüedad y el front tiene pantalla independiente.
        // La excepción deliberada es estatus-cancelacion, que ya nació compartido con flag.

        /// <summary>
        /// Listado paginado de facturas de pedidos especiales. Query: page, perPage, q, order,
        /// sort, idStatusFactura, idUsuario, fechaInicio, fechaFin (mismos filtros que ventas).
        /// </summary>
        [HttpGet("pedidos-especiales")]
        public async Task<Notificacion<IEnumerable<FacturaPedidoEspecial>>> ListarPedidosEspeciales([FromQuery] FacturasQuery query)
        {
            var pagina = await _facturacionService.ListarPedidosEspecialesAsync(query);
            return _pagination.Build(pagina, query, Request);
        }

        /// <summary>Detalle del pedido especial para el modal de reenvío.</summary>
        [HttpGet("pedidos-especiales/detalle/{idPedidoEspecial:long}")]
        public Task<Notificacion<DetalleVentaFactura>> ObtenerDetallePedidoEspecial(long idPedidoEspecial)
            => _facturacionService.ObtenerDetallePedidoEspecialAsync(idPedidoEspecial);

        /// <summary>Reenvía la factura del pedido especial (PDF + XML) por correo, con copia opcional.</summary>
        [HttpPost("pedidos-especiales/reenviar")]
        public Task<Notificacion<string>> ReenviarPedidoEspecial([FromBody] ReenviarFacturaPeRequest request)
            => _facturacionService.ReenviarPedidoEspecialAsync(request);

        /// <summary>Cancela la factura del pedido especial ante el PAC y registra el estatus resultante.</summary>
        [HttpPost("pedidos-especiales/cancelar")]
        public Task<Notificacion<string>> CancelarPedidoEspecial([FromBody] CancelarFacturaPeRequest request)
            => _facturacionService.CancelarPedidoEspecialAsync(request, IdUsuario);

        /// <summary>Usuario autenticado (claim "idUsuario"; 0 si ausente o inválido).</summary>
        private int IdUsuario
            => int.TryParse(User.FindFirst("idUsuario")?.Value, out var valor) ? valor : 0;
    }
}
