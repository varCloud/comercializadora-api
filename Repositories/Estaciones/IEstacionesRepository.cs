using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Estaciones
{
    /// <summary>
    /// Acceso a datos del módulo de Estaciones. Envuelve los stored procedures del legado
    /// (SP_CONSULTA_ESTACIONES, SP_INSERTA_ACTUALIZA_ESTACIONES, SP_ACTUALIZA_STATUS_ESTACIONES)
    /// sin modificarlos. Reutiliza los catálogos de sucursales/almacenes vía BaseRepository.
    /// </summary>
    public interface IEstacionesRepository
    {
        /// <summary>Listado paginado de estaciones (SP_V2_CONSULTA_ESTACIONES). Devuelve filas + total.</summary>
        Task<RawPage<Estacion>> ListarAsync(PagedQuery query);

        /// <summary>Obtiene una estación por id (SP_V2_CONSULTA_ESTACIONES con @idEstacion).</summary>
        Task<Notificacion<Estacion>> ObtenerPorIdAsync(int idEstacion);

        /// <summary>Alta o edición (SP_INSERTA_ACTUALIZA_ESTACIONES). idUsuario = usuario del JWT.</summary>
        Task<Notificacion<string>> GuardarAsync(GuardarEstacionRequest estacion, int idUsuario);

        /// <summary>Cambia el estatus (borrado lógico) (SP_ACTUALIZA_STATUS_ESTACIONES).</summary>
        Task<Notificacion<string>> CambiarEstatusAsync(int idEstacion, int idStatus);

        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync();
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen);
    }
}
