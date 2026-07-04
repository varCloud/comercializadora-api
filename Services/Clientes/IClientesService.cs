using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Clientes
{
    /// <summary>Lógica de negocio de Clientes. Migra ClientesController + ClienteDAO (parte clientes).</summary>
    public interface IClientesService
    {
        Task<RawPage<Cliente>> ListarAsync(PagedQuery query);
        Task<Notificacion<Cliente>> ObtenerPorIdAsync(int idCliente);
        Task<Notificacion<string>> GuardarAsync(GuardarClienteRequest cliente);
        Task<Notificacion<string>> CambiarEstatusAsync(int idCliente, bool activo);
        Task<Notificacion<IEnumerable<TipoCliente>>> ObtenerTiposActivosAsync();
        Task<Notificacion<IEnumerable<RegimenFiscal>>> ObtenerRegimenesFiscalesAsync();
    }
}
