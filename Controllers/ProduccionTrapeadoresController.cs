using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.ProduccionTrapeadores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte "Producción Trapeadores" (carga de mercancía de trapeadores). Hermano de
    /// "Producción Líquidos": reutiliza el mismo SP paginado
    /// (<c>SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS</c>) fijando <c>idTipoMovInventario = 32</c>
    /// en el repository. Es un reporte de **solo lectura**: sin alta/edición/baja. Los catálogos
    /// de roles/usuarios para los filtros se reusan del módulo Usuarios ya migrado (no se
    /// duplican aquí).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/produccion-trapeadores")]
    public class ProduccionTrapeadoresController : ControllerBase
    {
        private readonly IProduccionTrapeadoresService _produccionTrapeadoresService;
        private readonly IPaginationBuilder _pagination;

        public ProduccionTrapeadoresController(
            IProduccionTrapeadoresService produccionTrapeadoresService, IPaginationBuilder pagination)
        {
            _produccionTrapeadoresService = produccionTrapeadoresService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado. Query: page, perPage, idRol, idUsuario, fechaIni, fechaFin.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<CargaMercanciaTrapeadores>>> Listar(
            [FromQuery] ProduccionTrapeadoresQuery query)
        {
            var page = await _produccionTrapeadoresService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }
    }
}
