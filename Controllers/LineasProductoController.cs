using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.LineasProducto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Mantenimiento del catálogo de Líneas de producto (submenú de Productos). Migra
    /// LineaProductoController + LineaProductoDAO del legado con verbos HTTP correctos y
    /// listado paginado (data/links/meta). Requiere JWT válido ([Authorize], sin restricción
    /// por rol). El listado muestra solo activas; desactivar bloquea si hay productos asociados.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/lineas-producto")]
    public class LineasProductoController : ControllerBase
    {
        private readonly ILineasProductoService _service;
        private readonly IPaginationBuilder _pagination;

        public LineasProductoController(ILineasProductoService service, IPaginationBuilder pagination)
        {
            _service = service;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Query: page, perPage, q, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<LineaProducto>>> Listar([FromQuery] PagedQuery query)
        {
            var page = await _service.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Obtiene una línea por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<LineaProducto>> ObtenerPorId(int id)
            => _service.ObtenerPorIdAsync(id);

        /// <summary>Alta de línea de producto.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarLineaProductoRequest request)
        {
            request.IdLineaProducto = 0;
            return _service.GuardarAsync(request);
        }

        /// <summary>Edición de línea de producto.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarLineaProductoRequest request)
        {
            request.IdLineaProducto = id;
            return _service.GuardarAsync(request);
        }

        /// <summary>Activa/desactiva una línea (baja lógica; bloquea baja con productos asociados).</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> CambiarEstatus(int id, [FromBody] CambiarEstatusRequest request)
            => _service.CambiarEstatusAsync(id, request.Activo);
    }
}
