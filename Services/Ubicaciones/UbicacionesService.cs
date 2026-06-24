using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Ubicaciones;

namespace comercializadora_api.Services.Ubicaciones
{
    /// <summary>Implementación de los catálogos de ubicaciones. Ver <see cref="IUbicacionesService"/>.</summary>
    public sealed class UbicacionesService : IUbicacionesService
    {
        private readonly IUbicacionesRepository _repository;

        public UbicacionesService(IUbicacionesRepository repository) => _repository = repository;

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int idSucursal)
            => _repository.ObtenerAlmacenesAsync(idSucursal);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPisosAsync()
            => _repository.ObtenerPisosAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPasillosAsync()
            => _repository.ObtenerPasillosAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRacksAsync()
            => _repository.ObtenerRacksAsync();
    }
}
