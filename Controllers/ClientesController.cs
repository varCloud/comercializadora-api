using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Administración de clientes (persona física/moral con datos fiscales, de contacto,
    /// pedidos especiales y crédito). Migra ClientesController + ClienteDAO del legado con
    /// verbos HTTP correctos y listado paginado estilo Laravel (data/links/meta). Incluye los
    /// catálogos del form: tipos de cliente activos y regímenes fiscales (read-only).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/clientes")]
    public class ClientesController : ControllerBase
    {
        private readonly IClientesService _clientesService;
        private readonly IPaginationBuilder _pagination;

        public ClientesController(IClientesService clientesService, IPaginationBuilder pagination)
        {
            _clientesService = clientesService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Query: page, perPage, q, order (nombre|rfc|municipio), sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<Cliente>>> Listar([FromQuery] PagedQuery query)
        {
            var page = await _clientesService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Obtiene un cliente por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<Cliente>> ObtenerPorId(int id)
            => _clientesService.ObtenerPorIdAsync(id);

        /// <summary>Alta de cliente.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarClienteRequest request)
        {
            request.IdCliente = 0;
            return _clientesService.GuardarAsync(request);
        }

        /// <summary>Edición de cliente.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarClienteRequest request)
        {
            request.IdCliente = id;
            return _clientesService.GuardarAsync(request);
        }

        /// <summary>Activa/desactiva un cliente (baja lógica; booleano real — bug del legado corregido).</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> CambiarEstatus(int id, [FromBody] CambiarEstatusRequest request)
            => _clientesService.CambiarEstatusAsync(id, request.Activo);

        /// <summary>Catálogo de tipos de cliente activos (dropdown del form).</summary>
        [HttpGet("catalogos/tipos")]
        public Task<Notificacion<IEnumerable<TipoCliente>>> ObtenerTipos()
            => _clientesService.ObtenerTiposActivosAsync();

        /// <summary>Catálogo read-only de regímenes fiscales del SAT (dropdown del form).</summary>
        [HttpGet("catalogos/regimenes-fiscales")]
        public Task<Notificacion<IEnumerable<RegimenFiscal>>> ObtenerRegimenesFiscales()
            => _clientesService.ObtenerRegimenesFiscalesAsync();
    }
}
