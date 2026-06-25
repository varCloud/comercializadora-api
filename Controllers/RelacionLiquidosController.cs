using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.RelacionLiquidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Módulo "Relación Liquidos" (menú legado → Productos → Relación Liquidos). Migra
    /// ProductosAgranelAEnvasarController + ProductosAgranelAEnvasarDAO con verbos HTTP correctos y
    /// listado paginado (data/links/meta). Gestiona las combinaciones materia prima a granel →
    /// producto envasado → envase, con su unidad de medida y cantidad. Los 3 selectores de producto
    /// se sirven por "tipo" (granel | envasar | envase), reutilizando el catálogo de Productos
    /// filtrado por línea. Requiere JWT válido ([Authorize], sin restricción por rol).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/relacion-liquidos")]
    public class RelacionLiquidosController : ControllerBase
    {
        private readonly IRelacionLiquidosService _service;
        private readonly IPaginationBuilder _pagination;

        public RelacionLiquidosController(IRelacionLiquidosService service, IPaginationBuilder pagination)
        {
            _service = service;
            _pagination = pagination;
        }

        /// <summary>Listado paginado de combinaciones. Query: page, perPage, q, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<RelacionLiquido>>> Listar([FromQuery] PagedQuery query)
        {
            var page = await _service.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>
        /// Catálogo paginado de productos para un selector, según <c>tipo</c>
        /// (granel | envasar | envase). Query: tipo, page, perPage, q, order, sort.
        /// </summary>
        [HttpGet("productos")]
        public async Task<Notificacion<IEnumerable<Producto>>> ListarProductos(
            [FromQuery] string tipo, [FromQuery] PagedQuery query)
        {
            var page = await _service.ListarProductosAsync(tipo, query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Catálogo de unidades de medida para líquidos a granel (subconjunto L/K).</summary>
        [HttpGet("unidades-medida")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesMedida()
            => _service.ListarUnidadesMedidaAsync();

        /// <summary>Obtiene una relación por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<RelacionLiquido>> ObtenerPorId(int id)
            => _service.ObtenerPorIdAsync(id);

        /// <summary>Alta de una relación.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarRelacionLiquidoRequest request)
        {
            request.IdRelacionEnvasadoAgranel = 0;
            return _service.GuardarAsync(request);
        }

        /// <summary>Edición de una relación.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarRelacionLiquidoRequest request)
        {
            request.IdRelacionEnvasadoAgranel = id;
            return _service.GuardarAsync(request);
        }

        /// <summary>Desactiva una relación (baja lógica).</summary>
        [HttpDelete("{id:int}")]
        public Task<Notificacion<string>> Desactivar(int id)
            => _service.DesactivarAsync(id);
    }
}
