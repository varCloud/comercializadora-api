using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.LimitesInventario
{
    /// <summary>Lógica del módulo Límites de Inventario.</summary>
    public interface ILimitesInventarioService
    {
        Task<RawPage<LimiteInventario>> ListarAsync(LimitesInventarioQuery query);
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen);
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync();
        Task<Notificacion<string>> GuardarAsync(GuardarLimiteRequest request, int idUsuario);
        Task<Notificacion<string>> GuardarMasivoAsync(GuardarLimitesMasivoRequest request, int idUsuario);
    }
}
