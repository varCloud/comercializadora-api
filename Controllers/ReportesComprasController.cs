using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Exportacion;
using comercializadora_api.Services.ReportesCompras;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte de Compras — quinto sub-reporte del módulo "Reportes" legado (migra
    /// <c>ReportesController.BuscarCompras</c> + <c>ComprasDAO.ObtenerCompras</c>, resumido por
    /// compra en vez de por producto). Listado paginado con filtros de pantalla (todos
    /// independientes, sin cascada) + exportación CSV completa (ignora la paginación, respeta
    /// los filtros). Usa <c>SP_V2_CONSULTA_COMPRAS_REPORTE</c> (paginado server-side, distinto de
    /// <c>SP_V2_CONSULTA_COMPRAS</c> que usa el módulo CRUD de Compras — ver el .sql).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/reportes/compras")]
    public class ReportesComprasController : ControllerBase
    {
        private readonly IComprasReporteService _service;
        private readonly IPaginationBuilder _pagination;
        private readonly IExportacionService _exportacion;
        private readonly IUsuariosService _usuarios;
        private readonly ExportacionOptions _exportacionOpciones;

        public ReportesComprasController(
            IComprasReporteService service,
            IPaginationBuilder pagination,
            IExportacionService exportacion,
            IUsuariosService usuarios,
            IOptions<ExportacionOptions> exportacionOpciones)
        {
            _service = service;
            _pagination = pagination;
            _exportacion = exportacion;
            _usuarios = usuarios;
            _exportacionOpciones = exportacionOpciones.Value;
        }

        /// <summary>
        /// Listado paginado (server-side). Query: fechaIni, fechaFin, idProveedor,
        /// idLineaProducto, idUsuario, idStatusCompra, page, perPage, order, sort.
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<CompraReporteItem>>> Listar([FromQuery] ComprasReporteQuery filtros)
        {
            var page = await _service.ListarAsync(filtros);
            return _pagination.Build(page, filtros, Request);
        }

        /// <summary>Catálogo de proveedores para el filtro.</summary>
        [HttpGet("proveedores")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerProveedores()
            => _service.ObtenerProveedoresAsync();

        /// <summary>Catálogo de líneas de producto para el filtro.</summary>
        [HttpGet("lineas")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineas()
            => _service.ObtenerLineasAsync();

        /// <summary>Catálogo de compradores (usuarios) para el filtro.</summary>
        [HttpGet("compradores")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerCompradores()
            => _service.ObtenerCompradoresAsync();

        /// <summary>Catálogo de estatus de compra para el filtro.</summary>
        [HttpGet("estatus")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatus()
            => _service.ObtenerEstatusAsync();

        /// <summary>
        /// Exporta a CSV TODAS las compras que cumplen los filtros de pantalla (ignora page/perPage).
        /// Mismo patrón que <see cref="ReportesVentasController.Exportar"/>/
        /// <see cref="ReportesInventarioController.Exportar"/>: resuelve destinatario vía
        /// <see cref="IUsuariosService.ObtenerPorIdAsync"/> y delega la regla de umbral
        /// (descarga inmediata vs. correo) en <see cref="IExportacionService"/>.
        /// </summary>
        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar([FromQuery] ComprasReporteQuery filtros, CancellationToken cancellationToken)
        {
            var compras = await _service.ExportarAsync(filtros);
            if (!compras.EsExitoso)
                return BadRequest(new Notificacion<string> { Estatus = compras.Estatus, Mensaje = compras.Mensaje });

            var destinatario = await ResolverDestinatarioAsync();
            if (destinatario is null)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "No hay destinatario de exportación configurado (Exportacion:CorreoDestino); no es posible exportar (se necesita para el envío diferido).",
                });

            var datos = (compras.Modelo ?? Enumerable.Empty<CompraReporteItem>()).ToList();
            var resultado = await _exportacion.ExportarAsync(
                datos, ColumnasReporte, "ReporteCompras", destinatario, cancellationToken);

            return resultado.EsDescargaInmediata
                ? File(resultado.Archivo!, ExportacionService.ContentTypeCsv, resultado.NombreArchivo)
                : Ok(new Notificacion<string> { Estatus = 200, Mensaje = resultado.Mensaje });
        }

        /// <summary>Columnas del CSV (paridad con las columnas de la tabla del reporte).</summary>
        private static readonly IReadOnlyList<ColumnaExportable<CompraReporteItem>> ColumnasReporte = new List<ColumnaExportable<CompraReporteItem>>
        {
            new("ID Compra", c => c.IdCompra),
            new("Proveedor", c => c.ProveedorNombre),
            new("Comprador", c => c.NombreCompleto),
            new("Fecha Compra", c => c.FechaAlta),
            new("Total", c => c.MontoTotal),
            new("Estatus", c => c.EstatusDescripcion),
            new("Productos", c => c.TotalCantProductos),
        };

        /// <summary>
        /// Resuelve nombre del usuario autenticado (para personalizar el cuerpo del correo) +
        /// destinatario(s) del envío diferido. El correo NUNCA sale de <c>Usuario</c> (esa columna
        /// no existe): sale de <see cref="ExportacionOptions.CorreoDestino"/> (mismo criterio que
        /// <c>ReportesVentasController</c>/<c>ReportesInventarioController</c>). Null si no hay
        /// ningún destino configurado.
        /// </summary>
        private async Task<DestinatarioExportacion?> ResolverDestinatarioAsync()
        {
            if (_exportacionOpciones.CorreoDestino.Count == 0)
                return null;

            var idUsuario = Claim("idUsuario");
            var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario);
            var nombreCompleto = usuario.EsExitoso
                ? usuario.Modelo?.NombreCompleto ?? usuario.Modelo?.NombreUsuario ?? "Usuario"
                : "Usuario";

            return new DestinatarioExportacion(
                idUsuario,
                nombreCompleto,
                _exportacionOpciones.CorreoDestino[0],
                _exportacionOpciones.CorreoDestino.Skip(1).ToList());
        }

        /// <summary>Claim numérico del JWT (0 si ausente o inválido).</summary>
        private int Claim(string tipo)
            => int.TryParse(User.FindFirst(tipo)?.Value, out var valor) ? valor : 0;
    }
}
