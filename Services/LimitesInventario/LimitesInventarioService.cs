using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.LimitesInventario;

namespace comercializadora_api.Services.LimitesInventario
{
    /// <summary>Implementación de la lógica de Límites de Inventario. Ver <see cref="ILimitesInventarioService"/>.</summary>
    public sealed class LimitesInventarioService : ILimitesInventarioService
    {
        private readonly ILimitesInventarioRepository _repository;

        public LimitesInventarioService(ILimitesInventarioRepository repository) => _repository = repository;

        public Task<RawPage<LimiteInventario>> ListarAsync(LimitesInventarioQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
            => _repository.ObtenerEstatusAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen)
            => _repository.ObtenerAlmacenesAsync(idSucursal, idTipoAlmacen);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync()
            => _repository.ObtenerLineasAsync();

        public Task<Notificacion<string>> GuardarAsync(GuardarLimiteRequest request, int idUsuario)
            => _repository.GuardarAsync(request, idUsuario);

        public Task<Notificacion<string>> GuardarMasivoAsync(GuardarLimitesMasivoRequest request, int idUsuario)
            => _repository.GuardarMasivoAsync(request.Limites, idUsuario);
    }
}
