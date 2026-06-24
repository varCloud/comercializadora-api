using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Compras
{
    /// <summary>Contrato del servicio de Compras. Ver <see cref="ComprasService"/>.</summary>
    public interface IComprasService
    {
        Task<RawPage<Compra>> ListarAsync(ComprasQuery query);
        Task<Notificacion<Compra>> ObtenerPorIdAsync(int idCompra);
        Task<Notificacion<string>> GuardarAsync(GuardarCompraRequest compra, int idUsuario);
        Task<Notificacion<string>> EliminarAsync(int idCompra);
        Task<Notificacion<IEnumerable<EstatusCompra>>> ObtenerEstatusAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int idSucursal);
    }
}
