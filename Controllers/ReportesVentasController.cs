using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Exportacion;
using comercializadora_api.Services.ReportesVentas;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte de Ventas — segundo sub-reporte del módulo "Reportes" legado
    /// (<c>ReportesController.Ventas/BuscarVentas</c> + <c>ReportesDAO.ObtenerVentas</c>).
    /// Corrección post-aprobación (2026-07-19): el listado pasa a paginar en memoria (mismo
    /// contrato <c>links</c>/<c>meta</c> que <see cref="ReportesInventarioController"/>, vía
    /// <see cref="IPaginationBuilder"/>) porque <c>SP_CONSULTA_VENTAS</c> no soporta
    /// OFFSET/FETCH y no se modifica. La exportación **sigue sin cambios de comportamiento**:
    /// respeta los filtros activos y trae TODAS las filas (no tiene el modo "exportar todo,
    /// ignora filtros" que sí tiene Inventario) — solo cambia de invocar
    /// <c>IVentaReporteService.ListarAsync</c> (ahora paginado) a
    /// <c>IVentaReporteService.ExportarAsync</c> (sin paginar), que conserva el mismo contrato
    /// <c>Notificacion&lt;T&gt;</c>/<c>EsExitoso</c> de antes de esta corrección. Usa
    /// <c>SP_CONSULTA_VENTAS</c> tal cual (sin <c>SP_V2_*</c>: no hay cambio de comportamiento
    /// que versionar).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/reportes/ventas")]
    public class ReportesVentasController : ControllerBase
    {
        private readonly IVentaReporteService _service;
        private readonly IPaginationBuilder _pagination;
        private readonly IExportacionService _exportacion;
        private readonly IUsuariosService _usuarios;
        private readonly ExportacionOptions _exportacionOpciones;

        public ReportesVentasController(
            IVentaReporteService service,
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
        /// Listado paginado (en memoria) con filtros de pantalla: idLineaProducto, idCliente,
        /// idUsuario, fechaIni, fechaFin, page, perPage (order/sort heredados de
        /// <see cref="Common.PagedQuery"/> pero sin whitelist propia: no se pide ordenar
        /// explícitamente, el SP conserva su orden natural).
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<VentaReporteItem>>> Listar([FromQuery] VentaReporteQuery filtros)
        {
            var page = await _service.ListarAsync(filtros);
            return _pagination.Build(page, filtros, Request);
        }

        /// <summary>
        /// Exporta a CSV con los MISMOS filtros del listado (a diferencia de
        /// <see cref="ReportesInventarioController.Exportar"/>, no ignora los filtros de
        /// pantalla), TODAS las filas sin paginar (<see cref="IVentaReporteService.ExportarAsync"/>,
        /// no el <c>ListarAsync</c> paginado que usa <see cref="Listar"/>). Resuelve destinatario
        /// vía <see cref="IUsuariosService.ObtenerPorIdAsync"/> y delega la regla de umbral
        /// descarga-vs-correo en <see cref="IExportacionService"/>.
        /// </summary>
        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar([FromQuery] VentaReporteQuery filtros, CancellationToken cancellationToken)
        {
            var ventas = await _service.ExportarAsync(filtros);
            if (!ventas.EsExitoso)
                return BadRequest(new Notificacion<string> { Estatus = ventas.Estatus, Mensaje = ventas.Mensaje });

            var destinatario = await ResolverDestinatarioAsync();
            if (destinatario is null)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "No hay destinatario de exportación configurado (Exportacion:CorreoDestino); no es posible exportar (se necesita para el envío diferido).",
                });

            var datos = (ventas.Modelo ?? Enumerable.Empty<VentaReporteItem>()).ToList();

            var resultado = await _exportacion.ExportarAsync(
                datos, Columnas, "ReporteVentas", destinatario, cancellationToken);

            return resultado.EsDescargaInmediata
                ? File(resultado.Archivo!, ExportacionService.ContentTypeCsv, resultado.NombreArchivo)
                : Ok(new Notificacion<string> { Estatus = 200, Mensaje = resultado.Mensaje });
        }

        /// <summary>Columnas del CSV (paridad con las 17 columnas de <c>_Ventas.cshtml</c>).</summary>
        private static readonly IReadOnlyList<ColumnaExportable<VentaReporteItem>> Columnas = new List<ColumnaExportable<VentaReporteItem>>
        {
            new("Fecha", i => i.Fecha),
            new("Sucursal", i => i.Sucursal),
            new("Tienda", i => i.Tienda),
            new("Cajero", i => i.Cajero),
            new("Folio", i => i.Folio),
            new("Cliente", i => i.Cliente),
            new("Codigo Barras", i => i.CodigoBarras),
            new("Linea", i => i.LineaProducto),
            new("Producto", i => i.Producto),
            new("Cantidad", i => i.Cantidad),
            new("Precio Venta", i => i.PrecioVenta),
            new("I.V.A.", i => i.Iva),
            new("Monto Total", i => i.MontoTotal),
            new("Costo Compra", i => i.CostoCompra),
            new("Utilidad", i => i.Utilidad),
            new("Margen Bruto", i => i.MargenBruto),
            new("Forma de Pago", i => i.FormaPago),
        };

        /// <summary>
        /// Resuelve nombre del usuario autenticado (para personalizar el cuerpo del correo) +
        /// destinatario(s) del envío diferido. El correo NUNCA sale de <c>Usuario</c> (esa columna
        /// no existe, ni existió en el legado): sale de <see cref="ExportacionOptions.CorreoDestino"/>
        /// (mismo criterio que <c>correoCCFacturas</c> del legado — lista fija, no personal). Null
        /// si no hay ningún destino configurado.
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
