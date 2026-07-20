using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.ReportesCompras
{
    /// <summary>
    /// Acceso a datos del reporte de Compras. Migra <c>ReportesController.BuscarCompras</c> +
    /// <c>ComprasDAO.ObtenerCompras</c> del legado. El listado usa
    /// <c>SP_V2_CONSULTA_COMPRAS_REPORTE</c> (paginado server-side, nuevo); los catálogos
    /// reutilizan SP ya existentes en el repo (compartidos con Compras/Proveedores/Líneas/Usuarios).
    /// </summary>
    public interface IComprasReporteRepository
    {
        /// <summary>Listado paginado del reporte (filas + total; el controller arma data/links/meta).</summary>
        Task<RawPage<CompraReporteItem>> ListarAsync(ComprasReporteQuery filtros);

        /// <summary>Todas las filas que cumplen los filtros, sin paginar (usado por <c>/exportar</c>).</summary>
        Task<Notificacion<IEnumerable<CompraReporteItem>>> ExportarAsync(ComprasReporteQuery filtros);

        /// <summary>Catálogo de proveedores para el filtro (todos, sin paginar).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerProveedoresAsync();

        /// <summary>Catálogo de líneas de producto para el filtro (todas, sin paginar).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync();

        /// <summary>Catálogo de compradores (usuarios) para el filtro (todos, sin paginar).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerCompradoresAsync();

        /// <summary>Catálogo de estatus de compra para el filtro.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();
    }
}
