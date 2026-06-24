using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Enums;

namespace comercializadora_api.Repositories.Dashboard
{
    /// <summary>
    /// Acceso a datos del dashboard. Envuelve los stored procedures del legado
    /// (SP_DASHBOARD_*) sin modificarlos. Replica el manejo de @idEstacion del legado:
    /// cuando idEstacion == 0 se pasa NULL al SP.
    /// </summary>
    public interface IDashboardRepository
    {
        /// <summary>
        /// Total de ventas por fecha (SP_DASHBOARD_CONSULTA_TOTAL_VENTAS_POR_FECHA).
        /// <paramref name="fechaConsulta"/> es opcional: cuando es null el SP ancla el periodo
        /// a la fecha actual (dbo.FechaActual()); pasando una fecha se consulta un periodo
        /// histórico (útil con respaldos de meses anteriores).
        /// </summary>
        Task<Notificacion<IEnumerable<Categoria>>> ObtenerVentasPorFechaAsync(
            EnumTipoReporteGrafico tipoReporte, int idEstacion, DateTime? fechaConsulta = null);

        /// <summary>Total de ventas por estación (SP_DASHBOARD_CONSULTA_TOTAL_VENTAS_POR_ESTACION).</summary>
        Task<Notificacion<IEnumerable<EstacionVenta>>> ObtenerVentasPorEstacionAsync(
            DateTime? fechaIni, DateTime? fechaFin, int idEstacion);

        /// <summary>Top ten (productos/clientes/proveedores) (SP_DASHBOARD_CONSULTA_TOP_TEN).</summary>
        Task<Notificacion<IEnumerable<Categoria>>> ObtenerTopTenAsync(
            EnumTipoReporteGrafico tipoReporte, EnumTipoGrafico tipoGrafico, int idEstacion);

        /// <summary>Información global (SP_DASHBOARD_CONSULTA_INFORMACION_GLOBAL).</summary>
        Task<Notificacion<IEnumerable<Categoria>>> ObtenerInformacionGlobalAsync(
            EnumTipoReporteGrafico tipoReporte, int idEstacion);

        /// <summary>Merma mensual (SP_DASHBOARD_MERMA, sin parámetros).</summary>
        Task<Notificacion<IEnumerable<MermaMensual>>> ObtenerMermaAsync();

        /// <summary>IVA acumulado por fecha (SP_DASHBOARD_OBTENER_IVA_ACUMULADO).</summary>
        Task<Notificacion<IEnumerable<Categoria>>> ObtenerIvaAcumuladoAsync(
            EnumTipoReporteGrafico tipoReporte, int idEstacion);

        /// <summary>Costo de producción a granel mensual (SP_DASHBOARD_COSTO_PRODUCCION, sin parámetros).</summary>
        Task<Notificacion<IEnumerable<CostoProduccionMensual>>> ObtenerCostoProduccionAsync();
    }
}
