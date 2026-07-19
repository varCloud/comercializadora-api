using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.InventariosFisicos
{
    /// <summary>
    /// Contrato de negocio del módulo Inventario Físico. La carpeta/namespace va en plural
    /// (InventariosFisicos) para no colisionar con la entidad <see cref="InventarioFisico"/>.
    /// </summary>
    public interface IInventarioFisicoService
    {
        Task<RawPage<InventarioFisico>> ListarAsync(InventarioFisicoQuery query, int idSucursal);

        Task<Notificacion<string>> GuardarAsync(int idInventarioFisico, string nombre, int idUsuario);

        Task<Notificacion<string>> ActualizarEstatusAsync(
            int idInventarioFisico, int idEstatus, string? observaciones, int idUsuario);

        Task<Notificacion<IEnumerable<AjusteInventarioFisico>>> ObtenerAjustesAsync(
            int idInventarioFisico, AjustesInventarioQuery query);
    }
}
