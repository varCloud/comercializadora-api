using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Enums;
using comercializadora_api.Repositories.Dashboard;

namespace comercializadora_api.Services.Dashboard
{
    /// <summary>
    /// Implementación de la lógica de negocio del dashboard. Ver <see cref="IDashboardService"/>.
    /// </summary>
    public sealed class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository) => _repository = repository;

        /// <summary>
        /// Regla de visibilidad heredada del legado: solo el rol Cajero (idRol == 3) queda
        /// acotado a su estación; los demás roles ven todas (idEstacion = 0 => NULL en el SP).
        /// </summary>
        private static int ResolverEstacion(int idRol, int idEstacionToken)
            => idRol == 3 ? idEstacionToken : 0;

        public async Task<Notificacion<DashboardKpis>> ObtenerKpisAsync(int idRol, int idEstacionToken)
        {
            int idEstacionFiltro = ResolverEstacion(idRol, idEstacionToken);

            // Ventas por estación es la fuente de los totales (día/semana/mes/año). Si falla,
            // se propaga el error: sin ella los KPIs no tienen sentido. Los demás sub-SP
            // (información global, merma, costo) son complementarios y degradan a 0/null.
            var ventas = await _repository.ObtenerVentasPorEstacionAsync(null, null, idEstacionFiltro);
            if (!ventas.EsExitoso)
            {
                return new Notificacion<DashboardKpis>
                {
                    Estatus = ventas.Estatus,
                    Mensaje = ventas.Mensaje
                };
            }

            var kpis = new DashboardKpis();
            var estaciones = ventas.Modelo?.ToList() ?? new List<EstacionVenta>();
            kpis.VentasDia = estaciones.Sum(e => e.MontoTotalDia);
            kpis.VentasSemana = estaciones.Sum(e => e.MontoTotalSemana);
            kpis.VentasMes = estaciones.Sum(e => e.MontoTotalMes);
            kpis.VentasAnio = estaciones.Sum(e => e.MontoTotalAnio);

            // Información global del día: el legado descarta la categoría con Id == 1.
            var global = await _repository.ObtenerInformacionGlobalAsync(EnumTipoReporteGrafico.Dia, idEstacionFiltro);
            if (global.EsExitoso && global.Modelo is not null)
                kpis.InformacionGlobal = global.Modelo.Where(c => c.Id != 1).ToList();

            // Merma: [0] = mes actual, [1] = mes anterior (si existen).
            var merma = await _repository.ObtenerMermaAsync();
            if (merma.EsExitoso && merma.Modelo is not null)
            {
                var lista = merma.Modelo.ToList();
                kpis.MermaActual = lista.ElementAtOrDefault(0);
                kpis.MermaAnterior = lista.ElementAtOrDefault(1);
            }

            // Costo de producción: [0] = mes actual, [1] = mes anterior (si existen).
            var costo = await _repository.ObtenerCostoProduccionAsync();
            if (costo.EsExitoso && costo.Modelo is not null)
            {
                var lista = costo.Modelo.ToList();
                kpis.CostoProduccionActual = lista.ElementAtOrDefault(0);
                kpis.CostoProduccionAnterior = lista.ElementAtOrDefault(1);
            }

            return new Notificacion<DashboardKpis>
            {
                Estatus = 200,
                Mensaje = "OK",
                Modelo = kpis
            };
        }

        public async Task<Notificacion<VentasPorFecha>> ObtenerVentasPorFechaAsync(
            EnumTipoReporteGrafico tipoReporte, int idRol, int idEstacionToken, DateTime? fechaConsulta = null)
        {
            int idEstacionFiltro = ResolverEstacion(idRol, idEstacionToken);
            var categorias = await _repository.ObtenerVentasPorFechaAsync(tipoReporte, idEstacionFiltro, fechaConsulta);

            var notificacion = new Notificacion<VentasPorFecha>
            {
                Estatus = categorias.Estatus,
                Mensaje = categorias.Mensaje
            };

            if (categorias.EsExitoso)
            {
                notificacion.Modelo = new VentasPorFecha
                {
                    Categorias = categorias.Modelo?.ToList() ?? new List<Categoria>()
                };
            }

            return notificacion;
        }

        public Task<Notificacion<IEnumerable<EstacionVenta>>> ObtenerVentasPorEstacionAsync(
            DateTime? fechaIni, DateTime? fechaFin, int idRol, int idEstacionToken)
            => _repository.ObtenerVentasPorEstacionAsync(
                fechaIni, fechaFin, ResolverEstacion(idRol, idEstacionToken));

        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerTopTenAsync(
            EnumTipoReporteGrafico tipoReporte, EnumTipoGrafico tipoGrafico, int idRol, int idEstacionToken)
            => _repository.ObtenerTopTenAsync(
                tipoReporte, tipoGrafico, ResolverEstacion(idRol, idEstacionToken));

        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerInformacionGlobalAsync(
            EnumTipoReporteGrafico tipoReporte, int idRol, int idEstacionToken)
            => _repository.ObtenerInformacionGlobalAsync(
                tipoReporte, ResolverEstacion(idRol, idEstacionToken));

        public Task<Notificacion<IEnumerable<MermaMensual>>> ObtenerMermaAsync()
            => _repository.ObtenerMermaAsync();

        public Task<Notificacion<IEnumerable<CostoProduccionMensual>>> ObtenerCostoProduccionAsync()
            => _repository.ObtenerCostoProduccionAsync();

        public Task<Notificacion<IEnumerable<Categoria>>> ObtenerIvaAcumuladoAsync(
            EnumTipoReporteGrafico tipoReporte, int idRol, int idEstacionToken)
            => _repository.ObtenerIvaAcumuladoAsync(
                tipoReporte, ResolverEstacion(idRol, idEstacionToken));
    }
}
