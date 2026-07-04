using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.RelacionTrapeadores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Módulo "Relación Trapeadores" (menú legado → Productos → Relación Trapeadores). Migra
    /// ProduccionProductosController + ProduccionProductosDAO con verbos HTTP correctos y
    /// listado paginado (data/links/meta). Gestiona las combinaciones materia prima/Matra +
    /// bastón → trapeador producido, con su unidad de medida y cantidad.
    /// <para>
    /// A diferencia de Relación Líquidos, NO expone un endpoint de productos por "tipo": los 3
    /// selectores del front consumen directamente <c>GET /api/productos</c> (decisión de diseño).
    /// </para>
    /// <para>
    /// A diferencia del legado (donde el botón "Editar" estaba deshabilitado), este módulo SÍ
    /// soporta edición (PUT), igual que Relación Líquidos (decisión de negocio).
    /// </para>
    /// Requiere JWT válido ([Authorize], sin restricción por rol).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/relacion-trapeadores")]
    public class RelacionTrapeadoresController : ControllerBase
    {
        private readonly IRelacionTrapeadoresService _service;
        private readonly IPaginationBuilder _pagination;

        public RelacionTrapeadoresController(IRelacionTrapeadoresService service, IPaginationBuilder pagination)
        {
            _service = service;
            _pagination = pagination;
        }

        /// <summary>Listado paginado de combinaciones. Query: page, perPage, q, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<RelacionTrapeador>>> Listar([FromQuery] PagedQuery query)
        {
            var page = await _service.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Catálogo de unidades de medida para trapeadores.</summary>
        [HttpGet("unidades-medida")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesMedida()
            => _service.ListarUnidadesMedidaAsync();

        /// <summary>Obtiene una relación por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<RelacionTrapeador>> ObtenerPorId(int id)
            => _service.ObtenerPorIdAsync(id);

        /// <summary>Alta de una relación.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarRelacionTrapeadorRequest request)
        {
            request.Id = 0;
            return _service.GuardarAsync(request);
        }

        /// <summary>Edición de una relación.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarRelacionTrapeadorRequest request)
        {
            request.Id = id;
            return _service.GuardarAsync(request);
        }

        /// <summary>Desactiva una relación (baja lógica), por su id propio.</summary>
        [HttpDelete("{id:int}")]
        public Task<Notificacion<string>> Desactivar(int id)
            => _service.DesactivarAsync(id);
    }
}
