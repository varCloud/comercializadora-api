using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Administración del catálogo de Productos (Fase A). Migra ProductosController + ProductosDAO
    /// del legado con verbos HTTP correctos y listado paginado (data/links/meta). El artículo y el
    /// código de barras se manejan como campos separados (SP_V2_INSERTA_ACTUALIZA_PRODUCTOS).
    /// Expone además la búsqueda para Compras, la lectura por código de barras y la búsqueda
    /// servidor de claves SAT. Requiere JWT válido ([Authorize], sin restricción por rol).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductosService _productosService;
        private readonly IPaginationBuilder _pagination;

        public ProductosController(IProductosService productosService, IPaginationBuilder pagination)
        {
            _productosService = productosService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Query: page, perPage, q, idLineaProducto, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<Producto>>> Listar([FromQuery] ProductosQuery query)
        {
            var page = await _productosService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Búsqueda por descripción para el modal de Compras (autocomplete).</summary>
        [HttpGet("buscar")]
        public Task<Notificacion<IEnumerable<Producto>>> BuscarPorDescripcion([FromQuery] string descripcion)
            => _productosService.BuscarPorDescripcionAsync(descripcion);

        /// <summary>Lectura exacta por código de barras (escaneo).</summary>
        [HttpGet("por-codigo")]
        public Task<Notificacion<Producto>> ObtenerPorCodigo([FromQuery] string codigo)
            => _productosService.ObtenerPorCodigoAsync(codigo);

        /// <summary>Búsqueda servidor de claves SAT (FactCatClaveProdServicio).</summary>
        [HttpGet("claves-sat")]
        public Task<Notificacion<IEnumerable<ClaveSat>>> BuscarClavesSat(
            [FromQuery] string? q = null,
            [FromQuery] int page = 1,
            [FromQuery] int perPage = 20)
            => _productosService.BuscarClavesSatAsync(q, page, perPage);

        /// <summary>Catálogo de líneas de producto.</summary>
        [HttpGet("catalogos/lineas")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineas()
            => _productosService.ObtenerLineasAsync();

        /// <summary>Catálogo de unidades de medida.</summary>
        [HttpGet("catalogos/unidades-medida")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesMedida()
            => _productosService.ObtenerUnidadesMedidaAsync();

        /// <summary>Catálogo de unidades de compra.</summary>
        [HttpGet("catalogos/unidades-compra")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesCompra()
            => _productosService.ObtenerUnidadesCompraAsync();

        /// <summary>Precios de un producto: base + rangos de mayoreo (Fase B).</summary>
        [HttpGet("{id:int}/precios")]
        public Task<Notificacion<PreciosProducto>> ObtenerPrecios(int id)
            => _productosService.ObtenerPreciosAsync(id);

        /// <summary>Guarda los precios base + rangos de mayoreo de un producto (Fase B).</summary>
        [HttpPut("{id:int}/precios")]
        public Task<Notificacion<string>> GuardarPrecios(int id, [FromBody] GuardarPreciosRequest request)
            => _productosService.GuardarPreciosAsync(id, request);

        /// <summary>Obtiene un producto por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<Producto>> ObtenerPorId(int id)
            => _productosService.ObtenerPorIdAsync(id);

        /// <summary>Alta de producto.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarProductoRequest request)
        {
            request.IdProducto = 0;
            return _productosService.GuardarAsync(request);
        }

        /// <summary>Edición de producto.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarProductoRequest request)
        {
            request.IdProducto = id;
            return _productosService.GuardarAsync(request);
        }

        /// <summary>Activa/desactiva un producto (baja lógica; bloquea baja con existencias).</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> CambiarEstatus(int id, [FromBody] CambiarEstatusRequest request)
            => _productosService.CambiarEstatusAsync(id, request.Activo);
    }
}
