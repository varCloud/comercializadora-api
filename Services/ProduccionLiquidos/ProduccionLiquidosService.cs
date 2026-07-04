using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ProduccionLiquidos;

namespace comercializadora_api.Services.ProduccionLiquidos
{
    /// <summary>Implementación del reporte "Producción Líquidos". Ver <see cref="IProduccionLiquidosService"/>.</summary>
    public sealed class ProduccionLiquidosService : IProduccionLiquidosService
    {
        private readonly IProduccionLiquidosRepository _repository;

        public ProduccionLiquidosService(IProduccionLiquidosRepository repository) => _repository = repository;

        public Task<RawPage<CargaMercanciaLiquidos>> ListarAsync(ProduccionLiquidosQuery query)
            => _repository.ListarAsync(query);
    }
}
