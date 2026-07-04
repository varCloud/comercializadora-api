using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.RelacionTrapeadores
{
    /// <summary>
    /// Lógica del módulo "Relación Trapeadores": CRUD de combinaciones (Matra + bastón →
    /// trapeador producido). A diferencia de Líquidos, los 3 selectores de producto del front
    /// consumen directamente <c>GET /api/productos</c> (decisión de diseño: no hay wrapper por
    /// tipo aquí).
    /// </summary>
    public interface IRelacionTrapeadoresService
    {
        Task<RawPage<RelacionTrapeador>> ListarAsync(PagedQuery query);

        Task<Notificacion<RelacionTrapeador>> ObtenerPorIdAsync(int id);

        Task<Notificacion<string>> GuardarAsync(GuardarRelacionTrapeadorRequest request);

        /// <summary>
        /// Desactiva la relación identificada por su id propio. Internamente resuelve el
        /// <c>idProductoProduccion</c> asociado (el SP legado exige ese id, no el de la relación;
        /// ver <see cref="Repositories.RelacionTrapeadores.IRelacionTrapeadoresRepository.DesactivarAsync"/>).
        /// Si la relación no existe, devuelve una <see cref="Notificacion{T}"/> de error sin
        /// llamar al SP.
        /// </summary>
        Task<Notificacion<string>> DesactivarAsync(int id);

        /// <summary>Catálogo de unidades de medida para trapeadores.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ListarUnidadesMedidaAsync();
    }
}
