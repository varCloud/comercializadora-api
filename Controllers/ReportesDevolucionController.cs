using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Exportacion;
using comercializadora_api.Services.ReportesDevolucion;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte de Devoluciones y Complementos — cuarto sub-reporte del módulo "Reportes" legado
    /// (<c>ReportesController.Devoluciones</c> + <c>ReportesDAO.ObtenerDevolucionesyComplementos</c>).
    /// Paginación en memoria desde el día uno (mismo criterio que <c>ReportesMermaController</c>):
    /// <c>SP_CONSULTA_DEVOLUCIONES_Y_COMPLEMENTOS</c> no soporta OFFSET/FETCH y no se modifica
    /// (decisión cerrada de la HU: se reutiliza tal cual, sin <c>SP_V2_*</c>). Exportación CSV
    /// agregada como excepción al legado (no existía), mismo patrón <c>ExportacionOptions.
    /// CorreoDestino</c> que <c>ReportesVentasController</c>/<c>ReportesMermaController</c>
    /// (destinatario fijo, no el correo personal del usuario).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/reportes/devoluciones")]
    public class ReportesDevolucionController : ControllerBase
    {
        private readonly IDevolucionReporteService _service;
        private readonly IPaginationBuilder _pagination;
        private readonly IExportacionService _exportacion;
        private readonly IUsuariosService _usuarios;
        private readonly ExportacionOptions _exportacionOpciones;

        public ReportesDevolucionController(
            IDevolucionReporteService service,
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
        /// Listado paginado (en memoria) con filtros de pantalla: fechaIni/fechaFin (obligatorios,
        /// fechaIni ≤ fechaFin), idAlmacen, idUsuario, tipoTicket (1 = Devoluciones [default],
        /// 2 = Complementos — el SP no admite "todos" para este filtro, ver
        /// <see cref="Repositories.ReportesDevolucion.DevolucionReporteRepository"/>), page, perPage
        /// (acotado 5-100 por contrato de la HU). El clamp se aplica aquí, ANTES de repository y
        /// <see cref="IPaginationBuilder"/>, para que ambos vean el mismo valor — <c>PaginationBuilder.
        /// Build</c> recalcula <c>meta.perPage</c>/<c>links</c> a partir de <c>filtros.PerPage</c>
        /// directamente (no del tamaño real de página que use el repositorio), así que clampear solo
        /// dentro del repository desalinearía meta/links con las filas realmente devueltas.
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<DevolucionItem>>> Listar([FromQuery] DevolucionQuery filtros)
        {
            filtros.PerPage = Math.Clamp(filtros.PerPage < 1 ? 20 : filtros.PerPage, 5, 100);
            var page = await _service.ListarAsync(filtros);
            return _pagination.Build(page, filtros, Request);
        }

        /// <summary>
        /// Exporta a CSV con los MISMOS filtros del listado (respeta filtros activos, sin modo
        /// "exportar todo"), TODAS las filas sin paginar. Resuelve destinatario vía
        /// <see cref="ExportacionOptions.CorreoDestino"/> (lista fija, no el correo personal del
        /// usuario) y delega la regla de umbral descarga-vs-correo en <see cref="IExportacionService"/>.
        /// </summary>
        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar([FromQuery] DevolucionQuery filtros, CancellationToken cancellationToken)
        {
            var devoluciones = await _service.ExportarAsync(filtros);
            if (!devoluciones.EsExitoso)
                return BadRequest(new Notificacion<string> { Estatus = devoluciones.Estatus, Mensaje = devoluciones.Mensaje });

            var destinatario = await ResolverDestinatarioAsync();
            if (destinatario is null)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "No hay destinatario de exportación configurado (Exportacion:CorreoDestino); no es posible exportar (se necesita para el envío diferido).",
                });

            var datos = (devoluciones.Modelo ?? Enumerable.Empty<DevolucionItem>()).ToList();

            var resultado = await _exportacion.ExportarAsync(
                datos, Columnas, "ReporteDevoluciones", destinatario, cancellationToken);

            return resultado.EsDescargaInmediata
                ? File(resultado.Archivo!, ExportacionService.ContentTypeCsv, resultado.NombreArchivo)
                : Ok(new Notificacion<string> { Estatus = 200, Mensaje = resultado.Mensaje });
        }

        /// <summary>Columnas del CSV (paridad con los campos de <c>DevolucionItem</c> mostrados en pantalla).</summary>
        private static readonly IReadOnlyList<ColumnaExportable<DevolucionItem>> Columnas = new List<ColumnaExportable<DevolucionItem>>
        {
            new("Fecha", i => i.FechaAlta),
            new("Tipo", i => i.Descripcion),
            new("Venta", i => i.IdVenta),
            new("Cliente", i => i.NombreCliente),
            new("Almacen", i => i.DescAlmacen),
            new("Codigo Barras", i => i.CodigoBarras),
            new("Producto", i => i.DescripcionProducto),
            new("Cantidad", i => i.Cantidad),
            new("Precio Venta", i => i.PrecioVenta),
            new("Monto Total", i => i.MontoTotal),
            new("Usuario", i => i.NombreUsuario),
        };

        /// <summary>
        /// Resuelve nombre del usuario autenticado (para personalizar el cuerpo del correo) +
        /// destinatario(s) del envío diferido. El correo sale de
        /// <see cref="ExportacionOptions.CorreoDestino"/> (mismo criterio que <c>correoCCFacturas</c>
        /// del legado — lista fija, no personal), NO de <c>Usuario.Correo</c> (mismo patrón que
        /// <c>ReportesVentasController</c>/<c>ReportesMermaController</c>). El primer correo de la
        /// lista es el destinatario principal (To); el resto va en copia oculta (Bcc). Null si no
        /// hay ningún destino configurado.
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
