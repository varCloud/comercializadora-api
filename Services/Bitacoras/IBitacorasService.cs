using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Services.Bitacoras
{
    /// <summary>Reglas de negocio del reporte "Bitácoras" (consulta de pedidos internos).</summary>
    public interface IBitacorasService
    {
        Task<RawPage<Bitacora>> ListarAsync(BitacorasQuery query);
        Task<Notificacion<IEnumerable<BitacoraDetalle>>> ObtenerDetalleAsync(int idPedidoInterno);
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();
    }
}
