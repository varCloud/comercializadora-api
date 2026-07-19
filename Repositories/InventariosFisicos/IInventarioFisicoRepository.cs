using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.InventariosFisicos
{
    /// <summary>
    /// Contrato de datos del módulo Inventario Físico. La carpeta/namespace va en plural
    /// (InventariosFisicos) para no colisionar con la entidad <see cref="InventarioFisico"/>.
    /// </summary>
    public interface IInventarioFisicoRepository
    {
        /// <summary>Listado paginado (SP_V2_CONSULTA_INVENTARIO_FISICO). idSucursal = claim JWT (0 = todas).</summary>
        Task<RawPage<InventarioFisico>> ListarAsync(InventarioFisicoQuery query, int idSucursal);

        /// <summary>Alta (id = 0) o renombrado (id &gt; 0) — SP_INSERTA_ACTUALIZA_INVENTARIO_FISICO.</summary>
        Task<Notificacion<string>> GuardarAsync(int idInventarioFisico, string nombre, int idUsuario);

        /// <summary>Transición de estatus (2 iniciar / 3 finalizar / 4 cancelar) — SP_ACTUALIZA_ESTATUS_INVENTARIO_FISICO.</summary>
        Task<Notificacion<string>> ActualizarEstatusAsync(
            int idInventarioFisico, int idEstatus, string? observaciones, int idUsuario);

        /// <summary>Ajustes del inventario, lista completa sin paginar — SP_CONSULTA_AJUSTE_INVENTARIO.</summary>
        Task<Notificacion<IEnumerable<AjusteInventarioFisico>>> ObtenerAjustesAsync(
            int idInventarioFisico, AjustesInventarioQuery query);
    }
}
