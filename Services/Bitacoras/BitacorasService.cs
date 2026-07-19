using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Bitacoras;

namespace comercializadora_api.Services.Bitacoras
{
    /// <summary>Implementación del reporte "Bitácoras". Ver <see cref="IBitacorasService"/>.</summary>
    public sealed class BitacorasService : IBitacorasService
    {
        private readonly IBitacorasRepository _repository;

        public BitacorasService(IBitacorasRepository repository) => _repository = repository;

        public Task<RawPage<Bitacora>> ListarAsync(BitacorasQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<IEnumerable<BitacoraDetalle>>> ObtenerDetalleAsync(int idPedidoInterno)
            => _repository.ObtenerDetalleAsync(idPedidoInterno);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
            => _repository.ObtenerEstatusAsync();
    }
}
