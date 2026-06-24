using System.Data;
using System.Globalization;
using System.Xml.Linq;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Compras
{
    /// <summary>
    /// Repositorio del módulo de Compras (Repository + Dapper + Stored Procedures). Migra
    /// ComprasDAO del legado. El listado usa SP_V2_CONSULTA_COMPRAS (paginado) y la lectura por
    /// id SP_V2_CONSULTA_COMPRA_DETALLE (cabecera + detalle limpios). Guardar/eliminar/estatus
    /// reutilizan los SP legados (SP_REGISTRA_COMPRA, SP_ELIMINA_COMPRA, SP_CONSULTA_ESTATUS_COMPRA)
    /// sin modificarlos; el detalle de productos se serializa al XML &lt;ArrayOfProducto&gt; que
    /// espera SP_REGISTRA_COMPRA.
    /// </summary>
    public sealed class ComprasRepository : BaseRepository, IComprasRepository
    {
        public ComprasRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<Compra>> ListarAsync(ComprasQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@idProveedor", query.IdProveedor);
            p.Add("@idStatusCompra", query.IdStatusCompra);
            p.Add("@idUsuario", query.IdUsuario);
            p.Add("@idAlmacen", (int?)null);
            p.Add("@fechaInicio", query.FechaInicio);
            p.Add("@fechaFin", query.FechaFin);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<Compra>("SP_V2_CONSULTA_COMPRAS", p);
        }

        public async Task<Notificacion<Compra>> ObtenerPorIdAsync(int idCompra)
        {
            var p = new DynamicParameters();
            p.Add("@idCompra", idCompra);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_COMPRA_DETALLE", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Compra>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (!notificacion.EsExitoso)
                return notificacion;

            var compra = await multi.ReadFirstOrDefaultAsync<Compra>();
            if (compra is not null)
                compra.ListProductos = (await multi.ReadAsync<CompraProducto>()).ToList();

            notificacion.Modelo = compra;
            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarCompraRequest compra, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idCompra", compra.IdCompra);
            p.Add("@idProveedor", compra.IdProveedor);
            p.Add("@idUsuario", idUsuario);
            p.Add("@idStatusCompra", compra.IdStatusCompra);
            p.Add("@productos", SerializarProductos(compra.Productos));
            p.Add("@observaciones", compra.Observaciones ?? string.Empty);
            p.Add("@idAlmacen", compra.IdAlmacen);
            return EjecutarAsync("SP_REGISTRA_COMPRA", p);
        }

        public Task<Notificacion<string>> EliminarAsync(int idCompra)
        {
            var p = new DynamicParameters();
            p.Add("@idCompra", idCompra);
            return EjecutarAsync("SP_ELIMINA_COMPRA", p);
        }

        public Task<Notificacion<IEnumerable<EstatusCompra>>> ObtenerEstatusAsync()
            => ConsultarAsync<EstatusCompra>("SP_CONSULTA_ESTATUS_COMPRA");

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int idSucursal)
        {
            var p = new DynamicParameters();
            p.Add("@idSucursal", idSucursal);
            p.Add("@idTipoAlmacen", (int?)null);
            return ConsultarCatalogoAsync("SP_CONSULTA_ALMACENES", p, "idAlmacen");
        }

        /// <summary>
        /// Serializa el detalle al XML &lt;ArrayOfProducto&gt;&lt;Producto&gt;&lt;idProducto/&gt;
        /// &lt;cantidad/&gt;&lt;precio/&gt;…&lt;/Producto&gt;&lt;/ArrayOfProducto&gt; que consume
        /// SP_REGISTRA_COMPRA (el legado lo generaba con XmlSerializer de List&lt;Producto&gt;).
        /// Los números van con punto decimal (InvariantCulture).
        /// </summary>
        private static string SerializarProductos(IEnumerable<CompraProductoRequest> productos)
        {
            var raiz = new XElement("ArrayOfProducto",
                productos.Select(pr => new XElement("Producto",
                    new XElement("idProducto", pr.IdProducto),
                    new XElement("cantidad", pr.Cantidad.ToString(CultureInfo.InvariantCulture)),
                    new XElement("precio", pr.Precio.ToString(CultureInfo.InvariantCulture)))));

            return raiz.ToString(SaveOptions.DisableFormatting);
        }
    }
}
