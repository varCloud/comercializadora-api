using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ConsumoMpl;

namespace comercializadora_api.Services.ConsumoMpl
{
    /// <summary>Implementación del reporte "Consumo de MPL". Ver <see cref="IConsumoMplService"/>.</summary>
    public sealed class ConsumoMplService : IConsumoMplService
    {
        private readonly IConsumoMplRepository _repository;

        public ConsumoMplService(IConsumoMplRepository repository) => _repository = repository;

        public Task<RawPage<CostoProduccionAgranel>> ListarAsync(ConsumoMplQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAniosAsync()
            => _repository.ObtenerAniosAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMesesAsync(int? anio)
            => _repository.ObtenerMesesAsync(anio);
    }
}
