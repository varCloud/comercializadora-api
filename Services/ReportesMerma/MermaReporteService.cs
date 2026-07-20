using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ReportesMerma;

namespace comercializadora_api.Services.ReportesMerma
{
    /// <summary>Implementación del reporte "Merma". Ver <see cref="IMermaReporteService"/>.</summary>
    public sealed class MermaReporteService : IMermaReporteService
    {
        private readonly IMermaReporteRepository _repository;

        public MermaReporteService(IMermaReporteRepository repository) => _repository = repository;

        public Task<RawPage<MermaItem>> ListarAsync(MermaQuery filtros)
            => _repository.ListarAsync(filtros);

        public Task<Notificacion<IEnumerable<MermaItem>>> ExportarAsync(MermaQuery filtros)
            => _repository.ExportarAsync(filtros);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAniosAsync()
            => _repository.ObtenerAniosAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMesesAsync(int? anio)
            => _repository.ObtenerMesesAsync(anio);
    }
}
