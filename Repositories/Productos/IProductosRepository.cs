using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Productos
{
    /// <summary>
    /// Acceso a datos del módulo de Productos (Fase A). Usa SP_V2_CONSULTA_PRODUCTOS (paginado)
    /// y SP_V2_INSERTA_ACTUALIZA_PRODUCTOS (artículo/código de barras separados); reutiliza los
    /// SP legados de estatus, búsqueda por descripción y por código de barras.
    /// </summary>
    public interface IProductosRepository
    {
        /// <summary>Listado paginado (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<Producto>> ListarAsync(ProductosQuery query);

        /// <summary>Obtiene un producto por id (para precargar el formulario de edición).</summary>
        Task<Notificacion<Producto>> ObtenerPorIdAsync(int idProducto);

        /// <summary>Alta o edición (SP_V2_INSERTA_ACTUALIZA_PRODUCTOS).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarProductoRequest producto);

        /// <summary>Activa/desactiva (SP_ACTUALIZA_STATUS_PRODUCTOS; bloquea baja con existencias).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idProducto, bool activo);

        /// <summary>Autocomplete por descripción para el modal de Compras.</summary>
        Task<Notificacion<IEnumerable<Producto>>> BuscarPorDescripcionAsync(string descripcion);

        /// <summary>Lectura exacta por código de barras (escaneo).</summary>
        Task<Notificacion<Producto>> ObtenerPorCodigoAsync(string codigo);

        /// <summary>Todos los productos activos de una línea (para el generador de códigos de barras).</summary>
        Task<Notificacion<IEnumerable<Producto>>> ObtenerPorLineaAsync(int idLineaProducto);

        /// <summary>Búsqueda servidor de claves SAT (FactCatClaveProdServicio, paginada).</summary>
        Task<Notificacion<IEnumerable<ClaveSat>>> BuscarClavesSatAsync(string? q, int page, int perPage);

        /// <summary>
        /// Catálogo de líneas de producto. Sin <paramref name="idAlmacen"/> (o 0) trae todas las
        /// líneas activas (SP_V2_CONSULTA_CATALOGOS_PRODUCTO); con <paramref name="idAlmacen"/>
        /// filtra a las líneas con existencia en ese almacén (SP_OBTENER_LINEAS_ALMACEN legado,
        /// reusado tal cual — paridad con <c>ConsultaLineaAlmacen</c> del legado, feature
        /// consumo_mpl).
        /// </summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync(int? idAlmacen = null);

        /// <summary>Catálogo de unidades de medida.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesMedidaAsync();

        /// <summary>Catálogo de unidades de compra.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesCompraAsync();

        /// <summary>Precios de un producto: base + rangos de mayoreo (SP_V2_CONSULTA_PRECIOS_PRODUCTO).</summary>
        Task<Notificacion<PreciosProducto>> ObtenerPreciosAsync(int idProducto);

        /// <summary>Guarda precios base + rangos (SP legado SP_INSERTA_ACTUALIZA_RANGOS_PRECIOS, vía XML).</summary>
        Task<Notificacion<string>> GuardarPreciosAsync(int idProducto, GuardarPreciosRequest precios);
    }
}
