using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.RelacionTrapeadores
{
    /// <summary>
    /// Acceso a datos del módulo "Relación Trapeadores" (Repository + Dapper + Stored Procedures).
    /// Migra ProduccionProductosDAO del legado. El listado usa
    /// SP_V2_CONSULTA_COMBINACION_PRODUCCION_PRODUCTOS (paginado); el alta/edición y la baja
    /// lógica reutilizan los SP legados sin modificarlos (devuelven la cabecera en columna
    /// "estatus", por eso se leen a mano).
    /// </summary>
    public interface IRelacionTrapeadoresRepository
    {
        /// <summary>Listado paginado (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<RelacionTrapeador>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene una relación por id (para precargar el formulario de edición).</summary>
        Task<Notificacion<RelacionTrapeador>> ObtenerPorIdAsync(int id);

        /// <summary>Alta o edición (SP_AGREGA_ACTUALIZA_COMBINACION_PRODUCCION_PRODUCTOS).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarRelacionTrapeadorRequest request);

        /// <summary>
        /// Baja lógica (SP_DESACTIVAR_COMBINACION_PRODUCTOS_PRODUCCION). ⚠️ Comportamiento legado
        /// EXACTO: el SP recibe <c>@idProductoProduccion</c> — NO el id propio de la relación —
        /// tal como hace el JS legado (<c>EvtProduccionProductos.js</c> → <c>EliminarRelacion(item.idProductoProduccion)</c>).
        /// Este método recibe YA el <c>idProductoProduccion</c> resuelto; la resolución
        /// id-de-relación → idProductoProduccion vive en <see cref="Services.RelacionTrapeadores.RelacionTrapeadoresService.DesactivarAsync"/>,
        /// para que el endpoint público siga usando el id propio de la relación.
        /// </summary>
        Task<Notificacion<string>> DesactivarAsync(int idProductoProduccion);

        /// <summary>
        /// Catálogo de unidades de medida para trapeadores, vía el SP legado
        /// SP_OBTENER_UNIDADES_DE_MEDIDA_TRAPEADORES. Proyecta a CatalogoItem.
        /// </summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ListarUnidadesMedidaAsync();
    }
}
