using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ReportesCompras
{
    /// <summary>Reglas del reporte de Compras. Por ahora delega directo en el repositorio (sin lógica de negocio adicional).</summary>
    public interface IComprasReporteService
    {
        Task<RawPage<CompraReporteItem>> ListarAsync(ComprasReporteQuery filtros);

        Task<Notificacion<IEnumerable<CompraReporteItem>>> ExportarAsync(ComprasReporteQuery filtros);

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerProveedoresAsync();

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync();

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerCompradoresAsync();

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();
    }
}
