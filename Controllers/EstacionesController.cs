using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Estaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Administración de estaciones (cajas / puntos de venta). Migra EstacionesController +
    /// EstacionesDAO del legado, corrigiendo los verbos HTTP. Requiere JWT válido; el alta toma
    /// el idUsuario del token. El listado es paginado server-side (SP_V2_CONSULTA_ESTACIONES,
    /// OFFSET/FETCH + búsqueda) y devuelve Notificacion con data/links/meta.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/estaciones")]
    public class EstacionesController : ControllerBase
    {
        // Estatus de baja lógica heredado del legado (EvtEstaciones.js: EliminarEstacion -> idStatus 2).
        private const int EstatusActivo = 1;
        private const int EstatusInactivo = 2;

        private readonly IEstacionesService _estacionesService;
        private readonly IPaginationBuilder _pagination;

        public EstacionesController(IEstacionesService estacionesService, IPaginationBuilder pagination)
        {
            _estacionesService = estacionesService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado de estaciones. Query: page, perPage, q, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<Estacion>>> Listar([FromQuery] PagedQuery query)
        {
            var page = await _estacionesService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Obtiene una estación por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<Estacion>> ObtenerPorId(int id)
            => _estacionesService.ObtenerPorIdAsync(id);

        /// <summary>Alta de estación. El idUsuario se toma del JWT.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarEstacionRequest request)
        {
            request.IdEstacion = 0;
            return _estacionesService.GuardarAsync(request, IdUsuario);
        }

        /// <summary>Edición de estación.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarEstacionRequest request)
        {
            request.IdEstacion = id;
            return _estacionesService.GuardarAsync(request, IdUsuario);
        }

        /// <summary>Activa/desactiva una estación (borrado lógico; idStatus 1=activo, 2=inactivo).</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> CambiarEstatus(int id, [FromBody] CambiarEstatusRequest request)
            => _estacionesService.CambiarEstatusAsync(id, request.Activo ? EstatusActivo : EstatusInactivo);

        /// <summary>Catálogo de sucursales.</summary>
        [HttpGet("catalogos/sucursales")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursales()
            => _estacionesService.ObtenerSucursalesAsync();

        /// <summary>
        /// Catálogo de almacenes, filtrado por sucursal y tipo. Para estaciones el legado usa
        /// idTipoAlmacen = 3 (EvtEstaciones.js).
        /// </summary>
        [HttpGet("catalogos/almacenes")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenes(
            [FromQuery] int? idSucursal = null,
            [FromQuery] int? idTipoAlmacen = 3)
            => _estacionesService.ObtenerAlmacenesAsync(idSucursal, idTipoAlmacen);

        /// <summary>Usuario autenticado (claim "idUsuario"; 0 si ausente o inválido).</summary>
        private int IdUsuario
            => int.TryParse(User.FindFirst("idUsuario")?.Value, out var valor) ? valor : 0;
    }
}
