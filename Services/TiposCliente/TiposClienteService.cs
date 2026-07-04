using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.TiposCliente;

namespace comercializadora_api.Services.TiposCliente
{
    /// <summary>Implementación de la lógica de Tipos de cliente. Ver <see cref="ITiposClienteService"/>.</summary>
    public sealed class TiposClienteService : ITiposClienteService
    {
        private readonly ITiposClienteRepository _repository;

        public TiposClienteService(ITiposClienteRepository repository) => _repository = repository;

        public Task<RawPage<TipoCliente>> ListarAsync(PagedQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<TipoCliente>> ObtenerPorIdAsync(int idTipoCliente)
            => _repository.ObtenerPorIdAsync(idTipoCliente);

        public Task<Notificacion<string>> GuardarAsync(GuardarTipoClienteRequest tipoCliente)
            => _repository.GuardarAsync(tipoCliente);

        public Task<Notificacion<string>> CambiarEstatusAsync(int idTipoCliente, bool activo)
            => _repository.CambiarEstatusAsync(idTipoCliente, activo);
    }
}
