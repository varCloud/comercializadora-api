using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ProduccionAgranel
{
    /// <summary>Contrato de datos del módulo "Producción a granel".</summary>
    public interface IProduccionAgranelRepository
    {
        /// <summary>Listado paginado del proceso de producción (SP_V2_CONSULTA_PROCESO_PRODUCCION_AGRANEL).</summary>
        Task<RawPage<ProcesoProduccionAgranel>> ListarAsync(ProduccionAgranelQuery query);

        /// <summary>Catálogo de estatus del proceso (SP_CONSULTA_ESTATUS_PROCESO_PRODUCCION legado).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();

        /// <summary>Alta de producto MPL a producción (SP_APP_INVENTARIO_AGREGAR_PRODUCTO_PRODUCCION_AGRANEL).</summary>
        Task<Notificacion<string>> AgregarAsync(AgregarProduccionAgranelRequest request, int idUsuario);

        /// <summary>Aprobación/rechazo de renglones (SP_APP_APROBAR_PRODUCTOS_PRODCUCCION_AGRANEL).</summary>
        Task<Notificacion<string>> AprobarAsync(AprobarProduccionAgranelRequest request, int idUsuario);

        /// <summary>Registro de envasado de líquidos (SP_APP_AGREGAR_PRODUCTO_INVENTARIO_LIQUIDOS_ENVASADO).</summary>
        Task<Notificacion<string>> AgregarEnvasadoAsync(AgregarEnvasadoLiquidosRequest request, int idUsuario);
    }
}
