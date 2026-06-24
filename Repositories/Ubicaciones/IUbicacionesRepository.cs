using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Ubicaciones
{
    /// <summary>
    /// Catálogos para el generador de etiquetas QR de ubicaciones (módulo Productos).
    /// Migra ProductosDAO.ObtenerPisos/Pasillos/Racks (SP_CONSULTA_PASILLO_PISO_RAQ) y
    /// UsuarioDAO.ObtenerAlmacenes (SP_CONSULTA_ALMACENES) del legado.
    /// </summary>
    public interface IUbicacionesRepository
    {
        /// <summary>Almacenes de una sucursal (SP_CONSULTA_ALMACENES).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int idSucursal);

        /// <summary>Catálogo de pisos (SP_CONSULTA_PASILLO_PISO_RAQ, @caso = 1).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPisosAsync();

        /// <summary>Catálogo de pasillos (SP_CONSULTA_PASILLO_PISO_RAQ, @caso = 2).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPasillosAsync();

        /// <summary>Catálogo de racks (SP_CONSULTA_PASILLO_PISO_RAQ, @caso = 3).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRacksAsync();
    }
}
