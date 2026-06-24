using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Proveedores
{
    /// <summary>Lógica de negocio de Proveedores. Migra ProveedoresController + ProveedorDAO.</summary>
    public interface IProveedoresService
    {
        Task<RawPage<Proveedor>> ListarAsync(PagedQuery query);
        Task<Notificacion<Proveedor>> ObtenerPorIdAsync(int idProveedor);
        Task<Notificacion<string>> GuardarAsync(GuardarProveedorRequest proveedor);
        Task<Notificacion<string>> CambiarEstatusAsync(int idProveedor, bool activo);
    }
}
