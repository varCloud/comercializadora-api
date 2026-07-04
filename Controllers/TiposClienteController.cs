using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.TiposCliente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Mantenimiento del catálogo de Tipos de cliente con su % de descuento (la pantalla
    /// "Descuentos" del legado, renombrada). Migra la parte de tipos de ClientesController +
    /// ClienteDAO con verbos HTTP correctos y listado paginado (data/links/meta). Módulo
    /// dedicado, precedente de Líneas de producto.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/tipos-cliente")]
    public class TiposClienteController : ControllerBase
    {
        private readonly ITiposClienteService _tiposClienteService;
        private readonly IPaginationBuilder _pagination;

        public TiposClienteController(ITiposClienteService tiposClienteService, IPaginationBuilder pagination)
        {
            _tiposClienteService = tiposClienteService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Query: page, perPage, q, order (descripcion|descuento), sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<TipoCliente>>> Listar([FromQuery] PagedQuery query)
        {
            var page = await _tiposClienteService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Obtiene un tipo de cliente por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<TipoCliente>> ObtenerPorId(int id)
            => _tiposClienteService.ObtenerPorIdAsync(id);

        /// <summary>Alta de tipo de cliente.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarTipoClienteRequest request)
        {
            request.IdTipoCliente = 0;
            return _tiposClienteService.GuardarAsync(request);
        }

        /// <summary>Edición de tipo de cliente.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarTipoClienteRequest request)
        {
            request.IdTipoCliente = id;
            return _tiposClienteService.GuardarAsync(request);
        }

        /// <summary>Activa/desactiva un tipo de cliente (baja lógica).</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> CambiarEstatus(int id, [FromBody] CambiarEstatusRequest request)
            => _tiposClienteService.CambiarEstatusAsync(id, request.Activo);
    }
}
