using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Clientes
{
    /// <summary>
    /// Acceso a datos del módulo de Clientes. Usa SP_V2_CONSULTA_CLIENTES (paginado + búsqueda
    /// + orden, resultset aplanado) para el listado/by-id y los SP legados sin modificar para
    /// guardar (SP_INSERTA_ACTUALIZA_CLIENTES), cambiar estatus (SP_ACTUALIZA_STATUS_CLIENTES)
    /// y los catálogos (SP_CONSULTA_TIPOS_CLIENTES, SP_FACTURACION_OBTENER_REGIMEN_FISCAL).
    /// </summary>
    public interface IClientesRepository
    {
        /// <summary>Listado paginado (solo filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<Cliente>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene un cliente por id (para precargar el formulario de edición).</summary>
        Task<Notificacion<Cliente>> ObtenerPorIdAsync(int idCliente);

        /// <summary>Alta o edición (SP_INSERTA_ACTUALIZA_CLIENTES; Title Case + mapeo física/moral).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarClienteRequest cliente);

        /// <summary>Activa/desactiva (SP_ACTUALIZA_STATUS_CLIENTES con booleano real — bug del legado corregido).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idCliente, bool activo);

        /// <summary>Catálogo de tipos de cliente activos para el dropdown del form.</summary>
        Task<Notificacion<IEnumerable<TipoCliente>>> ObtenerTiposActivosAsync();

        /// <summary>Catálogo read-only de regímenes fiscales (SP_FACTURACION_OBTENER_REGIMEN_FISCAL).</summary>
        Task<Notificacion<IEnumerable<RegimenFiscal>>> ObtenerRegimenesFiscalesAsync();
    }
}
