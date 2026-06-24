using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Ubicaciones;

namespace comercializadora_api.Services.Ubicaciones
{
    /// <summary>Catálogos del generador de etiquetas QR de ubicaciones. Ver <see cref="IUbicacionesRepository"/>.</summary>
    public interface IUbicacionesService
    {
        /// <summary>Almacenes de una sucursal.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int idSucursal);

        /// <summary>Catálogo de pisos.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPisosAsync();

        /// <summary>Catálogo de pasillos.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPasillosAsync();

        /// <summary>Catálogo de racks.</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRacksAsync();
    }
}
