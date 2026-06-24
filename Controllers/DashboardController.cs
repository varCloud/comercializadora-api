using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Enums;
using comercializadora_api.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Dashboard administrativo: KPIs y gráficos. Migra DashBoardController + DashboardDAO del
    /// legado, corrigiendo los verbos HTTP (todo consulta -> GET) y partiendo la antigua acción
    /// index() (que componía un ViewBag) en endpoints discretos. Requiere JWT válido; el rol y
    /// la estación se leen de los claims del token (idRol/idEstacion) para aplicar la
    /// visibilidad por estación (un cajero solo ve su estación).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
            => _dashboardService = dashboardService;

        /// <summary>KPIs del dashboard (ventas por periodo, información global, merma y costo).</summary>
        [HttpGet("kpis")]
        public Task<Notificacion<DashboardKpis>> ObtenerKpis()
            => _dashboardService.ObtenerKpisAsync(IdRol, IdEstacion);

        /// <summary>
        /// Gráfico de ventas por fecha. <paramref name="periodo"/> mapea a
        /// <see cref="EnumTipoReporteGrafico"/> (1=Semanal, 2=Mensual, 3=Anual, 4=Día).
        /// <paramref name="fechaConsulta"/> es opcional: si se omite, el periodo se ancla a la
        /// fecha actual; con una fecha (ej. 2026-01-15) se consulta ese periodo histórico, útil
        /// para visualizar respaldos de meses anteriores.
        /// </summary>
        [HttpGet("ventas-por-fecha")]
        public Task<Notificacion<VentasPorFecha>> ObtenerVentasPorFecha(
            [FromQuery] int periodo = 2,
            [FromQuery] DateTime? fechaConsulta = null)
            => _dashboardService.ObtenerVentasPorFechaAsync(
                (EnumTipoReporteGrafico)periodo, IdRol, IdEstacion, fechaConsulta);

        /// <summary>Ventas por estación en un rango de fechas opcional.</summary>
        [HttpGet("ventas-por-estacion")]
        public Task<Notificacion<IEnumerable<EstacionVenta>>> ObtenerVentasPorEstacion(
            [FromQuery] DateTime? fechaIni = null,
            [FromQuery] DateTime? fechaFin = null)
            => _dashboardService.ObtenerVentasPorEstacionAsync(fechaIni, fechaFin, IdRol, IdEstacion);

        /// <summary>
        /// Top ten. <paramref name="periodo"/> mapea a <see cref="EnumTipoReporteGrafico"/> y
        /// <paramref name="tipo"/> a <see cref="EnumTipoGrafico"/>
        /// (2=Productos, 3=Clientes, 4=Proveedores).
        /// </summary>
        [HttpGet("top-ten")]
        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerTopTen(
            [FromQuery] int periodo,
            [FromQuery] int tipo)
            => _dashboardService.ObtenerTopTenAsync(
                (EnumTipoReporteGrafico)periodo, (EnumTipoGrafico)tipo, IdRol, IdEstacion);

        /// <summary>
        /// Información global. <paramref name="periodo"/> mapea a
        /// <see cref="EnumTipoReporteGrafico"/> (por defecto 4=Día, como el index() legado).
        /// </summary>
        [HttpGet("informacion-global")]
        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerInformacionGlobal([FromQuery] int periodo = 4)
            => _dashboardService.ObtenerInformacionGlobalAsync((EnumTipoReporteGrafico)periodo, IdRol, IdEstacion);

        /// <summary>Merma mensual (mes actual vs. anterior).</summary>
        [HttpGet("merma")]
        public Task<Notificacion<IEnumerable<MermaMensual>>> ObtenerMerma()
            => _dashboardService.ObtenerMermaAsync();

        /// <summary>Costo de producción a granel mensual (mes actual vs. anterior).</summary>
        [HttpGet("costo-produccion")]
        public Task<Notificacion<IEnumerable<CostoProduccionMensual>>> ObtenerCostoProduccion()
            => _dashboardService.ObtenerCostoProduccionAsync();

        /// <summary>
        /// IVA acumulado por fecha. <paramref name="periodo"/> mapea a
        /// <see cref="EnumTipoReporteGrafico"/>. Se expone por completitud, pero la vista del
        /// legado no consumía este dato como gráfico propio (código muerto heredado): el SP se
        /// invocaba en CrearDataGraficoIVA y su resultado nunca se renderizaba.
        /// </summary>
        [HttpGet("iva-acumulado")]
        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerIvaAcumulado([FromQuery] int periodo = 2)
            => _dashboardService.ObtenerIvaAcumuladoAsync((EnumTipoReporteGrafico)periodo, IdRol, IdEstacion);

        /// <summary>Rol del usuario autenticado (claim "idRol"; 0 si ausente o inválido).</summary>
        private int IdRol => ParseClaim("idRol");

        /// <summary>Estación del usuario autenticado (claim "idEstacion"; 0 si ausente o inválido).</summary>
        private int IdEstacion => ParseClaim("idEstacion");

        private int ParseClaim(string tipo)
            => int.TryParse(User.FindFirst(tipo)?.Value, out var valor) ? valor : 0;
    }
}
