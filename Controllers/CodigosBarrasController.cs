using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Services.CodigosBarras;
using comercializadora_api.Services.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Generador de etiquetas de código de barras de productos (módulo Productos, Fase E).
    /// Migra la pantalla legada Productos/CodigosDeBarras: arma una lista de productos (uno a uno
    /// o una línea completa) y genera un PDF imprimible con la barra CODE_128 + precios. JWT ([Authorize]).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/productos/codigos-barras")]
    public class CodigosBarrasController : ControllerBase
    {
        private readonly IProductosService _productosService;
        private readonly ICodigosBarrasPdfService _pdfService;

        public CodigosBarrasController(IProductosService productosService, ICodigosBarrasPdfService pdfService)
        {
            _productosService = productosService;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Todos los productos activos de una línea, con descripción, precios y código de barras
        /// (alimenta "Agregar línea"). Reutiliza SP_V2_CONSULTA_PRODUCTOS filtrando por línea.
        /// </summary>
        [HttpGet("por-linea")]
        public Task<Notificacion<IEnumerable<Producto>>> ObtenerPorLinea([FromQuery] int idLineaProducto)
            => _productosService.ObtenerPorLineaAsync(idLineaProducto);

        /// <summary>
        /// Recibe la lista de productos y devuelve el PDF de etiquetas (2 por fila).
        /// El archivo va en la respuesta (application/pdf); no se persiste en disco.
        /// </summary>
        [HttpPost("generar")]
        public IActionResult Generar([FromBody] List<ProductoCodigoBarra> productos)
        {
            if (productos is null || productos.Count == 0)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "Agrega al menos un producto para generar el PDF."
                });

            byte[] pdf = _pdfService.GenerarPdf(productos);
            string nombreArchivo = $"CodigosBarras_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            return File(pdf, "application/pdf", nombreArchivo);
        }
    }
}
