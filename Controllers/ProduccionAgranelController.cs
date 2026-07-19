using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.ProduccionAgranel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Módulo "Producción a granel" (conversión MPL → líquido a granel y su envasado). Migra
    /// <c>ProduccionAgranelController</c> (pantalla web de consulta) y los webservices
    /// <c>AdminProduccionAgranelController</c> / <c>AdminLiquidosController.agregarLiquidosAInventario</c>
    /// del legado. El idUsuario de los comandos se toma del claim "idUsuario" del JWT (no del
    /// body). Los catálogos de usuarios/almacenes/productos para los filtros se reusan de los
    /// módulos Usuarios/Productos ya migrados (no se duplican aquí).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/produccion-agranel")]
    public class ProduccionAgranelController : ControllerBase
    {
        private readonly IProduccionAgranelService _produccionAgranelService;
        private readonly IPaginationBuilder _pagination;

        public ProduccionAgranelController(
            IProduccionAgranelService produccionAgranelService, IPaginationBuilder pagination)
        {
            _produccionAgranelService = produccionAgranelService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Query: page, perPage, idUsuario, idEstatus, idAlmacen, fechaIni, fechaFin, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<ProcesoProduccionAgranel>>> Listar(
            [FromQuery] ProduccionAgranelQuery query)
        {
            var page = await _produccionAgranelService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Catálogo de estatus del proceso de producción (la UI antepone "TODOS").</summary>
        [HttpGet("catalogos/estatus")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatus()
            => _produccionAgranelService.ObtenerEstatusAsync();

        /// <summary>Alta de producto MPL a producción a granel. El idUsuario se toma del JWT.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Agregar([FromBody] AgregarProduccionAgranelRequest request)
            => _produccionAgranelService.AgregarAsync(request, UsuarioActual());

        /// <summary>Aprobación/rechazo de renglones pendientes. El idUsuario se toma del JWT.</summary>
        [HttpPatch("aprobar")]
        public Task<Notificacion<string>> Aprobar([FromBody] AprobarProduccionAgranelRequest request)
            => _produccionAgranelService.AprobarAsync(request, UsuarioActual());

        /// <summary>Registro de envasado de líquidos (granel → envasado). El idUsuario se toma del JWT.</summary>
        [HttpPost("envasado")]
        public Task<Notificacion<string>> AgregarEnvasado([FromBody] AgregarEnvasadoLiquidosRequest request)
            => _produccionAgranelService.AgregarEnvasadoAsync(request, UsuarioActual());

        /// <summary>Usuario autenticado (claim "idUsuario"; 0 si ausente o inválido).</summary>
        private int UsuarioActual()
            => int.TryParse(User.FindFirst("idUsuario")?.Value, out var valor) ? valor : 0;
    }
}
