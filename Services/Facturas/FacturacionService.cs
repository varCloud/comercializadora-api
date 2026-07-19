using System.Globalization;
using System.Xml.Linq;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Enums;
using comercializadora_api.Repositories.Facturas;
using comercializadora_api.Services.Email;
using comercializadora_api.Services.FacturacionPac;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Services.Facturas
{
    /// <summary>
    /// Lógica de negocio de Facturación (ventas). Migra <c>FacturaController</c> (consulta,
    /// detalle, reenvío, cancelación) y <c>WsFacturaController.ActualizaEstatusCancelacionFactura</c>
    /// del legado. La generación/timbrado de CFDI queda fuera de alcance.
    /// </summary>
    public sealed class FacturacionService : IFacturacionService
    {
        private readonly IFacturaRepository _repository;
        private readonly IEmailService _emailService;
        private readonly ICfdiXmlSignerService _firmador;
        private readonly IFacturacionPacClient _pacClient;
        private readonly FacturacionArchivosOptions _opciones;

        public FacturacionService(
            IFacturaRepository repository,
            IEmailService emailService,
            ICfdiXmlSignerService firmador,
            IFacturacionPacClient pacClient,
            IOptions<FacturacionArchivosOptions> opciones)
        {
            _repository = repository;
            _emailService = emailService;
            _firmador = firmador;
            _pacClient = pacClient;
            _opciones = opciones.Value;
        }

        public async Task<RawPage<FacturaVenta>> ListarAsync(FacturasQuery query)
        {
            var pagina = await _repository.ListarAsync(query);
            foreach (var factura in pagina.Items)
                factura.PathArchivoFactura = AplicarUrlDominio(factura.PathArchivoFactura);
            return pagina;
        }

        public Task<Notificacion<DetalleVentaFactura>> ObtenerDetalleVentaAsync(long idVenta)
            => _repository.ObtenerDetalleVentaAsync(idVenta);

        public Task<Notificacion<string>> ReenviarAsync(ReenviarFacturaRequest request)
            => ReenviarCoreAsync(
                _repository.ObtenerDatosFacturaAsync(request.IdVenta),
                request.CorreoCopia,
                etiquetaFolio: $"Venta: {request.IdVenta}",
                sinFactura: "La venta no tiene una factura generada para reenviar.");

        public async Task<Notificacion<string>> CancelarAsync(CancelarFacturaRequest request, int idUsuario)
            => await CancelarCoreAsync(
                _repository.ObtenerCancelacionAsync(request.IdVenta),
                estatus => _repository.CancelarFacturaAsync(
                    request.IdVenta, idUsuario, estatus, "En proceso de cancelacion."));

        public async Task<Notificacion<AcuseEstatusCfdi>> ConsultarEstatusCancelacionAsync(EstatusCancelacionRequest request, int idUsuario)
        {
            bool esPe = request.EsPedidoEspecial;

            var archivo = await _repository.ObtenerPathArchivoAsync(
                esPe ? null : request.Id, esPe ? request.Id : null);
            if (!archivo.EsExitoso || archivo.Modelo?.PathArchivoFactura is null || archivo.Modelo.Uuid is null)
                return new Notificacion<AcuseEstatusCfdi>
                {
                    Estatus = -1,
                    Mensaje = archivo.Mensaje ?? "No se encontró el archivo/UUID de la factura."
                };

            var datosFactura = esPe
                ? await _repository.ObtenerDatosFacturaPedidoEspecialAsync(request.Id)
                : await _repository.ObtenerDatosFacturaAsync(request.Id);
            if (!datosFactura.EsExitoso || datosFactura.Modelo?.Rfc is null)
                return new Notificacion<AcuseEstatusCfdi> { Estatus = -1, Mensaje = datosFactura.Mensaje ?? "No se encontró el RFC del receptor." };

            var configuracion = await _repository.ObtenerConfiguracionComprobanteAsync();
            if (!configuracion.EsExitoso || configuracion.Modelo?.Rfc is null)
                return new Notificacion<AcuseEstatusCfdi> { Estatus = -1, Mensaje = configuracion.Mensaje ?? "No se encontró el RFC del emisor." };

            if (string.IsNullOrWhiteSpace(_opciones.RutaBaseArchivos))
                return new Notificacion<AcuseEstatusCfdi>
                {
                    Estatus = -1,
                    Mensaje = "Falta configurar FacturacionArchivos:RutaBaseArchivos para leer el comprobante timbrado."
                };

            string rutaPdf = ResolverRutaLocal(_opciones.RutaBaseArchivos, archivo.Modelo.PathArchivoFactura);
            string rutaXml = rutaPdf.Replace("Factura_", "Timbre_").Replace(".pdf", ".xml");
            if (!File.Exists(rutaXml))
                return new Notificacion<AcuseEstatusCfdi> { Estatus = -1, Mensaje = $"No se encontró el comprobante timbrado local ({rutaXml})." };

            string? sello = XDocument.Load(rutaXml).Root?.Attribute("Sello")?.Value;
            if (string.IsNullOrWhiteSpace(sello))
                return new Notificacion<AcuseEstatusCfdi> { Estatus = -1, Mensaje = "El comprobante timbrado local no tiene Sello." };

            string ultimosSello = sello.Length <= 8 ? sello : sello[^8..];
            string expresionImpresa =
                $"&id={archivo.Modelo.Uuid}" +
                $"&re={configuracion.Modelo.Rfc}" +
                $"&rr={datosFactura.Modelo.Rfc}" +
                $"&tt={datosFactura.Modelo.MontoTotal.ToString("#.##", CultureInfo.InvariantCulture)}" +
                $"&fe={ultimosSello}";

            AcuseEstatusCfdi acuse;
            try
            {
                acuse = await _pacClient.ConsultarEstatusAsync(expresionImpresa);
            }
            catch (Exception ex)
            {
                return new Notificacion<AcuseEstatusCfdi> { Estatus = -1, Mensaje = $"Error al consultar el estatus ante el SAT: {ex.Message}" };
            }

            if (string.Equals(acuse.Estado, "Cancelado", StringComparison.OrdinalIgnoreCase))
            {
                string mensajeCancelado = $"Cancelado correctamente | {acuse.Estado}";
                if (esPe)
                    await _repository.CancelarFacturaPedidoEspecialAsync(
                        request.Id, idUsuario, (int)EstatusFactura.Cancelada, mensajeCancelado);
                else
                    await _repository.CancelarFacturaAsync(
                        request.Id, idUsuario, (int)EstatusFactura.Cancelada, mensajeCancelado);
            }

            return new Notificacion<AcuseEstatusCfdi> { Estatus = 200, Mensaje = "OK", Modelo = acuse };
        }

        // ── Variantes de Pedidos Especiales ──

        public async Task<RawPage<FacturaPedidoEspecial>> ListarPedidosEspecialesAsync(FacturasQuery query)
        {
            var pagina = await _repository.ListarPedidosEspecialesAsync(query);
            foreach (var factura in pagina.Items)
                factura.PathArchivoFactura = AplicarUrlDominio(factura.PathArchivoFactura);
            return pagina;
        }

        public Task<Notificacion<DetalleVentaFactura>> ObtenerDetallePedidoEspecialAsync(long idPedidoEspecial)
            => _repository.ObtenerDetallePedidoEspecialAsync(idPedidoEspecial);

        public Task<Notificacion<string>> ReenviarPedidoEspecialAsync(ReenviarFacturaPeRequest request)
            => ReenviarCoreAsync(
                _repository.ObtenerDatosFacturaPedidoEspecialAsync(request.IdPedidoEspecial),
                request.CorreoCopia,
                etiquetaFolio: $"Pedido especial: PE{request.IdPedidoEspecial}",
                sinFactura: "El pedido especial no tiene una factura generada para reenviar.");

        public async Task<Notificacion<string>> CancelarPedidoEspecialAsync(CancelarFacturaPeRequest request, int idUsuario)
            => await CancelarCoreAsync(
                _repository.ObtenerCancelacionPedidoEspecialAsync(request.IdPedidoEspecial),
                estatus => _repository.CancelarFacturaPedidoEspecialAsync(
                    request.IdPedidoEspecial, idUsuario, estatus, "En proceso de cancelacion."));

        // ── Núcleos compartidos ventas/PE ──

        /// <summary>
        /// Flujo común de reenvío: valida correo/factura/config, lee PDF+XML del servidor de
        /// archivos y envía con copia oculta configurable. Solo cambian el origen de los datos
        /// (SP de ventas o de PE) y los textos.
        /// </summary>
        private async Task<Notificacion<string>> ReenviarCoreAsync(
            Task<Notificacion<DatosFacturaVenta>> obtenerDatos,
            string? correoCopia,
            string etiquetaFolio,
            string sinFactura)
        {
            var datos = await obtenerDatos;
            if (!datos.EsExitoso || datos.Modelo is null)
                return new Notificacion<string> { Estatus = datos.Estatus <= 0 ? datos.Estatus : -1, Mensaje = datos.Mensaje };

            if (string.IsNullOrWhiteSpace(datos.Modelo.Correo))
                return new Notificacion<string> { Estatus = -1, Mensaje = "El cliente no tiene correo registrado." };

            if (string.IsNullOrWhiteSpace(datos.Modelo.PathArchivoFactura))
                return new Notificacion<string> { Estatus = -1, Mensaje = sinFactura };

            if (string.IsNullOrWhiteSpace(_opciones.RutaBaseArchivos))
                return new Notificacion<string>
                {
                    Estatus = -1,
                    Mensaje = "Falta configurar FacturacionArchivos:RutaBaseArchivos (carpeta donde viven los PDF/XML) para poder reenviar."
                };

            string rutaPdf = ResolverRutaLocal(_opciones.RutaBaseArchivos, datos.Modelo.PathArchivoFactura);
            string rutaXml = rutaPdf.Replace("Factura_", "Timbre_").Replace(".pdf", ".xml");

            if (!File.Exists(rutaPdf) || !File.Exists(rutaXml))
                return new Notificacion<string>
                {
                    Estatus = -1,
                    Mensaje = $"No se encontraron los archivos de la factura en el servidor ({rutaPdf})."
                };

            var adjuntos = new[]
            {
                new EmailAdjunto(Path.GetFileName(rutaPdf), await File.ReadAllBytesAsync(rutaPdf), "application/pdf"),
                new EmailAdjunto(Path.GetFileName(rutaXml), await File.ReadAllBytesAsync(rutaXml), "application/xml")
            };

            var copiaOculta = _opciones.ObtenerCorreosCopia();
            if (!string.IsNullOrWhiteSpace(correoCopia))
                copiaOculta = copiaOculta.Append(correoCopia);

            string cuerpo = "<p>Adjunto encontrará su factura (CFDI) en formato PDF y XML.</p>" +
                             $"<p>{etiquetaFolio} &middot; Total: {datos.Modelo.MontoTotal:C2}</p>" +
                             "<p>Comercializadora Lluvia</p>";

            await _emailService.EnviarAsync(
                datos.Modelo.Correo!, "Factura Comercializadora Lluvia", cuerpo, adjuntos, copiaOculta);

            return new Notificacion<string> { Estatus = 200, Mensaje = "Factura reenviada exitosamente" };
        }

        /// <summary>
        /// Flujo común de cancelación ante el PAC: arma el XML de cancelación, lo firma con el
        /// CSD, lo envía a enviaAcuseCancelacion y, si el acuse es 201, registra el estatus
        /// "Pendiente de cancelación" con el SP correspondiente (ventas o PE).
        /// </summary>
        private async Task<Notificacion<string>> CancelarCoreAsync(
            Task<Notificacion<CancelacionFactura>> obtenerCancelacion,
            Func<int, Task<Notificacion<string>>> registrarPendiente)
        {
            var cancelacion = await obtenerCancelacion;
            if (!cancelacion.EsExitoso || cancelacion.Modelo is null)
                return new Notificacion<string>
                {
                    Estatus = -1,
                    Mensaje = cancelacion.Mensaje ?? "Ocurrió un error al intentar cancelar la factura."
                };

            if (string.IsNullOrWhiteSpace(cancelacion.Modelo.Uuid) || string.IsNullOrWhiteSpace(cancelacion.Modelo.Rfc))
                return new Notificacion<string> { Estatus = -1, Mensaje = "La factura no tiene UUID/RFC emisor registrados para cancelar." };

            string xmlSinFirmar = CfdiCancelacionXml.Construir(cancelacion.Modelo.Rfc!, cancelacion.Modelo.Uuid!);

            int estatusUuid;
            try
            {
                string xmlFirmado = _firmador.FirmarCancelacion(xmlSinFirmar);
                estatusUuid = await _pacClient.EnviarAcuseCancelacionAsync(xmlFirmado);
            }
            catch (Exception ex)
            {
                return new Notificacion<string> { Estatus = -1, Mensaje = $"Ocurrió un error al cancelar la factura: {ex.Message}" };
            }

            return estatusUuid switch
            {
                201 => await registrarPendiente((int)EstatusFactura.PendienteDeCancelacion),
                202 => new Notificacion<string>
                {
                    Estatus = -1,
                    Mensaje = "Factura previamente enviada, espere un momento y consulte su estado en el módulo de facturas."
                },
                _ => new Notificacion<string>
                {
                    Estatus = -1,
                    Mensaje = $"Espere un momento y vuelva a intentarlo: [estatus]: {estatusUuid}"
                }
            };
        }

        private string? AplicarUrlDominio(string? pathArchivoFactura)
        {
            if (string.IsNullOrWhiteSpace(pathArchivoFactura)) return pathArchivoFactura;
            if (string.IsNullOrWhiteSpace(_opciones.UrlDominio)) return pathArchivoFactura;

            string relativo = pathArchivoFactura.StartsWith('/') ? pathArchivoFactura : "/" + pathArchivoFactura;
            return _opciones.UrlDominio.TrimEnd('/') + relativo;
        }

        private static string ResolverRutaLocal(string rutaBase, string pathRelativo)
        {
            string limpio = pathRelativo.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(rutaBase, limpio);
        }
    }
}
