using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.LineasProducto
{
    /// <summary>
    /// Acceso a datos del submenú "Líneas de producto". Usa los SP nuevos
    /// SP_V2_CONSULTA_LINEAS_PRODUCTO (paginado), SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO
    /// (unicidad de descripción) y SP_V2_ACTUALIZA_STATUS_LINEAS_PRODUCTO (bloquea baja con
    /// productos asociados). No reemplazan los SP legados.
    /// </summary>
    public interface ILineasProductoRepository
    {
        /// <summary>Listado paginado (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<LineaProducto>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene una línea por id (para precargar el formulario de edición).</summary>
        Task<Notificacion<LineaProducto>> ObtenerPorIdAsync(int idLineaProducto);

        /// <summary>Alta o edición (SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarLineaProductoRequest linea);

        /// <summary>Activa/desactiva (SP_V2_ACTUALIZA_STATUS_LINEAS_PRODUCTO; bloquea baja con productos).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idLineaProducto, bool activo);
    }
}
