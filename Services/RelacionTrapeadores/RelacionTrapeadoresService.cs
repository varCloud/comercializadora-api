using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.RelacionTrapeadores;

namespace comercializadora_api.Services.RelacionTrapeadores
{
    /// <summary>Implementación de la lógica de "Relación Trapeadores". Ver <see cref="IRelacionTrapeadoresService"/>.</summary>
    public sealed class RelacionTrapeadoresService : IRelacionTrapeadoresService
    {
        private readonly IRelacionTrapeadoresRepository _repository;

        public RelacionTrapeadoresService(IRelacionTrapeadoresRepository repository)
        {
            _repository = repository;
        }

        public Task<RawPage<RelacionTrapeador>> ListarAsync(PagedQuery query)
            => _repository.ListarAsync(query);

        public Task<Notificacion<RelacionTrapeador>> ObtenerPorIdAsync(int id)
            => _repository.ObtenerPorIdAsync(id);

        public Task<Notificacion<string>> GuardarAsync(GuardarRelacionTrapeadorRequest request)
            => _repository.GuardarAsync(request);

        /// <summary>
        /// ⚠️ Comportamiento legado a preservar: SP_DESACTIVAR_COMBINACION_PRODUCTOS_PRODUCCION
        /// solo acepta <c>@idProductoProduccion</c> (NO el id propio de la relación), igual que
        /// el JS legado (EvtProduccionProductos.js → EliminarRelacion(item.idProductoProduccion)).
        /// Para mantener limpio el contrato público (DELETE por id de relación, como en
        /// Líquidos), primero resolvemos la relación para obtener su <c>IdProductoProduccion</c>
        /// y luego llamamos al repositorio con ese valor.
        /// </summary>
        public async Task<Notificacion<string>> DesactivarAsync(int id)
        {
            var relacion = await _repository.ObtenerPorIdAsync(id);
            if (!relacion.EsExitoso || relacion.Modelo is null)
            {
                return new Notificacion<string>
                {
                    Estatus = 404,
                    Mensaje = "No se encontró la relación solicitada."
                };
            }

            return await _repository.DesactivarAsync(relacion.Modelo.IdProductoProduccion);
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ListarUnidadesMedidaAsync()
            => _repository.ListarUnidadesMedidaAsync();
    }
}
