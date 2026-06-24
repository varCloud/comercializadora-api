using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;

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

        /// <summary>
        /// Lee un SP de catálogo legado que devuelve UN solo resultset donde la primera fila
        /// trae status/mensaje junto con los datos (patrón del backend viejo). Proyecta a
        /// <see cref="CatalogoItem"/> usando la columna de id indicada y "descripcion".
        /// Compartido por los módulos que pueblan combos (Usuarios, Estaciones, …).
        /// </summary>
        protected async Task<Notificacion<IEnumerable<CatalogoItem>>> ConsultarCatalogoAsync(
            string storedProcedure, object? parametros, string idColumn)
        {
            using IDbConnection db = _factory.CreateConnection();
            var crudas = (await db.QueryAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure)).ToList();

            // Los nombres de columna del legado varían en mayúsculas (p. ej. Almacenes.Descripcion);
            // se normalizan a un diccionario case-insensitive para mapear sin sorpresas.
            var filas = crudas
                .Select(f => new Dictionary<string, object>(
                    (IDictionary<string, object>)f, StringComparer.OrdinalIgnoreCase))
                .ToList();

            var notificacion = new Notificacion<IEnumerable<CatalogoItem>>
            {
                Estatus = 200,
                Mensaje = "OK",
                Modelo = new List<CatalogoItem>()
            };

            if (filas.Count == 0)
                return notificacion;

            notificacion.Estatus = Convert.ToInt32(filas[0]["status"]);
            notificacion.Mensaje = filas[0].TryGetValue("mensaje", out var m) ? m?.ToString() : null;

            if (!notificacion.EsExitoso)
                return notificacion;

            notificacion.Modelo = filas
                .Select(f => new CatalogoItem
                {
                    Id = Convert.ToInt32(f[idColumn]),
                    Descripcion = f.TryGetValue("descripcion", out var d) ? d?.ToString() : null
                })
                .ToList();

            return notificacion;
        }

        /// <summary>
        /// Ejecuta un SP de listado paginado (convención SP_V2_*): primer resultset = cabecera
        /// (status, mensaje, total), segundo = filas de la página. Devuelve <see cref="RawPage{T}"/>
        /// (filas + total); el controller arma data/links/meta. Si el SP falla, página vacía.
        /// </summary>
        protected async Task<RawPage<T>> ConsultarPaginaAsync<T>(
            string storedProcedure, object? parametros)
        {
            using IDbConnection db = _factory.CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            if ((int)cabecera.status != 200)
                return RawPage<T>.Empty();

            var items = (await multi.ReadAsync<T>()).ToList();
            return new RawPage<T> { Items = items, Total = (int)cabecera.total };
        }
    }
}
