using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Productos
{
    /// <summary>Lógica de negocio de Productos (Fase A). Migra ProductosController + ProductosDAO.</summary>
    public interface IProductosService
    {
        Task<RawPage<Producto>> ListarAsync(ProductosQuery query);
        Task<Notificacion<Producto>> ObtenerPorIdAsync(int idProducto);
        Task<Notificacion<string>> GuardarAsync(GuardarProductoRequest producto);
        Task<Notificacion<string>> CambiarEstatusAsync(int idProducto, bool activo);
        Task<Notificacion<IEnumerable<Producto>>> BuscarPorDescripcionAsync(string descripcion);
        Task<Notificacion<Producto>> ObtenerPorCodigoAsync(string codigo);
        Task<Notificacion<IEnumerable<Producto>>> ObtenerPorLineaAsync(int idLineaProducto);
        Task<Notificacion<IEnumerable<ClaveSat>>> BuscarClavesSatAsync(string? q, int page, int perPage);
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesMedidaAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesCompraAsync();
        Task<Notificacion<PreciosProducto>> ObtenerPreciosAsync(int idProducto);
        Task<Notificacion<string>> GuardarPreciosAsync(int idProducto, GuardarPreciosRequest precios);
    }
}
