using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.InventariosFisicos;

namespace comercializadora_api.Services.InventariosFisicos
{
    /// <summary>
    /// Servicio del módulo "Inventario Físico". Las reglas de negocio pesadas (unicidad del
    /// nombre, transiciones de estatus válidas, snapshot del inventario al iniciar y afectación
    /// de existencias al finalizar) viven dentro de los SP legados reutilizados; aquí solo se
    /// orquesta (mismo criterio que ProduccionAgranelService).
    /// </summary>
    public sealed class InventarioFisicoService : IInventarioFisicoService
    {
        private readonly IInventarioFisicoRepository _repository;

        public InventarioFisicoService(IInventarioFisicoRepository repository)
            => _repository = repository;

        public Task<RawPage<InventarioFisico>> ListarAsync(InventarioFisicoQuery query, int idSucursal)
            => _repository.ListarAsync(query, idSucursal);

        public Task<Notificacion<string>> GuardarAsync(int idInventarioFisico, string nombre, int idUsuario)
            => _repository.GuardarAsync(idInventarioFisico, nombre, idUsuario);

        public Task<Notificacion<string>> ActualizarEstatusAsync(
            int idInventarioFisico, int idEstatus, string? observaciones, int idUsuario)
            => _repository.ActualizarEstatusAsync(idInventarioFisico, idEstatus, observaciones, idUsuario);

        public Task<Notificacion<IEnumerable<AjusteInventarioFisico>>> ObtenerAjustesAsync(
            int idInventarioFisico, AjustesInventarioQuery query)
            => _repository.ObtenerAjustesAsync(idInventarioFisico, query);
    }
}
