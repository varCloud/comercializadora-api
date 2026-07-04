using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ProduccionTrapeadores
{
    /// <summary>Repositorio del reporte "Producción Trapeadores" (solo lectura).</summary>
    public interface IProduccionTrapeadoresRepository
    {
        Task<RawPage<CargaMercanciaTrapeadores>> ListarAsync(ProduccionTrapeadoresQuery query);
    }
}
