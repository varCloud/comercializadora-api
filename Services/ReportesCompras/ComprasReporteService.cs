using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.ReportesCompras;

namespace comercializadora_api.Services.ReportesCompras
{
    /// <inheritdoc cref="IComprasReporteService" />
    public sealed class ComprasReporteService : IComprasReporteService
    {
        private readonly IComprasReporteRepository _repository;

        public ComprasReporteService(IComprasReporteRepository repository) => _repository = repository;

        public Task<RawPage<CompraReporteItem>> ListarAsync(ComprasReporteQuery filtros)
            => _repository.ListarAsync(filtros);

        public Task<Notificacion<IEnumerable<CompraReporteItem>>> ExportarAsync(ComprasReporteQuery filtros)
            => _repository.ExportarAsync(filtros);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerProveedoresAsync()
            => _repository.ObtenerProveedoresAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync()
            => _repository.ObtenerLineasAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerCompradoresAsync()
            => _repository.ObtenerCompradoresAsync();

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
            => _repository.ObtenerEstatusAsync();
    }
}
