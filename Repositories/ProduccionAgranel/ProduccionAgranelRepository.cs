using System.Data;
using System.Globalization;
using System.Xml.Linq;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.ProduccionAgranel
{
    /// <summary>
    /// Repositorio del módulo "Producción a granel" (Repository + Dapper + Stored Procedures).
    /// Migra <c>ProcesoProduccionAgranelDAO</c> y las regiones de producción a granel/envasado
    /// de <c>InventarioDAO</c> del legado. El listado usa
    /// <c>SP_V2_CONSULTA_PROCESO_PRODUCCION_AGRANEL</c> (paginado, nuevo); los comandos y el
    /// catálogo de estatus reutilizan los SP legados sin modificarlos. Los SP <c>SP_APP_*</c>
    /// devuelven la cabecera en columnas <c>Estatus</c>/<c>Mensaje</c> (mayúscula inicial, y en
    /// error columnas extra ErrorNumber/ErrorMessage), por lo que se leen con un diccionario
    /// case-insensitive en <see cref="EjecutarAppAsync"/>.
    /// </summary>
    public sealed class ProduccionAgranelRepository : BaseRepository, IProduccionAgranelRepository
    {
        private const string SpListado  = "SP_V2_CONSULTA_PROCESO_PRODUCCION_AGRANEL";
        private const string SpEstatus  = "SP_CONSULTA_ESTATUS_PROCESO_PRODUCCION";
        private const string SpAgregar  = "SP_APP_INVENTARIO_AGREGAR_PRODUCTO_PRODUCCION_AGRANEL";
        private const string SpAprobar  = "SP_APP_APROBAR_PRODUCTOS_PRODCUCCION_AGRANEL"; // typo "PRODCUCCION" = nombre real en BD
        private const string SpEnvasado = "SP_APP_AGREGAR_PRODUCTO_INVENTARIO_LIQUIDOS_ENVASADO";

        public ProduccionAgranelRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<ProcesoProduccionAgranel>> ListarAsync(ProduccionAgranelQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idUsuario", query.IdUsuario);
            p.Add("@idEstatus", query.IdEstatus);
            p.Add("@idAlmacen", query.IdAlmacen);
            p.Add("@fechaIni", query.FechaIni);
            p.Add("@fechaFin", query.FechaFin);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<ProcesoProduccionAgranel>(SpListado, p);
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
        {
            // SP legado: cabecera (status, mensaje) + resultset (value, text) con los estatus
            // de CatEstatusProcesoAgranel con id > 1 (la UI antepone la opción "TODOS").
            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpEstatus, null, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<CatalogoItem>>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (!notificacion.EsExitoso)
                return notificacion;

            var filas = (await multi.ReadAsync())
                .Select(f => new Dictionary<string, object>(
                    (IDictionary<string, object>)f, StringComparer.OrdinalIgnoreCase));

            notificacion.Modelo = filas
                .Select(f => new CatalogoItem
                {
                    Id = Convert.ToInt32(f["value"]),
                    Descripcion = f.TryGetValue("text", out var t) ? t?.ToString() : null
                })
                .ToList();

            return notificacion;
        }

        public Task<Notificacion<string>> AgregarAsync(AgregarProduccionAgranelRequest request, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", request.IdProducto);
            p.Add("@cantidad", request.Cantidad);
            p.Add("@idUsuario", idUsuario);
            p.Add("@idAlmacen", request.IdAlmacen);
            return EjecutarAppAsync(SpAgregar, p);
        }

        public Task<Notificacion<string>> AprobarAsync(AprobarProduccionAgranelRequest request, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@xmlProductos", SerializarProductos(request.Productos));
            p.Add("@idUsuario", idUsuario);
            p.Add("@idAlmacen", request.IdAlmacen);
            return EjecutarAppAsync(SpAprobar, p);
        }

        public Task<Notificacion<string>> AgregarEnvasadoAsync(AgregarEnvasadoLiquidosRequest request, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", request.IdProducto);
            p.Add("@cantidad", request.Cantidad);
            p.Add("@idUsuario", idUsuario);
            p.Add("@idAlmacen", request.IdAlmacen);
            return EjecutarAppAsync(SpEnvasado, p);
        }

        /// <summary>
        /// Serializa los renglones al XML &lt;ArrayOfProductosProduccionAgranel&gt;
        /// &lt;ProductosProduccionAgranel&gt;&lt;idProcesoProduccionAgranel/&gt;…&lt;/…&gt; que
        /// consume SP_APP_APROBAR_PRODUCTOS_PRODCUCCION_AGRANEL (el legado lo generaba con
        /// XmlSerializer de List&lt;ProductosProduccionAgranel&gt;). El SP solo lee
        /// idProcesoProduccionAgranel/idProducto/idUbicacion/cantidadAtendida/observaciones; el
        /// estatus lo calcula él mismo. Números con punto decimal (InvariantCulture).
        /// </summary>
        private static string SerializarProductos(IEnumerable<AprobarProduccionAgranelItem> productos)
        {
            var raiz = new XElement("ArrayOfProductosProduccionAgranel",
                productos.Select(pr => new XElement("ProductosProduccionAgranel",
                    new XElement("idProcesoProduccionAgranel", pr.IdProcesoProduccionAgranel),
                    new XElement("idProducto", pr.IdProducto),
                    new XElement("idUbicacion", pr.IdUbicacion),
                    new XElement("cantidadAtendida", pr.CantidadAtendida.ToString(CultureInfo.InvariantCulture)),
                    new XElement("observaciones", pr.Observaciones ?? string.Empty))));

            return raiz.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Ejecuta un SP legado <c>SP_APP_*</c> cuya cabecera viene en columnas
        /// <c>Estatus</c>/<c>Mensaje</c> (no <c>status</c>/<c>mensaje</c>). Se lee la primera
        /// fila a un diccionario case-insensitive para no depender del casing exacto del SP.
        /// </summary>
        private async Task<Notificacion<string>> EjecutarAppAsync(string storedProcedure, object parametros)
        {
            using IDbConnection db = CreateConnection();
            var fila = await db.QueryFirstAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            var columnas = new Dictionary<string, object>(
                (IDictionary<string, object>)fila, StringComparer.OrdinalIgnoreCase);

            return new Notificacion<string>
            {
                Estatus = Convert.ToInt32(columnas["estatus"]),
                Mensaje = columnas.TryGetValue("mensaje", out var m) ? m?.ToString() : null
            };
        }
    }
}
