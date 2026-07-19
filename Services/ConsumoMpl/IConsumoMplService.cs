using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ConsumoMpl
{
    /// <summary>Lógica del reporte "Consumo de MPL" (Costo de Producción Agranel). 100% lectura.</summary>
    public interface IConsumoMplService
    {
        Task<RawPage<CostoProduccionAgranel>> ListarAsync(ConsumoMplQuery query);
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAniosAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMesesAsync(int? anio);
    }
}
