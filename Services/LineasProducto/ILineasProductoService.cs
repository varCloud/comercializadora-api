using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.LineasProducto
{
    /// <summary>Lógica del submenú "Líneas de producto". Migra LineaProductoController + LineaProductoDAO.</summary>
    public interface ILineasProductoService
    {
        Task<RawPage<LineaProducto>> ListarAsync(PagedQuery query);
        Task<Notificacion<LineaProducto>> ObtenerPorIdAsync(int idLineaProducto);
        Task<Notificacion<string>> GuardarAsync(GuardarLineaProductoRequest linea);
        Task<Notificacion<string>> CambiarEstatusAsync(int idLineaProducto, bool activo);
    }
}
