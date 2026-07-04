using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ProduccionLiquidos
{
    /// <summary>Repositorio del reporte "Producción Líquidos" (solo lectura).</summary>
    public interface IProduccionLiquidosRepository
    {
        Task<RawPage<CargaMercanciaLiquidos>> ListarAsync(ProduccionLiquidosQuery query);
    }
}
