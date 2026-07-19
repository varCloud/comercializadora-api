using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ReportesInventario;

namespace comercializadora_api.Services.ReportesInventario
{
    /// <inheritdoc cref="IInventarioReporteService" />
    public sealed class InventarioReporteService : IInventarioReporteService
    {
        private readonly IInventarioReporteRepository _repository;

        public InventarioReporteService(IInventarioReporteRepository repository) => _repository = repository;

        public Task<RawPage<InventarioReporteItem>> ListarAsync(InventarioReporteQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<IEnumerable<InventarioReporteExportItem>>> ExportarAsync(int tipo)
            => _repository.ExportarAsync(tipo);
    }
}
