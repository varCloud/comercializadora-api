using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Bitacoras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte "Bitácoras": consulta de solo lectura de pedidos internos (traspasos de producto
    /// entre almacenes) con su línea de tiempo de cambios de estatus. Migra
    /// <c>BitacoraController</c> del legado (acciones <c>Bitacoras</c>, <c>_ObtenerBitacoras</c>,
    /// <c>_DetalleBitacora</c>). Sin alta/edición/baja. Los catálogos de almacenes/usuarios/
    /// productos para los filtros se reutilizan de los módulos ya migrados (no se duplican aquí).
    /// La exportación (PDF/Excel del legado) queda diferida.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/bitacoras")]
    public class BitacorasController : ControllerBase
    {
        private const int RolAdmin = 1;

        private readonly IBitacorasService _bitacorasService;
        private readonly IPaginationBuilder _pagination;

        public BitacorasController(IBitacorasService bitacorasService, IPaginationBuilder pagination)
        {
            _bitacorasService = bitacorasService;
            _pagination = pagination;
        }

        /// <summary>
        /// Listado paginado. Query: page, perPage, idPedidoInterno, idEstatusPedidoInterno,
        /// idAlmacenOrigen, idAlmacenDestino, idUsuario, idProducto, fechaIni, fechaFin, order, sort.
        /// Visibilidad por rol: un usuario no-admin que no elige un usuario concreto solo ve sus
        /// propios pedidos (idUsuario del JWT), igual que el gate del legado.
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<Bitacora>>> Listar([FromQuery] BitacorasQuery query)
        {
            if (IdRol != RolAdmin && (query.IdUsuario is null || query.IdUsuario == 0))
                query.IdUsuario = IdUsuario;

            var page = await _bitacorasService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Línea de tiempo (historial de estatus) de un folio de pedido interno.</summary>
        [HttpGet("{idPedidoInterno:int}/detalle")]
        public Task<Notificacion<IEnumerable<BitacoraDetalle>>> ObtenerDetalle(int idPedidoInterno)
            => _bitacorasService.ObtenerDetalleAsync(idPedidoInterno);

        /// <summary>Catálogo de estatus de pedidos internos (la UI antepone "TODOS").</summary>
        [HttpGet("catalogos/estatus")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatus()
            => _bitacorasService.ObtenerEstatusAsync();

        /// <summary>Usuario autenticado (claim "idUsuario"; 0 si ausente o inválido).</summary>
        private int IdUsuario
            => int.TryParse(User.FindFirst("idUsuario")?.Value, out var valor) ? valor : 0;

        /// <summary>Rol del usuario autenticado (claim "idRol"; 0 si ausente o inválido).</summary>
        private int IdRol
            => int.TryParse(User.FindFirst("idRol")?.Value, out var valor) ? valor : 0;
    }
}
