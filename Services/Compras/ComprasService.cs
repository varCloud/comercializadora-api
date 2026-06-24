using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Compras;

namespace comercializadora_api.Services.Compras
{
    /// <summary>Implementación de la lógica de Compras. Ver <see cref="IComprasService"/>.</summary>
    public sealed class ComprasService : IComprasService
    {
        private readonly IComprasRepository _repository;

        public ComprasService(IComprasRepository repository) => _repository = repository;

        public Task<RawPage<Compra>> ListarAsync(ComprasQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<Compra>> ObtenerPorIdAsync(int idCompra)
            => _repository.ObtenerPorIdAsync(idCompra);

        public Task<Notificacion<string>> GuardarAsync(GuardarCompraRequest compra, int idUsuario)
            => _repository.GuardarAsync(compra, idUsuario);

        public Task<Notificacion<string>> EliminarAsync(int idCompra)
            => _repository.EliminarAsync(idCompra);

        public Task<Notificacion<IEnumerable<EstatusCompra>>> ObtenerEstatusAsync()
            => _repository.ObtenerEstatusAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int idSucursal)
            => _repository.ObtenerAlmacenesAsync(idSucursal);
    }
}
