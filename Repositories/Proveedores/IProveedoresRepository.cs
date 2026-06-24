using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Proveedores
{
    /// <summary>
    /// Acceso a datos del módulo de Proveedores. Usa SP_V2_CONSULTA_PROVEEDORES (paginado +
    /// búsqueda + orden) para el listado y los SP legados para guardar/cambiar estatus.
    /// </summary>
    public interface IProveedoresRepository
    {
        /// <summary>Listado paginado (devuelve solo filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<Proveedor>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene un proveedor por id.</summary>
        Task<Notificacion<Proveedor>> ObtenerPorIdAsync(int idProveedor);

        /// <summary>Alta o edición (SP_INSERTA_ACTUALIZA_PROVEEDORES).</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarProveedorRequest proveedor);

        /// <summary>Activa/desactiva (SP_ACTUALIZA_STATUS_PROVEEDOR).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idProveedor, bool activo);
    }
}
