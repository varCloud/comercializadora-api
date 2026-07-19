using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.ConsumoMpl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte de solo lectura "Consumo de MPL" (nombre técnico legado "Costo de Producción
    /// Agranel"). Migra <c>ReportesController.CostoProduccionAgranel/ObtenerCostoProduccion</c>
    /// del legado. Pantalla distinta del proceso operativo "Producción a granel"
    /// (<see cref="ProduccionAgranelController"/>), ya migrado. El controller legado solo exigía
    /// sesión iniciada (sin chequeo de rol); se mantiene el mismo criterio: [Authorize] sin
    /// restricción de rol.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/consumo-mpl")]
    public class ConsumoMplController : ControllerBase
    {
        private readonly IConsumoMplService _consumoMplService;
        private readonly IPaginationBuilder _pagination;

        public ConsumoMplController(IConsumoMplService consumoMplService, IPaginationBuilder pagination)
        {
            _consumoMplService = consumoMplService;
            _pagination = pagination;
        }

        /// <summary>
        /// Listado paginado. Query: anioCalculo, mesCalculo, idAlmacen, idLineaProducto (0/ausente
        /// = TODOS), q (producto por descripción/código de barras), page, perPage, order, sort.
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<CostoProduccionAgranel>>> Listar(
            [FromQuery] ConsumoMplQuery query)
        {
            var page = await _consumoMplService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Catálogo de años disponibles para el filtro Año.</summary>
        [HttpGet("catalogos/anios")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAnios()
            => _consumoMplService.ObtenerAniosAsync();

        /// <summary>Catálogo de meses disponibles para el año dado (cascada Año → Mes).</summary>
        [HttpGet("catalogos/meses")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMeses([FromQuery] int? anio)
            => _consumoMplService.ObtenerMesesAsync(anio);
    }
}
