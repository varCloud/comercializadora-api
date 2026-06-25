using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Productos;
using comercializadora_api.Repositories.RelacionLiquidos;

namespace comercializadora_api.Services.RelacionLiquidos
{
    /// <summary>Implementación de la lógica de "Relación Liquidos". Ver <see cref="IRelacionLiquidosService"/>.</summary>
    public sealed class RelacionLiquidosService : IRelacionLiquidosService
    {
        private readonly IRelacionLiquidosRepository _repository;
        private readonly IProductosRepository _productosRepository;

        public RelacionLiquidosService(
            IRelacionLiquidosRepository repository,
            IProductosRepository productosRepository)
        {
            _repository = repository;
            _productosRepository = productosRepository;
        }

        public Task<RawPage<RelacionLiquido>> ListarAsync(PagedQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<RelacionLiquido>> ObtenerPorIdAsync(int idRelacionEnvasadoAgranel)
            => _repository.ObtenerPorIdAsync(idRelacionEnvasadoAgranel);

        public Task<Notificacion<string>> GuardarAsync(GuardarRelacionLiquidoRequest request)
            => _repository.GuardarAsync(request);

        public Task<Notificacion<string>> DesactivarAsync(int idRelacionEnvasadoAgranel)
            => _repository.DesactivarAsync(idRelacionEnvasadoAgranel);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ListarUnidadesMedidaAsync()
            => _repository.ListarUnidadesMedidaAsync();

        public Task<RawPage<Producto>> ListarProductosAsync(string? tipo, PagedQuery query)
        {
            var lineas = RelacionLiquidosLineas.CsvPorTipo(tipo);
            if (lineas is null)
                return Task.FromResult(RawPage<Producto>.Empty()); // tipo no reconocido

            var productosQuery = new ProductosQuery
            {
                Page = query.Page,
                PerPage = query.PerPage,
                Q = query.Q,
                Order = query.Order,
                Sort = query.Sort,
                IdLineasProducto = lineas
            };
            return _productosRepository.ListarAsync(productosQuery);
        }
    }
}
