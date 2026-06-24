using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.LimitesInventario
{
    /// <summary>Acceso a datos del módulo Límites de Inventario (SP + Dapper).</summary>
    public interface ILimitesInventarioRepository
    {
        /// <summary>Página de límites filtrada por almacén/línea/estatus + búsqueda libre.</summary>
        Task<RawPage<LimiteInventario>> ListarAsync(LimitesInventarioQuery query);

        /// <summary>Catálogo de estatus de límite (1/2/3) para el filtro.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();

        /// <summary>Catálogo de almacenes (opcionalmente por sucursal/tipo).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen);

        /// <summary>Catálogo de líneas de producto.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync();

        /// <summary>Crea/actualiza el límite mín/máx de un producto en un almacén.</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarLimiteRequest request, int idUsuario);

        /// <summary>Carga masiva de límites (serializa la lista a XML para el SP masivo).</summary>
        Task<Notificacion<string>> GuardarMasivoAsync(IEnumerable<LimiteMasivoItem> limites, int idUsuario);
    }
}
