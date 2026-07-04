using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Clientes;

namespace comercializadora_api.Services.Clientes
{
    /// <summary>
    /// Implementación de la lógica de Clientes. Ver <see cref="IClientesService"/>.
    /// La normalización Title Case y el mapeo física/moral viven en el repositorio
    /// (paridad con el ClienteDAO legado); la validación condicional, en el DTO.
    /// </summary>
    public sealed class ClientesService : IClientesService
    {
        private readonly IClientesRepository _repository;

        public ClientesService(IClientesRepository repository) => _repository = repository;

        public Task<RawPage<Cliente>> ListarAsync(PagedQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<Cliente>> ObtenerPorIdAsync(int idCliente)
            => _repository.ObtenerPorIdAsync(idCliente);

        public Task<Notificacion<string>> GuardarAsync(GuardarClienteRequest cliente)
            => _repository.GuardarAsync(cliente);

        public Task<Notificacion<string>> CambiarEstatusAsync(int idCliente, bool activo)
            => _repository.CambiarEstatusAsync(idCliente, activo);

        public Task<Notificacion<IEnumerable<TipoCliente>>> ObtenerTiposActivosAsync()
            => _repository.ObtenerTiposActivosAsync();

        public Task<Notificacion<IEnumerable<RegimenFiscal>>> ObtenerRegimenesFiscalesAsync()
            => _repository.ObtenerRegimenesFiscalesAsync();
    }
}
