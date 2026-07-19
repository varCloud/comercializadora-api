using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ProduccionAgranel
{
    /// <summary>Reglas de negocio del módulo "Producción a granel".</summary>
    public interface IProduccionAgranelService
    {
        Task<RawPage<ProcesoProduccionAgranel>> ListarAsync(ProduccionAgranelQuery query);
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();
        Task<Notificacion<string>> AgregarAsync(AgregarProduccionAgranelRequest request, int idUsuario);
        Task<Notificacion<string>> AprobarAsync(AprobarProduccionAgranelRequest request, int idUsuario);
        Task<Notificacion<string>> AgregarEnvasadoAsync(AgregarEnvasadoLiquidosRequest request, int idUsuario);
    }
}
