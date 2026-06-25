using System.Data;
using System.Globalization;
using System.Xml.Linq;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Productos
{
    /// <summary>
    /// Repositorio del módulo de Productos (Repository + Dapper + Stored Procedures), Fase A.
    /// Migra ProductosDAO del legado. El listado usa SP_V2_CONSULTA_PRODUCTOS (paginado) y el
    /// alta/edición SP_V2_INSERTA_ACTUALIZA_PRODUCTOS (artículo/código de barras separados).
    /// Reutiliza sin modificar: SP_ACTUALIZA_STATUS_PRODUCTOS,
    /// SP_APP_CONSULTA_PRODUCTOS_POR_DESCRIPCION, SP_CONSULTA_PRODUCTOS_POR_CODIGO_BARRAS.
    /// </summary>
    public sealed class ProductosRepository : BaseRepository, IProductosRepository
    {
        public ProductosRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<Producto>> ListarAsync(ProductosQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", 0);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@idLineaProducto", query.IdLineaProducto);
            p.Add("@idLineasProducto", string.IsNullOrWhiteSpace(query.IdLineasProducto) ? null : query.IdLineasProducto.Trim());
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<Producto>("SP_V2_CONSULTA_PRODUCTOS", p);
        }

        public async Task<Notificacion<Producto>> ObtenerPorIdAsync(int idProducto)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", idProducto);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_PRODUCTOS", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Producto>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<Producto>();

            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarProductoRequest producto)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", producto.IdProducto);
            p.Add("@descripcion", producto.Descripcion);
            p.Add("@idUnidadMedida", producto.IdUnidadMedida);
            p.Add("@idLineaProducto", producto.IdLineaProducto);
            p.Add("@cantidadUnidadMedida", producto.CantidadUnidadMedida);
            p.Add("@codigoBarras", string.IsNullOrWhiteSpace(producto.CodigoBarras) ? null : producto.CodigoBarras.Trim());
            p.Add("@activo", producto.Activo);
            p.Add("@articulo", producto.Articulo);
            p.Add("@claveProdServ", producto.ClaveProdServ);
            p.Add("@idUnidadCompra", producto.IdUnidadCompra);
            p.Add("@cantidadUnidadCompra", producto.CantidadUnidadCompra);
            return EjecutarAsync("SP_V2_INSERTA_ACTUALIZA_PRODUCTOS", p);
        }

        public Task<Notificacion<string>> CambiarEstatusAsync(int idProducto, bool activo)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", idProducto);
            p.Add("@activo", activo);
            return EjecutarAsync("SP_ACTUALIZA_STATUS_PRODUCTOS", p);
        }

        public async Task<Notificacion<IEnumerable<Producto>>> BuscarPorDescripcionAsync(string descripcion)
        {
            // SP legado: cabecera con columna "estatus" (no "status") -> lectura manual.
            var p = new DynamicParameters();
            p.Add("@descripcion", descripcion ?? string.Empty);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_APP_CONSULTA_PRODUCTOS_POR_DESCRIPCION", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<Producto>>
            {
                Estatus = (int)cabecera.estatus,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadAsync<Producto>();

            return notificacion;
        }

        public async Task<Notificacion<Producto>> ObtenerPorCodigoAsync(string codigo)
        {
            // SP legado: cabecera con columna "estatus" (no "status") -> lectura manual.
            var p = new DynamicParameters();
            p.Add("@codigo", codigo ?? string.Empty);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_CONSULTA_PRODUCTOS_POR_CODIGO_BARRAS", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Producto>
            {
                Estatus = (int)cabecera.estatus,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<Producto>();

            return notificacion;
        }

        public async Task<Notificacion<IEnumerable<Producto>>> ObtenerPorLineaAsync(int idLineaProducto)
        {
            // Reutiliza SP_V2_CONSULTA_PRODUCTOS (catálogo puro con precios + código de barras),
            // filtrando por línea y sin paginar (pageSize máximo) para traer todos los activos.
            var p = new DynamicParameters();
            p.Add("@idProducto", 0);
            p.Add("@idLineaProducto", idLineaProducto);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", int.MaxValue);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_PRODUCTOS", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<Producto>>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadAsync<Producto>();

            return notificacion;
        }

        public Task<Notificacion<IEnumerable<ClaveSat>>> BuscarClavesSatAsync(string? q, int page, int perPage)
        {
            var p = new DynamicParameters();
            p.Add("@search", string.IsNullOrWhiteSpace(q) ? null : q.Trim());
            p.Add("@pageNumber", page < 1 ? 1 : page);
            p.Add("@pageSize", perPage < 1 ? 20 : perPage);
            return ConsultarAsync<ClaveSat>("SP_V2_CONSULTA_CLAVES_SAT", p);
        }

        public async Task<Notificacion<PreciosProducto>> ObtenerPreciosAsync(int idProducto)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", idProducto);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_PRECIOS_PRODUCTO", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<PreciosProducto>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (!notificacion.EsExitoso)
                return notificacion;

            var precios = await multi.ReadFirstOrDefaultAsync<PreciosProducto>() ?? new PreciosProducto();
            precios.Rangos = (await multi.ReadAsync<RangoPrecio>()).ToList();
            notificacion.Modelo = precios;
            return notificacion;
        }

        public Task<Notificacion<string>> GuardarPreciosAsync(int idProducto, GuardarPreciosRequest precios)
        {
            // La BD está en compatibility_level 120 (sin OPENJSON): los rangos se envían como XML,
            // igual que el SP legado SP_INSERTA_ACTUALIZA_RANGOS_PRECIOS, que se reutiliza tal cual.
            var xml = new XElement("rangos",
                precios.Rangos.Select(r => new XElement("rango",
                    new XElement("contador", 0),
                    new XElement("min", r.Min.ToString(CultureInfo.InvariantCulture)),
                    new XElement("max", r.Max.ToString(CultureInfo.InvariantCulture)),
                    new XElement("costo", r.Costo.ToString(CultureInfo.InvariantCulture)),
                    new XElement("porcUtilidad", r.PorcUtilidad.ToString(CultureInfo.InvariantCulture)))));

            var p = new DynamicParameters();
            p.Add("@xml", xml.ToString(SaveOptions.DisableFormatting));
            p.Add("@idProducto", idProducto);
            p.Add("@precioIndividual", precios.PrecioIndividual);
            p.Add("@precioMenudeo", precios.PrecioMenudeo);
            p.Add("@ultimoCostoCompra", precios.UltimoCostoCompra);
            p.Add("@porcUtilidadIndividual", precios.PorcUtilidadIndividual);
            p.Add("@porcUtilidadMayoreo", precios.PorcUtilidadMayoreo);
            return EjecutarAsync("SP_INSERTA_ACTUALIZA_RANGOS_PRECIOS", p);
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync()
            => ConsultarCatalogoTipoAsync("lineas");

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesMedidaAsync()
            => ConsultarCatalogoTipoAsync("unidades-medida");

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerUnidadesCompraAsync()
            => ConsultarCatalogoTipoAsync("unidades-compra");

        /// <summary>
        /// Catálogos de Productos vía SP_V2_CONSULTA_CATALOGOS_PRODUCTO (cabecera + datos
        /// id/descripcion, forma uniforme), por @tipo. Mapea directo a CatalogoItem.
        /// </summary>
        private Task<Notificacion<IEnumerable<CatalogoItem>>> ConsultarCatalogoTipoAsync(string tipo)
        {
            var p = new DynamicParameters();
            p.Add("@tipo", tipo);
            return ConsultarAsync<CatalogoItem>("SP_V2_CONSULTA_CATALOGOS_PRODUCTO", p);
        }
    }
}
