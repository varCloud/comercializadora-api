using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Services.Ubicaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Generador de etiquetas QR de ubicaciones de almacén (módulo Productos). Migra la pantalla
    /// legada Productos/Ubicaciones: arma combos (almacén por sucursal, pisos/pasillos/racks) y
    /// genera un PDF imprimible con un QR por ubicación. Requiere JWT válido ([Authorize]).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/productos/ubicaciones")]
    public class UbicacionesController : ControllerBase
    {
        private readonly IUbicacionesService _service;
        private readonly IUbicacionesPdfService _pdfService;

        public UbicacionesController(IUbicacionesService service, IUbicacionesPdfService pdfService)
        {
            _service = service;
            _pdfService = pdfService;
        }

        /// <summary>Almacenes de una sucursal (para el combo).</summary>
        [HttpGet("catalogos/almacenes")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenes([FromQuery] int idSucursal)
            => _service.ObtenerAlmacenesAsync(idSucursal);

        /// <summary>Catálogo de pisos.</summary>
        [HttpGet("catalogos/pisos")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPisos()
            => _service.ObtenerPisosAsync();

        /// <summary>Catálogo de pasillos.</summary>
        [HttpGet("catalogos/pasillos")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPasillos()
            => _service.ObtenerPasillosAsync();

        /// <summary>Catálogo de racks.</summary>
        [HttpGet("catalogos/racks")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRacks()
            => _service.ObtenerRacksAsync();

        /// <summary>
        /// Recibe la lista de ubicaciones y devuelve el PDF con un QR por cada una (4 por fila).
        /// El archivo se devuelve en la respuesta (application/pdf); no se persiste en disco.
        /// </summary>
        [HttpPost("imprimir")]
        public IActionResult Imprimir([FromBody] List<UbicacionImprimir> ubicaciones)
        {
            if (ubicaciones is null || ubicaciones.Count == 0)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "Agrega al menos una ubicación para generar el PDF."
                });

            byte[] pdf = _pdfService.GenerarPdf(ubicaciones);
            string nombreArchivo = $"Ubicaciones_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(pdf, "application/pdf", nombreArchivo);
        }
    }
}
