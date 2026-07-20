using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.ReportesMerma
{
    /// <summary>Lógica del reporte "Merma". 100% lectura.</summary>
    public interface IMermaReporteService
    {
        Task<RawPage<MermaItem>> ListarAsync(MermaQuery filtros);
        Task<Notificacion<IEnumerable<MermaItem>>> ExportarAsync(MermaQuery filtros);
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAniosAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerMesesAsync(int? anio);
    }
}
