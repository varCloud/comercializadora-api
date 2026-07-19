using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ReportesVentas;

namespace comercializadora_api.Services.ReportesVentas
{
    /// <inheritdoc cref="IVentaReporteService" />
    public sealed class VentaReporteService : IVentaReporteService
    {
        private readonly IVentaReporteRepository _repository;

        public VentaReporteService(IVentaReporteRepository repository) => _repository = repository;

        public Task<RawPage<VentaReporteItem>> ListarAsync(VentaReporteQuery filtros)
            => _repository.ListarAsync(filtros);

        public Task<Notificacion<IEnumerable<VentaReporteItem>>> ExportarAsync(VentaReporteQuery filtros)
            => _repository.ExportarAsync(filtros);
    }
}
