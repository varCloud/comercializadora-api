using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ProduccionLiquidos
{
    /// <summary>Lógica de negocio del reporte "Producción Líquidos" (solo lectura).</summary>
    public interface IProduccionLiquidosService
    {
        Task<RawPage<CargaMercanciaLiquidos>> ListarAsync(ProduccionLiquidosQuery query);
    }
}
