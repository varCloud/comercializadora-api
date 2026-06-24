using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Estaciones
{
    /// <summary>
    /// Repositorio del módulo de Estaciones (Repository + Dapper + Stored Procedures).
    /// Migra EstacionesDAO del legado reutilizando los SP existentes sin modificarlos.
    /// </summary>
    public sealed class EstacionesRepository : BaseRepository, IEstacionesRepository
    {
        public EstacionesRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<Estacion>> ListarAsync(PagedQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idEstacion", 0);
            p.Add("@idAlmacen", 0);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<Estacion>("SP_V2_CONSULTA_ESTACIONES", p);
        }

        public async Task<Notificacion<Estacion>> ObtenerPorIdAsync(int idEstacion)
        {
            var p = new DynamicParameters();
            p.Add("@idEstacion", idEstacion);
            p.Add("@idAlmacen", 0);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_ESTACIONES", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Estacion>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<Estacion>();

            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarEstacionRequest estacion, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idEstacion", estacion.IdEstacion);
            p.Add("@idAlmacen", estacion.IdAlmacen);
            p.Add("@macAdress", estacion.MacAdress);
            p.Add("@nombre", estacion.Nombre);
            p.Add("@numero", estacion.Numero);
            p.Add("@configurado", estacion.Configurado);
            p.Add("@idUsuario", idUsuario);
            return EjecutarAsync("SP_INSERTA_ACTUALIZA_ESTACIONES", p);
        }

        public Task<Notificacion<string>> CambiarEstatusAsync(int idEstacion, int idStatus)
        {
            var p = new DynamicParameters();
            p.Add("@idEstacion", idEstacion);
            p.Add("@idStatus", idStatus);
            return EjecutarAsync("SP_ACTUALIZA_STATUS_ESTACIONES", p);
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursalesAsync()
            => ConsultarCatalogoAsync("SP_CONSULTA_SUCURSALES", null, "idSucursal");

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen)
        {
            var p = new DynamicParameters();
            p.Add("@idSucursal", idSucursal);
            p.Add("@idTipoAlmacen", idTipoAlmacen);
            return ConsultarCatalogoAsync("SP_CONSULTA_ALMACENES", p, "idAlmacen");
        }
    }
}
