using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ReportesDevolucion;

namespace comercializadora_api.Services.ReportesDevolucion
{
    /// <summary>Implementación del reporte "Devoluciones y Complementos". Ver <see cref="IDevolucionReporteService"/>.</summary>
    public sealed class DevolucionReporteService : IDevolucionReporteService
    {
        private readonly IDevolucionReporteRepository _repository;

        public DevolucionReporteService(IDevolucionReporteRepository repository) => _repository = repository;

        public Task<RawPage<DevolucionItem>> ListarAsync(DevolucionQuery filtros)
            => _repository.ListarAsync(filtros);

        public Task<Notificacion<IEnumerable<DevolucionItem>>> ExportarAsync(DevolucionQuery filtros)
            => _repository.ExportarAsync(filtros);
    }
}
