using System.Data;
using Dapper;
using comercializadora_api.Models.Common;

namespace comercializadora_api.Repositories.Base
{
    /// <summary>
    /// Base para los repositorios. Encapsula la ejecución de stored procedures con Dapper.
    /// Equivalente moderno y asíncrono del ConstructorDapper del backend legado.
    /// </summary>
    public abstract class BaseRepository
    {
        private readonly IDbConnectionFactory _factory;

        protected BaseRepository(IDbConnectionFactory factory) => _factory = factory;

        /// <summary>
        /// Crea una conexión para casos que requieren leer varios resultsets a medida
        /// (p. ej. logins). Úsalo solo dentro del repositorio.
        /// </summary>
        protected IDbConnection CreateConnection() => _factory.CreateConnection();

        /// <summary>
        /// Ejecuta un SP que devuelve un resultset de cabecera (status, mensaje) seguido
        /// de un resultset de datos. Mapea a <see cref="Notificacion{T}"/>.
        /// </summary>
        protected async Task<Notificacion<IEnumerable<T>>> ConsultarAsync<T>(
            string storedProcedure, object? parametros = null)
        {
            using IDbConnection db = _factory.CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<T>>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadAsync<T>();

            return notificacion;
        }

        /// <summary>
        /// Ejecuta un SP que devuelve una sola fila con status, mensaje (insert/update/delete).
        /// </summary>
        protected async Task<Notificacion<string>> EjecutarAsync(
            string storedProcedure, object? parametros = null)
        {
            using IDbConnection db = _factory.CreateConnection();
            var fila = await db.QueryFirstAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            return new Notificacion<string>
            {
                Estatus = (int)fila.status,
                Mensaje = (string?)fila.mensaje
            };
        }

        /// <summary>
        /// Ejecuta un SP que devuelve status, mensaje y una entidad escalar/única.
        /// </summary>
        protected async Task<Notificacion<T>> ConsultarUnicoAsync<T>(
            string storedProcedure, object? parametros = null)
        {
            using IDbConnection db = _factory.CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<T>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<T>();

            return notificacion;
        }
    }
}
