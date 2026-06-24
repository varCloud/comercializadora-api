using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Proveedores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Administración de proveedores. Migra ProveedoresController + ProveedorDAO del legado,
    /// con verbos HTTP correctos y listado paginado estilo Laravel (data/links/meta).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/proveedores")]
    public class ProveedoresController : ControllerBase
    {
        private readonly IProveedoresService _proveedoresService;
        private readonly IPaginationBuilder _pagination;

        public ProveedoresController(IProveedoresService proveedoresService, IPaginationBuilder pagination)
        {
            _proveedoresService = proveedoresService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Filtros por query: page, perPage, q, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<Proveedor>>> Listar([FromQuery] PagedQuery query)
        {
            var page = await _proveedoresService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Obtiene un proveedor por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<Proveedor>> ObtenerPorId(int id)
            => _proveedoresService.ObtenerPorIdAsync(id);

        /// <summary>Alta de proveedor.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarProveedorRequest request)
        {
            request.IdProveedor = 0;
            return _proveedoresService.GuardarAsync(request);
        }

        /// <summary>Edición de proveedor.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarProveedorRequest request)
        {
            request.IdProveedor = id;
            return _proveedoresService.GuardarAsync(request);
        }

        /// <summary>Activa/desactiva un proveedor (borrado lógico).</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> CambiarEstatus(int id, [FromBody] CambiarEstatusRequest request)
            => _proveedoresService.CambiarEstatusAsync(id, request.Activo);
    }
}
