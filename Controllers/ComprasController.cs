using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Compras;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Administración de Compras a proveedores. Migra ComprasController + ComprasDAO del legado
    /// con verbos HTTP correctos y listado paginado (data/links/meta). El alta/edición ejecuta
    /// SP_REGISTRA_COMPRA tomando el idUsuario del JWT (no del body). Requiere JWT válido
    /// ([Authorize], sin restricción por rol).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/compras")]
    public class ComprasController : ControllerBase
    {
        // Sucursal de operación (Uruapan). El catálogo de almacenes del modal se filtra por ella
        // (regla 15 del front: se opera una sola sucursal).
        private const int SucursalUruapan = 1;

        private readonly IComprasService _comprasService;
        private readonly IPaginationBuilder _pagination;

        public ComprasController(IComprasService comprasService, IPaginationBuilder pagination)
        {
            _comprasService = comprasService;
            _pagination = pagination;
        }

        /// <summary>
        /// Listado paginado de compras. Query: page, perPage, q, order, sort, idProveedor,
        /// idStatusCompra, idUsuario, fechaInicio, fechaFin.
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<Compra>>> Listar([FromQuery] ComprasQuery query)
        {
            var page = await _comprasService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Catálogo de estatus de compra (para los filtros y el modal).</summary>
        [HttpGet("estatus")]
        public Task<Notificacion<IEnumerable<EstatusCompra>>> ObtenerEstatus()
            => _comprasService.ObtenerEstatusAsync();

        /// <summary>Catálogo de almacenes de la sucursal de operación (Uruapan).</summary>
        [HttpGet("catalogos/almacenes")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenes()
            => _comprasService.ObtenerAlmacenesAsync(SucursalUruapan);

        /// <summary>Compra por id: cabecera + detalle de productos (para precargar el modal).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<Compra>> ObtenerPorId(int id)
            => _comprasService.ObtenerPorIdAsync(id);

        /// <summary>Alta de compra. El idUsuario se toma del JWT.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarCompraRequest request)
        {
            request.IdCompra = 0;
            return _comprasService.GuardarAsync(request, IdUsuario);
        }

        /// <summary>Edición de compra. El idUsuario se toma del JWT.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarCompraRequest request)
        {
            request.IdCompra = id;
            return _comprasService.GuardarAsync(request, IdUsuario);
        }

        /// <summary>Elimina (baja lógica) una compra.</summary>
        [HttpDelete("{id:int}")]
        public Task<Notificacion<string>> Eliminar(int id)
            => _comprasService.EliminarAsync(id);

        /// <summary>Usuario autenticado (claim "idUsuario"; 0 si ausente o inválido).</summary>
        private int IdUsuario
            => int.TryParse(User.FindFirst("idUsuario")?.Value, out var valor) ? valor : 0;
    }
}
