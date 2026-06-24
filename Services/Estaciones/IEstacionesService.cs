using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Estaciones
{
    /// <summary>
    /// Lógica de negocio de Estaciones. Migra EstacionesController + EstacionesDAO del legado.
    /// El idUsuario (alta) se recibe ya resuelto del JWT por el controller.
    /// </summary>
    public interface IEstacionesService
    {
        /// <summary>Listado paginado de estaciones (con búsqueda libre). Devuelve filas + total.</summary>
        Task<RawPage<Estacion>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene una estación por id (para precargar el formulario de edición).</summary>
        Task<Notificacion<Estacion>> ObtenerPorIdAsync(int idEstacion);

        /// <summary>Alta o edición.</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarEstacionRequest estacion, int idUsuario);

        /// <summary>Cambia el estatus (borrado lógico).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idEstacion, int idStatus);

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen);
    }
}
