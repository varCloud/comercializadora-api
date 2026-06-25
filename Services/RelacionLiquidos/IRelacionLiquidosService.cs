using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.RelacionLiquidos
{
    /// <summary>
    /// Lógica del módulo "Relación Liquidos": CRUD de combinaciones (granel → envasado → envase)
    /// y los catálogos de producto por tipo (granel | envasar | envase), que reutilizan el
    /// listado paginado de Productos filtrado por las líneas configuradas en el back.
    /// </summary>
    public interface IRelacionLiquidosService
    {
        Task<RawPage<RelacionLiquido>> ListarAsync(PagedQuery query);

        Task<Notificacion<RelacionLiquido>> ObtenerPorIdAsync(int idRelacionEnvasadoAgranel);

        Task<Notificacion<string>> GuardarAsync(GuardarRelacionLiquidoRequest request);

        Task<Notificacion<string>> DesactivarAsync(int idRelacionEnvasadoAgranel);

        /// <summary>Catálogo de unidades de medida para líquidos a granel (subconjunto L/K).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ListarUnidadesMedidaAsync();

        /// <summary>
        /// Página de productos para un selector, según el <paramref name="tipo"/> semántico
        /// (granel | envasar | envase). Reutiliza el listado de Productos filtrando por las líneas
        /// que correspondan. Si el tipo no es válido devuelve una página vacía.
        /// </summary>
        Task<RawPage<Producto>> ListarProductosAsync(string? tipo, PagedQuery query);
    }
}
