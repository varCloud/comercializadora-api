using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Facturas
{
    /// <summary>
    /// Repositorio de Facturación (ventas). Migra <c>FacturaDAO</c> del legado (solo lo que usa
    /// la pantalla "Facturas Ventas": consulta, detalle para reenvío, cancelación y estatus).
    /// La generación/timbrado de CFDI queda fuera de alcance.
    ///
    /// Gotcha de cabeceras (ver <c>dapper-mapeo-columnas.md</c>): los SP legados reusados aquí
    /// (<c>SP_FACTURACION_OBTENER_*</c>, <c>SP_OBTENER_CANCELACION_FACTURA</c>,
    /// <c>SP_FACTURAS_OBTENER_PATH_ARCHIVO</c>) devuelven la cabecera como <c>Estatus/Mensaje</c>
    /// (PascalCase, y con casing inconsistente entre sus propias ramas éxito/error) en vez del
    /// <c>status/mensaje</c> minúsculas que esperan los helpers <c>ConsultarAsync</c>/
    /// <c>ConsultarUnicoAsync</c> de <see cref="BaseRepository"/>. Por eso esos SP se leen a mano
    /// con <see cref="LeerCabecera"/> (diccionario case-insensitive), y solo el nuevo
    /// <c>SP_V2_CONSULTA_FACTURAS</c> (que sí emite <c>status/mensaje</c>) usa
    /// <c>ConsultarPaginaAsync</c>.
    /// </summary>
    public sealed class FacturaRepository : BaseRepository, IFacturaRepository
    {
        private const string SpListado = "SP_V2_CONSULTA_FACTURAS";
        private const string SpDetalleVenta = "SP_FACTURACION_OBTENER_DETALLE_VENTA";
        private const string SpDatosFactura = "SP_FACTURACION_OBTENER_DATOS_FACTURA";
        private const string SpObtenerCancelacion = "SP_OBTENER_CANCELACION_FACTURA";
        private const string SpInsertaCancelada = "SP_FACTURACION_INSERTA_FACTURA_CANCELADA";
        private const string SpPathArchivo = "SP_FACTURAS_OBTENER_PATH_ARCHIVO";
        private const string SpConfiguracionComprobante = "SP_FACTURACION_OBTENER_CONFIGURACION_COMPROBANTE";

        // Variantes de Pedidos Especiales (feature migracion_facturas_pedidos_esp). Ojo:
        // SP_FACTURACION_OBTENER_CANCELACION_FACTURA es la variante PE (verificado en BD);
        // la de ventas es SP_OBTENER_CANCELACION_FACTURA, sin prefijo FACTURACION_.
        private const string SpListadoPe = "SP_V2_CONSULTA_FACTURAS_PEDIDOS_ESPECIALES";
        private const string SpDetallePe = "SP_FACTURACION_OBTENER_DETALLE_PEDIDO_ESPECIAL";
        private const string SpDatosFacturaPe = "SP_FACTURACION_OBTENER_DATOS_FACTURA_PEDIDO_ESPECIAL";
        private const string SpObtenerCancelacionPe = "SP_FACTURACION_OBTENER_CANCELACION_FACTURA";
        private const string SpInsertaCanceladaPe = "SP_FACTURACION_INSERTA_FACTURA_CANCELADA_PEDIDOS_ESPECIALES";

        public FacturaRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<FacturaVenta>> ListarAsync(FacturasQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@idStatusFactura", query.IdStatusFactura);
            p.Add("@idUsuario", query.IdUsuario);
            p.Add("@fechaInicio", query.FechaInicio?.Date);
            p.Add("@fechaFin", query.FechaFin?.Date);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<FacturaVenta>(SpListado, p);
        }

        public Task<Notificacion<DetalleVentaFactura>> ObtenerDetalleVentaAsync(long idVenta)
            => ObtenerDetalleCoreAsync(SpDetalleVenta, "@idVenta", idVenta);

        public Task<Notificacion<DatosFacturaVenta>> ObtenerDatosFacturaAsync(long idVenta)
            => ObtenerDatosFacturaCoreAsync(SpDatosFactura, "@idVenta", idVenta);

        public Task<Notificacion<CancelacionFactura>> ObtenerCancelacionAsync(long idVenta)
            => ObtenerCancelacionCoreAsync(SpObtenerCancelacion, "@idVenta", (int)idVenta);

        public Task<Notificacion<string>> CancelarFacturaAsync(
            long idVenta, int idUsuario, int idEstatusFactura, string mensajeError)
            => CancelarFacturaCoreAsync(SpInsertaCancelada, "@idVenta", (int)idVenta, idUsuario, idEstatusFactura, mensajeError);

        // ── Variantes de Pedidos Especiales ──

        public Task<RawPage<FacturaPedidoEspecial>> ListarPedidosEspecialesAsync(FacturasQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@idStatusFactura", query.IdStatusFactura);
            p.Add("@idUsuario", query.IdUsuario);
            p.Add("@fechaInicio", query.FechaInicio?.Date);
            p.Add("@fechaFin", query.FechaFin?.Date);
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<FacturaPedidoEspecial>(SpListadoPe, p);
        }

        public Task<Notificacion<DetalleVentaFactura>> ObtenerDetallePedidoEspecialAsync(long idPedidoEspecial)
            => ObtenerDetalleCoreAsync(SpDetallePe, "@idPedidoEspecial", idPedidoEspecial);

        public Task<Notificacion<DatosFacturaVenta>> ObtenerDatosFacturaPedidoEspecialAsync(long idPedidoEspecial)
            => ObtenerDatosFacturaCoreAsync(SpDatosFacturaPe, "@idPedidoEspecial", idPedidoEspecial);

        public Task<Notificacion<CancelacionFactura>> ObtenerCancelacionPedidoEspecialAsync(long idPedidoEspecial)
            => ObtenerCancelacionCoreAsync(SpObtenerCancelacionPe, "@idPedidoEspecial", (int)idPedidoEspecial);

        public Task<Notificacion<string>> CancelarFacturaPedidoEspecialAsync(
            long idPedidoEspecial, int idUsuario, int idEstatusFactura, string mensajeError)
            => CancelarFacturaCoreAsync(SpInsertaCanceladaPe, "@idPedidoEspecial", (int)idPedidoEspecial, idUsuario, idEstatusFactura, mensajeError);

        // ── Núcleos compartidos ventas/PE (los SP son espejo, solo cambia el nombre del parámetro id) ──

        private async Task<Notificacion<DetalleVentaFactura>> ObtenerDetalleCoreAsync(
            string storedProcedure, string nombreParametroId, long id)
        {
            var p = new DynamicParameters();
            p.Add(nombreParametroId, id);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                storedProcedure, p, commandType: CommandType.StoredProcedure);

            var (estatus, mensaje) = LeerCabecera((object)await multi.ReadFirstAsync());
            var notificacion = new Notificacion<DetalleVentaFactura> { Estatus = estatus, Mensaje = mensaje };
            if (!notificacion.EsExitoso)
                return notificacion;

            var receptor = await multi.ReadFirstOrDefaultAsync<DetalleVentaFactura>();
            var conceptos = (await multi.ReadAsync<ConceptoVentaFactura>()).ToList();
            // Tercer resultset (conceptos con descripciones para la addenda) no se necesita en
            // esta vista resumida; se descarta con ReadAsync para no dejar el multi a medias.
            await multi.ReadAsync();

            if (receptor is not null)
            {
                receptor.Conceptos = conceptos;
                receptor.Total = conceptos.Sum(c => c.Importe);
            }

            notificacion.Modelo = receptor;
            return notificacion;
        }

        private async Task<Notificacion<DatosFacturaVenta>> ObtenerDatosFacturaCoreAsync(
            string storedProcedure, string nombreParametroId, long id)
        {
            var p = new DynamicParameters();
            p.Add(nombreParametroId, id);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                storedProcedure, p, commandType: CommandType.StoredProcedure);

            var (estatus, mensaje) = LeerCabecera((object)await multi.ReadFirstAsync());
            var notificacion = new Notificacion<DatosFacturaVenta> { Estatus = estatus, Mensaje = mensaje };
            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<DatosFacturaVenta>();

            return notificacion;
        }

        private async Task<Notificacion<CancelacionFactura>> ObtenerCancelacionCoreAsync(
            string storedProcedure, string nombreParametroId, int id)
        {
            var p = new DynamicParameters();
            p.Add(nombreParametroId, id);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                storedProcedure, p, commandType: CommandType.StoredProcedure);

            var (estatus, mensaje) = LeerCabecera((object)await multi.ReadFirstAsync());
            var notificacion = new Notificacion<CancelacionFactura> { Estatus = estatus, Mensaje = mensaje };
            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<CancelacionFactura>();

            return notificacion;
        }

        private async Task<Notificacion<string>> CancelarFacturaCoreAsync(
            string storedProcedure, string nombreParametroId, int id,
            int idUsuario, int idEstatusFactura, string mensajeError)
        {
            var p = new DynamicParameters();
            p.Add(nombreParametroId, id);
            p.Add("@idUsuario", idUsuario);
            p.Add("@idEstatusFactura", idEstatusFactura);
            p.Add("@msjError", mensajeError);

            using IDbConnection db = CreateConnection();
            var fila = await db.QueryFirstAsync(
                storedProcedure, p, commandType: CommandType.StoredProcedure);

            var (estatus, mensaje) = LeerCabecera((object)fila);
            return new Notificacion<string> { Estatus = estatus, Mensaje = mensaje };
        }

        public async Task<Notificacion<ArchivoFactura>> ObtenerPathArchivoAsync(long? idVenta, long? idPedidoEspecial)
        {
            var p = new DynamicParameters();
            p.Add("@idVenta", (int?)idVenta);
            p.Add("@idPedidoEspecial", (int?)idPedidoEspecial);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpPathArchivo, p, commandType: CommandType.StoredProcedure);

            var (estatus, mensaje) = LeerCabecera((object)await multi.ReadFirstAsync());
            var notificacion = new Notificacion<ArchivoFactura> { Estatus = estatus, Mensaje = mensaje };
            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<ArchivoFactura>();

            return notificacion;
        }

        public async Task<Notificacion<ConfiguracionComprobante>> ObtenerConfiguracionComprobanteAsync()
        {
            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpConfiguracionComprobante, commandType: CommandType.StoredProcedure);

            var (estatus, mensaje) = LeerCabecera((object)await multi.ReadFirstAsync());
            var notificacion = new Notificacion<ConfiguracionComprobante> { Estatus = estatus, Mensaje = mensaje };
            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<ConfiguracionComprobante>();

            return notificacion;
        }

        /// <summary>
        /// Lee la cabecera (Estatus/Mensaje, con casing inconsistente según el SP y la rama) de
        /// forma case-insensitive. Ver comentario de clase.
        /// </summary>
        private static (int Estatus, string? Mensaje) LeerCabecera(object fila)
        {
            var columnas = new Dictionary<string, object>(
                (IDictionary<string, object>)fila, StringComparer.OrdinalIgnoreCase);

            int estatus = Convert.ToInt32(columnas.TryGetValue("estatus", out var e) ? e : columnas["status"]);
            string? mensaje = columnas.TryGetValue("mensaje", out var m) ? m?.ToString() : null;
            return (estatus, mensaje);
        }
    }
}
