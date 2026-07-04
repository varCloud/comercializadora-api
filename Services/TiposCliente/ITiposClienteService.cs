using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.TiposCliente
{
    /// <summary>
    /// Lógica de negocio de Tipos de cliente (la pantalla "Descuentos" del legado).
    /// Migra ClientesController + ClienteDAO (parte descuentos/tipos).
    /// </summary>
    public interface ITiposClienteService
    {
        Task<RawPage<TipoCliente>> ListarAsync(PagedQuery query);
        Task<Notificacion<TipoCliente>> ObtenerPorIdAsync(int idTipoCliente);
        Task<Notificacion<string>> GuardarAsync(GuardarTipoClienteRequest tipoCliente);
        Task<Notificacion<string>> CambiarEstatusAsync(int idTipoCliente, bool activo);
    }
}
