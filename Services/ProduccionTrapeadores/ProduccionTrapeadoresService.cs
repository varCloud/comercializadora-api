using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ProduccionTrapeadores;

namespace comercializadora_api.Services.ProduccionTrapeadores
{
    /// <summary>Implementación del reporte "Producción Trapeadores". Ver <see cref="IProduccionTrapeadoresService"/>.</summary>
    public sealed class ProduccionTrapeadoresService : IProduccionTrapeadoresService
    {
        private readonly IProduccionTrapeadoresRepository _repository;

        public ProduccionTrapeadoresService(IProduccionTrapeadoresRepository repository) => _repository = repository;

        public Task<RawPage<CargaMercanciaTrapeadores>> ListarAsync(ProduccionTrapeadoresQuery query)
            => _repository.ListarAsync(query);
    }
}
