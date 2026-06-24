using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Proveedores;

namespace comercializadora_api.Services.Proveedores
{
    /// <summary>Implementación de la lógica de Proveedores. Ver <see cref="IProveedoresService"/>.</summary>
    public sealed class ProveedoresService : IProveedoresService
    {
        private readonly IProveedoresRepository _repository;

        public ProveedoresService(IProveedoresRepository repository) => _repository = repository;

        public Task<RawPage<Proveedor>> ListarAsync(PagedQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<Proveedor>> ObtenerPorIdAsync(int idProveedor)
            => _repository.ObtenerPorIdAsync(idProveedor);

        public Task<Notificacion<string>> GuardarAsync(GuardarProveedorRequest proveedor)
            => _repository.GuardarAsync(proveedor);

        public Task<Notificacion<string>> CambiarEstatusAsync(int idProveedor, bool activo)
            => _repository.CambiarEstatusAsync(idProveedor, activo);
    }
}
