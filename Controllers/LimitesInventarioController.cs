using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.LimitesInventario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Módulo de Límites de Inventario. Migra LimiteInventarioDAO + las acciones LimitesInventario
    /// del ProductosController legado, con verbos HTTP correctos y listado paginado (data/links/meta).
    /// El idUsuario y el filtro de almacén por rol se resuelven con los claims del JWT.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/limites-inventario")]
    public class LimitesInventarioController : ControllerBase
    {
        private const int RolAdministrador = 1;

        private readonly ILimitesInventarioService _service;
        private readonly IPaginationBuilder _pagination;

        public LimitesInventarioController(ILimitesInventarioService service, IPaginationBuilder pagination)
        {
            _service = service;
            _pagination = pagination;
        }

        /// <summary>
        /// Listado paginado. Filtros por query: page, perPage, q, order, sort, idAlmacen,
        /// idLineaProducto, idEstatusLimiteInv. Si el usuario no es administrador (idRol != 1) y
        /// no eligió almacén, se fuerza su almacén del token (como el legado).
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<LimiteInventario>>> Listar([FromQuery] LimitesInventarioQuery query)
        {
            if (Claim("idRol") != RolAdministrador && query.IdAlmacen == 0)
                query.IdAlmacen = Claim("idAlmacen");

            var page = await _service.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Catálogo de estatus de límite (1 dentro / 2 sobre máx / 3 bajo mín).</summary>
        [HttpGet("estatus")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatus()
            => _service.ObtenerEstatusAsync();

        /// <summary>Catálogo de almacenes (filtro), opcionalmente por sucursal/tipo.</summary>
        [HttpGet("catalogos/almacenes")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenes(
            [FromQuery] int? idSucursal = null,
            [FromQuery] int? idTipoAlmacen = null)
            => _service.ObtenerAlmacenesAsync(idSucursal, idTipoAlmacen);

        /// <summary>Catálogo de líneas de producto (filtro).</summary>
        [HttpGet("catalogos/lineas")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineas()
            => _service.ObtenerLineasAsync();

        /// <summary>Crea/actualiza el límite mín/máx de un producto en un almacén. idUsuario del JWT.</summary>
        [HttpPatch]
        public Task<Notificacion<string>> Guardar([FromBody] GuardarLimiteRequest request)
            => _service.GuardarAsync(request, Claim("idUsuario"));

        /// <summary>Carga masiva de límites (filas del Excel parseadas en el front). idUsuario del JWT.</summary>
        [HttpPost("masivo")]
        public Task<Notificacion<string>> GuardarMasivo([FromBody] GuardarLimitesMasivoRequest request)
            => _service.GuardarMasivoAsync(request, Claim("idUsuario"));

        /// <summary>Lee un claim entero del JWT (0 si ausente o inválido).</summary>
        private int Claim(string tipo)
            => int.TryParse(User.FindFirst(tipo)?.Value, out var valor) ? valor : 0;
    }
}
