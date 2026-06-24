using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.LineasProducto;

namespace comercializadora_api.Services.LineasProducto
{
    /// <summary>Implementación de la lógica de Líneas de producto. Ver <see cref="ILineasProductoService"/>.</summary>
    public sealed class LineasProductoService : ILineasProductoService
    {
        private readonly ILineasProductoRepository _repository;

        public LineasProductoService(ILineasProductoRepository repository) => _repository = repository;

        public Task<RawPage<LineaProducto>> ListarAsync(PagedQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<LineaProducto>> ObtenerPorIdAsync(int idLineaProducto)
            => _repository.ObtenerPorIdAsync(idLineaProducto);

        public Task<Notificacion<string>> GuardarAsync(GuardarLineaProductoRequest linea)
            => _repository.GuardarAsync(linea);

        public Task<Notificacion<string>> CambiarEstatusAsync(int idLineaProducto, bool activo)
            => _repository.CambiarEstatusAsync(idLineaProducto, activo);
    }
}
