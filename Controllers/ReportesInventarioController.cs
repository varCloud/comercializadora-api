using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Exportacion;
using comercializadora_api.Services.ReportesInventario;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Reporte de Inventario — primera sub-feature del módulo "Reportes" legado
    /// (<c>ReportesController.Inventario/BuscarInventario/ReporteGeneral</c> +
    /// <c>ReportesDAO.ObtenerInventario/ObtenerReporteGeneral</c>). Listado paginado con
    /// filtros de pantalla + 2 exportaciones completas (General/Ubicación) que ignoran esos
    /// filtros, paridad exacta con el legado. Ambos endpoints usan
    /// <c>SP_V2_CONSULTA_INVENTARIO</c> (unifica los 2 SP legados).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/reportes/inventario")]
    public class ReportesInventarioController : ControllerBase
    {
        private readonly IInventarioReporteService _service;
        private readonly IPaginationBuilder _pagination;
        private readonly IExportacionService _exportacion;
        private readonly IUsuariosService _usuarios;

        public ReportesInventarioController(
            IInventarioReporteService service,
            IPaginationBuilder pagination,
            IExportacionService exportacion,
            IUsuariosService usuarios)
        {
            _service = service;
            _pagination = pagination;
            _exportacion = exportacion;
            _usuarios = usuarios;
        }

        /// <summary>
        /// Listado paginado. Query: idLineaProducto, idAlmacen, q (búsqueda libre sobre
        /// descripción/artículo), fechaIni, fechaFin, page, perPage, order, sort.
        /// </summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<InventarioReporteItem>>> Listar(
            [FromQuery] InventarioReporteQuery query)
        {
            var page = await _service.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>
        /// Exporta TODO el inventario a CSV (ignora los filtros de pantalla, paridad legado).
        /// <c>tipo=1</c> General, <c>tipo=2</c> Ubicación. Mismo patrón que
        /// <see cref="InventarioFisicoController.ExportarAjustes"/>: resuelve destinatario vía
        /// <see cref="IUsuariosService.ObtenerPorIdAsync"/>, arma <see cref="ColumnaExportable{T}"/>
        /// según <c>tipo</c> y delega la regla de umbral en <see cref="IExportacionService"/>.
        /// </summary>
        [HttpGet("exportar")]
        public async Task<IActionResult> Exportar([FromQuery] int tipo, CancellationToken cancellationToken)
        {
            if (tipo != 1 && tipo != 2)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "El parámetro 'tipo' debe ser 1 (General) o 2 (Ubicación).",
                });

            var inventario = await _service.ExportarAsync(tipo);
            if (!inventario.EsExitoso)
                return BadRequest(new Notificacion<string> { Estatus = inventario.Estatus, Mensaje = inventario.Mensaje });

            var destinatario = await ResolverDestinatarioAsync();
            if (destinatario is null)
                return BadRequest(new Notificacion<string>
                {
                    Estatus = 400,
                    Mensaje = "El usuario no tiene correo registrado; no es posible exportar (se necesita para el envío diferido).",
                });

            var datos = (inventario.Modelo ?? Enumerable.Empty<InventarioReporteExportItem>()).ToList();
            var columnas = tipo == 1 ? ColumnasGeneral : ColumnasUbicacion;
            var nombreReporte = tipo == 1 ? "ReporteInventarioGeneral" : "ReporteInventarioUbicacion";

            var resultado = await _exportacion.ExportarAsync(
                datos, columnas, nombreReporte, destinatario, cancellationToken);

            return resultado.EsDescargaInmediata
                ? File(resultado.Archivo!, ExportacionService.ContentTypeCsv, resultado.NombreArchivo)
                : Ok(new Notificacion<string> { Estatus = 200, Mensaje = resultado.Mensaje });
        }

        /// <summary>Columnas del "Reporte General" (paridad con generaCSVInventario tipo 1 del legado).</summary>
        private static readonly IReadOnlyList<ColumnaExportable<InventarioReporteExportItem>> ColumnasGeneral = new List<ColumnaExportable<InventarioReporteExportItem>>
        {
            new("IdProducto", i => i.IdProducto),
            new("Descripcion", i => i.Descripcion),
            new("Ultimo Costo Compra", i => i.UltimoCostoCompra),
            new("Precio Individual", i => i.PrecioIndividual),
            new("Precio Menudeo", i => i.PrecioMenudeo),
            new("Cantidad", i => i.Cantidad),
        };

        /// <summary>Columnas del "Reporte por Ubicación" (paridad con generaCSVInventario tipo 2 del legado).</summary>
        private static readonly IReadOnlyList<ColumnaExportable<InventarioReporteExportItem>> ColumnasUbicacion = new List<ColumnaExportable<InventarioReporteExportItem>>
        {
            new("IdProducto", i => i.IdProducto),
            new("Descripcion", i => i.Descripcion),
            new("Ultimo Costo Compra", i => i.UltimoCostoCompra),
            new("Precio Individual", i => i.PrecioIndividual),
            new("Precio Menudeo", i => i.PrecioMenudeo),
            new("Cantidad", i => i.Cantidad),
            new("Almacen", i => i.Almacen),
            new("Pasillo", i => i.Pasillo),
            new("Raq", i => i.Raq),
            new("Piso", i => i.Piso),
        };

        /// <summary>Resuelve nombre/correo del usuario autenticado; null si no tiene correo registrado.</summary>
        private async Task<DestinatarioExportacion?> ResolverDestinatarioAsync()
        {
            var idUsuario = Claim("idUsuario");
            var usuario = await _usuarios.ObtenerPorIdAsync(idUsuario);
            if (!usuario.EsExitoso || string.IsNullOrWhiteSpace(usuario.Modelo?.Correo))
                return null;

            return new DestinatarioExportacion(idUsuario, usuario.Modelo.NombreCompleto ?? usuario.Modelo.NombreUsuario ?? "Usuario", usuario.Modelo.Correo);
        }

        /// <summary>Claim numérico del JWT (0 si ausente o inválido).</summary>
        private int Claim(string tipo)
            => int.TryParse(User.FindFirst(tipo)?.Value, out var valor) ? valor : 0;
    }
}
