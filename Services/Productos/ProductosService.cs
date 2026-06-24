using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Productos;

namespace comercializadora_api.Services.Productos
{
    /// <summary>Implementación de la lógica de Productos. Ver <see cref="IProductosService"/>.</summary>
    public sealed class ProductosService : IProductosService
    {
        private readonly IProductosRepository _repository;

        public ProductosService(IProductosRepository repository) => _repository = repository;

        public Task<RawPage<Producto>> ListarAsync(ProductosQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<Producto>> ObtenerPorIdAsync(int idProducto)
            => _repository.ObtenerPorIdAsync(idProducto);

        public Task<Notificacion<string>> GuardarAsync(GuardarProductoRequest producto)
            => _repository.GuardarAsync(producto);

        public Task<Notificacion<string>> CambiarEstatusAsync(int idProducto, bool activo)
            => _repository.CambiarEstatusAsync(idProducto, activo);

        public Task<Notificacion<IEnumerable<Producto>>> BuscarPorDescripcionAsync(string descripcion)
            => _repository.BuscarPorDescripcionAsync(descripcion);

        public Task<Notificacion<Producto>> ObtenerPorCodigoAsync(string codigo)
            => _repository.ObtenerPorCodigoAsync(codigo);

        public Task<Notificacion<IEnumerable<Producto>>> ObtenerPorLineaAsync(int idLineaProducto)
            => _repository.ObtenerPorLineaAsync(idLineaProducto);

        public Task<Notificacion<IEnumerable<ClaveSat>>> BuscarClavesSatAsync(string? q, int page, int perPage)
            => _repository.BuscarClavesSatAsync(q, page, perPage);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync()
            => _repository.ObtenerLineasAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesMedidaAsync()
            => _repository.ObtenerUnidadesMedidaAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesCompraAsync()
            => _repository.ObtenerUnidadesCompraAsync();

        public Task<Notificacion<PreciosProducto>> ObtenerPreciosAsync(int idProducto)
            => _repository.ObtenerPreciosAsync(idProducto);

        public Task<Notificacion<string>> GuardarPreciosAsync(int idProducto, GuardarPreciosRequest precios)
            => _repository.GuardarPreciosAsync(idProducto, precios);
    }
}
