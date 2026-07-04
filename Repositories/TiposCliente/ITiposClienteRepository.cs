using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.TiposCliente
{
    /// <summary>
    /// Acceso a datos del catálogo Tipos de cliente (pantalla "Descuentos" del legado).
    /// Usa SP_V2_CONSULTA_TIPOS_CLIENTES (paginado + búsqueda + orden) para el listado y
    /// los SP legados sin modificar para guardar (SP_INSERTA_ACTUALIZA_TIPOS_CLIENTES) y
    /// cambiar estatus (SP_ACTUALIZA_STATUS_TIPOS_CLIENTES).
    /// </summary>
    public interface ITiposClienteRepository
    {
        /// <summary>Listado paginado (solo filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<TipoCliente>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene un tipo de cliente por id.</summary>
        Task<Notificacion<TipoCliente>> ObtenerPorIdAsync(int idTipoCliente);

        /// <summary>Alta o edición (SP_INSERTA_ACTUALIZA_TIPOS_CLIENTES).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarTipoClienteRequest tipoCliente);

        /// <summary>Activa/desactiva (SP_ACTUALIZA_STATUS_TIPOS_CLIENTES).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idTipoCliente, bool activo);
    }
}
