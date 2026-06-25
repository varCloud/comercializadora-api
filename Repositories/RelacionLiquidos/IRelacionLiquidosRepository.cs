using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.RelacionLiquidos
{
    /// <summary>
    /// Acceso a datos del módulo "Relación Liquidos" (Repository + Dapper + Stored Procedures).
    /// Migra ProductosAgranelAEnvasarDAO del legado. El listado usa SP_V2_CONSULTA_COMBINACION_LIQUIDOS
    /// (paginado); el alta/edición y la baja lógica reutilizan los SP legados sin modificarlos
    /// (devuelven la cabecera en columna "estatus", por eso se leen a mano).
    /// </summary>
    public interface IRelacionLiquidosRepository
    {
        /// <summary>Listado paginado (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<RelacionLiquido>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene una relación por id (para precargar el formulario de edición).</summary>
        Task<Notificacion<RelacionLiquido>> ObtenerPorIdAsync(int idRelacionEnvasadoAgranel);

        /// <summary>Alta o edición (SP_AGREGA_ACTUALIZA_COMBINACION_PRODUCTOS_ENSAVDOS_A_AGRANEL).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarRelacionLiquidoRequest request);

        /// <summary>Baja lógica (SP_DESACTIVAR_COMBINACION_PRODUCTOS_ENVASADOS_A_AGRANEL).</summary>
        Task<Notificacion<string>> DesactivarAsync(int idRelacionEnvasadoAgranel);

        /// <summary>
        /// Catálogo de unidades de medida válidas para líquidos a granel (subconjunto L/K), vía el
        /// SP legado SP_OBTENER_UNIDADES_DE_MEDIDA_LIQUIDOS_AGRANEL. Proyecta a CatalogoItem.
        /// </summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ListarUnidadesMedidaAsync();
    }
}
