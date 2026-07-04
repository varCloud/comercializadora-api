using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.ProduccionLiquidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte "Producción Líquidos" (carga de mercancía de productos líquidos). Migra la
    /// acción <c>BuscarCargaMercanciaLiquidos</c> de <c>ProductosController</c> del legado.
    /// Es un reporte de **solo lectura**: sin alta/edición/baja. Los catálogos de roles/usuarios
    /// para los filtros se reusan del módulo Usuarios ya migrado (no se duplican aquí).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/produccion-liquidos")]
    public class ProduccionLiquidosController : ControllerBase
    {
        private readonly IProduccionLiquidosService _produccionLiquidosService;
        private readonly IPaginationBuilder _pagination;

        public ProduccionLiquidosController(
            IProduccionLiquidosService produccionLiquidosService, IPaginationBuilder pagination)
        {
            _produccionLiquidosService = produccionLiquidosService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Query: page, perPage, idRol, idUsuario, fechaIni, fechaFin.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<CargaMercanciaLiquidos>>> Listar(
            [FromQuery] ProduccionLiquidosQuery query)
        {
            var page = await _produccionLiquidosService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }
    }
}
