using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Exportacion;
using comercializadora_api.Services.InventariosFisicos;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Módulo "Inventario Físico" (conteo y ajuste de existencias por sucursal). Migra la
    /// pantalla web <c>InventarioFisicoController</c> del legado (los WS móviles
    /// <c>WsInventarioFisicoController</c>/<c>WsInventarioController</c> quedan fuera de
    /// alcance). El idUsuario de los comandos y el idSucursal del listado se toman de los
    /// claims del JWT (paridad con la sesión legada). Los catálogos de almacenes/líneas para
    /// los filtros del diálogo de ajuste se reusan de los módulos ya migrados.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/inventario-fisico")]
    public class InventarioFisicoController : ControllerBase
    {
        private readonly IInventarioFisicoService _service;
        private readonly IPaginationBuilder _pagination;
        private readonly IExportacionService _exportacion;
        private readonly IUsuariosService _usuarios;
        private readonly ExportacionOptions _exportacionOpciones;

        public InventarioFisicoController(
            IInventarioFisicoService service,
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

        /// <summary>Listado paginado de la sucursal del usuario. Query: page, perPage, idTipoInventario, fechaIni, fechaFin, order, sort.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<InventarioFisico>>> Listar(
            [FromQuery] InventarioFisicoQuery query)
        {
            var page = await _service.ListarAsync(query, Claim("idSucursal"));
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Alta de inventario físico (estatus 1 Pendiente). El idUsuario se toma del JWT.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarInventarioFisicoRequest request)
            => _service.GuardarAsync(0, request.Nombre, Claim("idUsuario"));

        /// <summary>Renombrado inline de un inventario. El idUsuario se toma del JWT.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Renombrar(int id, [FromBody] GuardarInventarioFisicoRequest request)
            => _service.GuardarAsync(id, request.Nombre, Claim("idUsuario"));

        /// <summary>Transición de estatus: 2 iniciar, 3 finalizar (afecta inventario), 4 cancelar.</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> ActualizarEstatus(
            int id, [FromBody] ActualizarEstatusInventarioFisicoRequest request)
            => _service.ActualizarEstatusAsync(id, request.IdEstatus, request.Observaciones, Claim("idUsuario"));

        /// <summary>Ajustes del inventario (lista completa, sin paginar). Query: idAlmacen, idLineaProducto (null/0 = TODOS).</summary>
        [HttpGet("{id:int}/ajustes")]
        public Task<Notificacion<IEnumerable<AjusteInventarioFisico>>> ObtenerAjustes(
            int id, [FromQuery] AjustesInventarioQuery query)
            => _service.ObtenerAjustesAsync(id, query);

        /// <summary>
        /// Exporta los ajustes a CSV. Migra <c>generaCSVInventario</c> del legado (que siempre
        /// descargaba Y enviaba por correo) aplicando la nueva regla de umbral vía
        /// <see cref="IExportacionService"/>: <c>&lt;= umbral</c> descarga inmediata (200 + archivo);
        /// <c>&gt; umbral</c> se procesa en segundo plano y se notifica que llegará por correo.
        /// </summary>
        [HttpGet("{id:int}/ajustes/exportar")]
        public async Task<IActionResult> ExportarAjustes(
            int id, [FromQuery] AjustesInventarioQuery query, CancellationToken cancellationToken)
        {
            var ajustes = await _service.ObtenerAjustesAsync(id, query);
            if (!ajustes.EsExitoso)
                return BadRequest(new Notificacion<string> { Estatus = ajustes.Estatus, Mensaje = ajustes.Mensaje });

            var destinatario = await ResolverDestinatarioAsync();
            if (destinatario is null)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "No hay destinatario de exportación configurado (Exportacion:CorreoDestino); no es posible exportar (se necesita para el envío diferido).",
                });

            var datos = (ajustes.Modelo ?? Enumerable.Empty<AjusteInventarioFisico>()).ToList();
            var resultado = await _exportacion.ExportarAsync(
                datos, ColumnasAjustesInventario, $"AjustesInventario_{id}", destinatario, cancellationToken);

            return resultado.EsDescargaInmediata
                ? File(resultado.Archivo!, ExportacionService.ContentTypeCsv, resultado.NombreArchivo)
                : Ok(new Notificacion<string> { Estatus = 200, Mensaje = resultado.Mensaje });
        }

        private static readonly IReadOnlyList<ColumnaExportable<AjusteInventarioFisico>> ColumnasAjustesInventario = new List<ColumnaExportable<AjusteInventarioFisico>>
        {
            new("Producto", a => a.Producto?.Descripcion),
            new("Línea", a => a.Producto?.DescripcionLinea),
            new("Almacén", a => a.Producto?.Almacen),
            new("Cantidad Sistema", a => a.CantidadActual),
            new("Cantidad Física", a => a.CantidadEnFisico),
            new("Cantidad a Ajustar", a => a.CantidadAAjustar),
            new("Último Costo Compra", a => a.Producto?.UltimoCostoCompra),
        };

        /// <summary>Resuelve nombre/correo del usuario autenticado; null si no tiene correo registrado.</summary>
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
