using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Estaciones;

namespace comercializadora_api.Services.Estaciones
{
    /// <summary>
    /// Implementación de la lógica de negocio de Estaciones. Ver <see cref="IEstacionesService"/>.
    /// </summary>
    public sealed class EstacionesService : IEstacionesService
    {
        private readonly IEstacionesRepository _repository;

        public EstacionesService(IEstacionesRepository repository) => _repository = repository;

        public Task<RawPage<Estacion>> ListarAsync(PagedQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<Estacion>> ObtenerPorIdAsync(int idEstacion)
            => _repository.ObtenerPorIdAsync(idEstacion);

        public Task<Notificacion<string>> GuardarAsync(GuardarEstacionRequest estacion, int idUsuario)
            => _repository.GuardarAsync(estacion, idUsuario);

        public Task<Notificacion<string>> CambiarEstatusAsync(int idEstacion, int idStatus)
            => _repository.CambiarEstatusAsync(idEstacion, idStatus);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync()
            => _repository.ObtenerSucursalesAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen)
            => _repository.ObtenerAlmacenesAsync(idSucursal, idTipoAlmacen);
    }
}
