using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Exportacion;
using comercializadora_api.Services.ReportesMerma;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte de Merma — tercer sub-reporte del módulo "Reportes" legado
    /// (<c>ReportesController.Merma/ObtenerMerma/ObtenerMesesAnio</c> +
    /// <c>ReportesDAO.ObtenerMerma/ObtenerAnios/ObtenerMeses</c>). Paginación en memoria desde el
    /// día uno (a diferencia de <c>reporte_ventas</c>, que la agregó como corrección posterior):
    /// <c>SP_CONSULTA_MERMA</c> no soporta OFFSET/FETCH y no se modifica (decisión cerrada de la
    /// HU). Cascada Año→Mes fiel al legado vía <c>/anios</c> y <c>/meses?anio=</c>. Exportación CSV
    /// agregada como excepción al legado (no existía), mismo patrón <c>ExportacionOptions.
    /// CorreoDestino</c> que <c>ReportesVentasController</c> (destinatario fijo, no el correo
    /// personal del usuario) — respeta los filtros activos, sin modo "exportar todo".
    /// Usa <c>SP_CONSULTA_MERMA</c>/<c>SP_CONSULTA_ANIOS</c>/<c>SP_CONSULTA_MESES</c> tal cual (sin
    /// <c>SP_V2_*</c>: no hay cambio de comportamiento que versionar).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/reportes/merma")]
    public class ReportesMermaController : ControllerBase
    {
        private readonly IMermaReporteService _service;
        private readonly IPaginationBuilder _pagination;
        private readonly IExportacionService _exportacion;
        private readonly IUsuariosService _usuarios;
        private readonly ExportacionOptions _exportacionOpciones;

        public ReportesMermaController(
            IMermaReporteService service,
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
        /// Listado paginado (en memoria) con filtros de pantalla: anioCalculo, mesCalculo,
        /// idAlmacen, idLineaProducto, page, perPage. Sin anioCalculo/mesCalculo, el propio SP usa
        /// el mes/año calendario ACTUAL (verificado contra <c>SP_CONSULTA_MERMA</c> real, no "el
        /// más reciente con datos").
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<MermaItem>>> Listar([FromQuery] MermaQuery filtros)
        {
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
        public async Task<IActionResult> Exportar([FromQuery] MermaQuery filtros, CancellationToken cancellationToken)
        {
            var merma = await _service.ExportarAsync(filtros);
            if (!merma.EsExitoso)
                return BadRequest(new Notificacion<string> { Estatus = merma.Estatus, Mensaje = merma.Mensaje });

            var destinatario = await ResolverDestinatarioAsync();
            if (destinatario is null)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "No hay destinatario de exportación configurado (Exportacion:CorreoDestino); no es posible exportar (se necesita para el envío diferido).",
                });

            var datos = (merma.Modelo ?? Enumerable.Empty<MermaItem>()).ToList();

            var resultado = await _exportacion.ExportarAsync(
                datos, Columnas, "ReporteMerma", destinatario, cancellationToken);

            return resultado.EsDescargaInmediata
                ? File(resultado.Archivo!, ExportacionService.ContentTypeCsv, resultado.NombreArchivo)
                : Ok(new Notificacion<string> { Estatus = 200, Mensaje = resultado.Mensaje });
        }

        /// <summary>Catálogo de años con datos (para el filtro; la UI antepone/permite vacío = actual).</summary>
        [HttpGet("anios")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAnios()
            => _service.ObtenerAniosAsync();

        /// <summary>Catálogo de meses de un año (cascada: se repuebla al cambiar Año en el filtro).</summary>
        [HttpGet("meses")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMeses([FromQuery] int? anio = null)
            => _service.ObtenerMesesAsync(anio);

        /// <summary>Columnas del CSV (paridad con los campos de <c>MermaItem</c> mostrados en pantalla).</summary>
        private static readonly IReadOnlyList<ColumnaExportable<MermaItem>> Columnas = new List<ColumnaExportable<MermaItem>>
        {
            new("Fecha Alta", i => i.FechaAlta),
            new("Codigo Barras", i => i.CodigoBarras),
            new("Producto", i => i.DescripcionProducto),
            new("Linea", i => i.DescripcionLinea),
            new("Inventario Final Mes Anterior", i => i.InventarioFinalMesAnt),
            new("Total Compras", i => i.TotalCompras),
            new("Inventario Sistema", i => i.InventarioSistema),
            new("Merma", i => i.Merma),
            new("% Merma", i => i.PorcMerma),
            new("Ultimo Costo Compra", i => i.UltCostoCompra),
            new("Costo Merma", i => i.CostoMerma),
            new("Ultimo Dia Mes Calculo", i => i.UltimoDiaMesCalculo),
            new("Ultimo Dia Mes Anterior", i => i.UltimoDiaMesAnterior),
        };

        /// <summary>
        /// Resuelve nombre del usuario autenticado (para personalizar el cuerpo del correo) +
        /// destinatario(s) del envío diferido. El correo sale de
        /// <see cref="ExportacionOptions.CorreoDestino"/> (mismo criterio que
        /// <c>correoCCFacturas</c> del legado — lista fija, no personal), NO de
        /// <c>Usuario.Correo</c> (mismo patrón que <c>ReportesVentasController</c>). El primer
        /// correo de la lista es el destinatario principal (To); el resto va en copia oculta
        /// (Bcc) — ver <see cref="ExportacionOptions.CorreoDestino"/>. Null si no hay ningún
        /// destino configurado.
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
