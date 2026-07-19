using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;

namespace comercializadora_api.Repositories.Bitacoras
{
    /// <summary>Contrato de datos del reporte "Bitácoras" (consulta de pedidos internos).</summary>
    public interface IBitacorasRepository
    {
        /// <summary>Listado paginado de pedidos internos (SP_V2_CONSULTA_PEDIDOS_INTERNOS).</summary>
        Task<RawPage<Bitacora>> ListarAsync(BitacorasQuery query);

        /// <summary>Timeline de estatus de un folio (SP_V2_CONSULTA_DETALLE_PEDIDOS_INTERNOS).</summary>
        Task<Notificacion<IEnumerable<BitacoraDetalle>>> ObtenerDetalleAsync(int idPedidoInterno);

        /// <summary>Catálogo de estatus de pedidos internos (SP_CONSULTA_ESTATUS_PEDIDOS_INTERNOS legado).</summary>
        Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync();
    }
}
