using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Ubicaciones
{
    /// <summary>
    /// Catálogos del generador de etiquetas QR de ubicaciones. Reutiliza SP legados sin tocarlos:
    /// SP_CONSULTA_ALMACENES (almacenes por sucursal) y SP_CONSULTA_PASILLO_PISO_RAQ (@caso 1/2/3).
    /// </summary>
    public sealed class UbicacionesRepository : BaseRepository, IUbicacionesRepository
    {
        public UbicacionesRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int idSucursal)
        {
            var p = new DynamicParameters();
            p.Add("@idSucursal", idSucursal <= 0 ? (int?)null : idSucursal);
            p.Add("@idTipoAlmacen", (int?)null);
            return ConsultarCatalogoAsync("SP_CONSULTA_ALMACENES", p, "idAlmacen");
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPisosAsync()
            => ConsultarPasilloPisoRaqAsync(1);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerPasillosAsync()
            => ConsultarPasilloPisoRaqAsync(2);

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRacksAsync()
            => ConsultarPasilloPisoRaqAsync(3);

        /// <summary>
        /// SP_CONSULTA_PASILLO_PISO_RAQ devuelve dos resultsets; los datos (id/descripcion) están en
        /// el segundo (el DAO legado hace DataReader.NextResult() antes de leer). Se descarta el
        /// primero y se proyecta el segundo a <see cref="CatalogoItem"/> (id -> Id, descripcion -> Descripcion).
        /// </summary>
        private async Task<Notificacion<IEnumerable<CatalogoItem>>> ConsultarPasilloPisoRaqAsync(int caso)
        {
            var p = new DynamicParameters();
            p.Add("@caso", caso);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_CONSULTA_PASILLO_PISO_RAQ", p, commandType: CommandType.StoredProcedure);

            await multi.ReadAsync();                                   // primer resultset (no usado por el legado)
            var items = (await multi.ReadAsync<CatalogoItem>()).ToList();

            return new Notificacion<IEnumerable<CatalogoItem>>
            {
                Estatus = 200,
                Mensaje = "OK",
                Modelo = items
            };
        }
    }
}
