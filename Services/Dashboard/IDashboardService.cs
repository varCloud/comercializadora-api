using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Enums;

namespace comercializadora_api.Services.Dashboard
{
    /// <summary>
    /// Lógica de negocio del dashboard. Aplica la regla de visibilidad por estación/rol
    /// (un cajero -idRol == 3- solo ve su estación) y compone los KPIs como el index() legado.
    /// Los métodos reciben idRol e idEstacion ya resueltos del JWT por el controller.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>KPIs del dashboard (ventas por periodo, información global, merma y costo).</summary>
        Task<Notificacion<DashboardKpis>> ObtenerKpisAsync(int idRol, int idEstacionToken);

        /// <summary>
        /// Gráfico de ventas por fecha (con categorías para drilldown del front).
        /// <paramref name="fechaConsulta"/> opcional: null = periodo actual; con fecha se
        /// consulta un periodo histórico (útil con respaldos antiguos).
        /// </summary>
        Task<Notificacion<VentasPorFecha>> ObtenerVentasPorFechaAsync(
            EnumTipoReporteGrafico tipoReporte, int idRol, int idEstacionToken, DateTime? fechaConsulta = null);

        /// <summary>Ventas por estación en un rango de fechas opcional.</summary>
        Task<Notificacion<IEnumerable<EstacionVenta>>> ObtenerVentasPorEstacionAsync(
            DateTime? fechaIni, DateTime? fechaFin, int idRol, int idEstacionToken);

        /// <summary>Top ten (productos/clientes/proveedores).</summary>
        Task<Notificacion<IEnumerable<Categoria>>> ObtenerTopTenAsync(
            EnumTipoReporteGrafico tipoReporte, EnumTipoGrafico tipoGrafico, int idRol, int idEstacionToken);

        /// <summary>Información global por periodo.</summary>
        Task<Notificacion<IEnumerable<Categoria>>> ObtenerInformacionGlobalAsync(
            EnumTipoReporteGrafico tipoReporte, int idRol, int idEstacionToken);

        /// <summary>Merma mensual (actual vs. anterior, según devuelva el SP).</summary>
        Task<Notificacion<IEnumerable<MermaMensual>>> ObtenerMermaAsync();

        /// <summary>Costo de producción a granel mensual.</summary>
        Task<Notificacion<IEnumerable<CostoProduccionMensual>>> ObtenerCostoProduccionAsync();

        /// <summary>IVA acumulado por fecha.</summary>
        Task<Notificacion<IEnumerable<Categoria>>> ObtenerIvaAcumuladoAsync(
            EnumTipoReporteGrafico tipoReporte, int idRol, int idEstacionToken);
    }
}
