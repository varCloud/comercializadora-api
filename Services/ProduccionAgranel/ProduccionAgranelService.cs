using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ProduccionAgranel;

namespace comercializadora_api.Services.ProduccionAgranel
{
    /// <summary>
    /// Servicio del módulo "Producción a granel". Las validaciones de negocio pesadas (línea
    /// MPL, inventario suficiente, cantidades atendidas vs. solicitadas, relación
    /// envasado↔granel) viven dentro de los SP legados reutilizados; aquí solo se orquesta.
    /// </summary>
    public sealed class ProduccionAgranelService : IProduccionAgranelService
    {
        private readonly IProduccionAgranelRepository _repository;

        public ProduccionAgranelService(IProduccionAgranelRepository repository)
            => _repository = repository;

        public Task<RawPage<ProcesoProduccionAgranel>> ListarAsync(ProduccionAgranelQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
            => _repository.ObtenerEstatusAsync();

        public Task<Notificacion<string>> AgregarAsync(AgregarProduccionAgranelRequest request, int idUsuario)
            => _repository.AgregarAsync(request, idUsuario);

        public Task<Notificacion<string>> AprobarAsync(AprobarProduccionAgranelRequest request, int idUsuario)
            => _repository.AprobarAsync(request, idUsuario);

        public Task<Notificacion<string>> AgregarEnvasadoAsync(AgregarEnvasadoLiquidosRequest request, int idUsuario)
            => _repository.AgregarEnvasadoAsync(request, idUsuario);
    }
}
