# Módulo Facturación migrado (ventas `migracion_facturas_ventas` + PE `migracion_facturas_pedidos_esp`)

`api/facturas` `[Authorize]`: `GET /` (paginado), `GET /detalle/{idVenta}`, `POST /reenviar`,
`POST /cancelar`, `POST /estatus-cancelacion`. Consulta + reenvío + cancelación + estatus de
facturas de **venta**.

**Extensión Pedidos Especiales (2026-07-15, mismo módulo, sin controller nuevo):** rutas PE
propias `GET /pedidos-especiales` (paginado, `SP_V2_CONSULTA_FACTURAS_PEDIDOS_ESPECIALES`
nuevo), `GET /pedidos-especiales/detalle/{idPedidoEspecial}`, `POST /pedidos-especiales/reenviar`
(`{idPedidoEspecial, correoCopia?}`), `POST /pedidos-especiales/cancelar` (`{idPedidoEspecial}`);
`POST /estatus-cancelacion` ahora **sí soporta `esPedidoEspecial=true`** (`id`=idPedidoEspecial,
usa `SP_FACTURAS_OBTENER_PATH_ARCHIVO @idPedidoEspecial` + datos PE + inserta-cancelada PE).
Decisión de contrato: rutas PE dedicadas en vez de flag (requests tipados sin ambigüedad,
pantalla front independiente); la excepción es estatus-cancelacion que nació compartido.
SPs PE verificados en BD (firmas = DAO legado): `SP_FACTURACION_OBTENER_FACTURAS_PEDIDOS_ESPECIALES`
(reemplazado por el SP_V2 paginado; aquí el filtro `@idUsuario` legado SÍ funcionaba, a
diferencia de ventas), `SP_FACTURACION_OBTENER_DETALLE_PEDIDO_ESPECIAL`,
`SP_FACTURACION_OBTENER_DATOS_FACTURA_PEDIDO_ESPECIAL` (⚠️ sin guard de cliente incompleto, a
diferencia de su gemelo de ventas), `SP_FACTURACION_INSERTA_FACTURA_CANCELADA_PEDIDOS_ESPECIALES`
(⚠️ sin el IF EXISTS anti-doble-cancelación que sí tiene el de ventas — quirk del legado,
conservado), `SP_FACTURACION_OBTENER_CANCELACION_FACTURA` (la variante PE, confirmado de nuevo).
Refactor interno: repository con núcleos compartidos (`ObtenerDetalleCoreAsync`,
`ObtenerDatosFacturaCoreAsync`, `ObtenerCancelacionCoreAsync`, `CancelarFacturaCoreAsync`
parametrizados por SP + nombre de parámetro id) y service con `ReenviarCoreAsync`/
`CancelarCoreAsync` — ventas y PE son SP espejo, solo cambia el id. Entidad
`FacturaPedidoEspecial` propia (no reusa `FacturaVenta`; mismo criterio que
Producción Trapeadores vs Líquidos). Los archivos PE se llaman `Factura_PE{id}_*.pdf` /
`Timbre_PE{id}_*.xml` — los mismos Replace de ventas funcionan.

## Desajuste con el tablero de tareas
El tablero listaba `SP_FACTURACION_OBTENER_CANCELACION_FACTURA` para obtener los datos de
cancelación. Verificado en BD: ese SP es la variante de **pedidos especiales**
(`FacturasPedidosEspeciales`); el que usa el DAO legado para **ventas** (`idPedidoEspecial==0`,
que es siempre el caso en esta feature) es `SP_OBTENER_CANCELACION_FACTURA` (sin prefijo
`FACTURACION_`). Se usó este último; el otro queda para la siguiente feature.

## SP_V2_CONSULTA_FACTURAS (nuevo, paginado)
El legado `SP_CONSULTA_FACTURAS` no paginaba (TOP 50 solo sin filtros) y su parámetro
`@idUsuario` estaba declarado pero **nunca se usaba en el WHERE** (línea comentada) — el filtro
de usuario del front legado nunca filtró nada. El `SP_V2` sí lo aplica, además de
paginación/búsqueda/orden estándar del proyecto.

## Gotcha de cabeceras: Estatus/Mensaje con casing inconsistente
Los SP reusados (`SP_FACTURACION_OBTENER_*`, `SP_OBTENER_CANCELACION_FACTURA`,
`SP_FACTURAS_OBTENER_PATH_ARCHIVO`) devuelven cabecera `Estatus`/`Mensaje` (PascalCase) — y
dentro del MISMO SP la rama de error a veces usa `mensaje` minúscula y la de éxito `Mensaje`
(ver `SP_FACTURACION_OBTENER_DATOS_FACTURA`). `FacturaRepository.LeerCabecera` normaliza con
`Dictionary<string,object>(..., StringComparer.OrdinalIgnoreCase)` en vez de usar
`ConsultarAsync`/`ConsultarUnicoAsync` (que esperan `status`/`mensaje` minúsculas) — mismo
criterio que `dapper-mapeo-columnas.md` punto 3, aplicado también a cabeceras, no solo a datos.

## SOAP del PAC: la Connected Service del tablero está vacía; se usa lo que el legado ejecuta de verdad
`Connected Services\servicioProduccionCancelaFactura\Reference.cs` (mencionada en el tablero)
es un **stub vacío** (`namespace lluviaBackEnd.servicioProduccionCancelaFactura { }` — Visual
Studio nunca generó el proxy, probablemente porque el WSDL es **rpc/encoded** SOAP 1.1, que WCF
no soporta). Lo que el legado ejecuta en producción son los **Web References ASMX**
`cancelaCFDI4Prod`/`cancelaCFDITest` (`SoapHttpClientProtocol`, toggle por appSetting
`FacturarPro`). Como .NET 10 no tiene `System.Web.Services` (ASMX) ni WCF soporta rpc/encoded,
se portó como **HttpClient crudo** armando el envelope SOAP a mano
(`Services/FacturacionPac/FacturacionPacClient.cs`), replicando el wrapper
`CallenviaAcuseCancelacion` (nombre del método .NET del proxy legado, distinto del nombre de
operación del WSDL) + namespace `http://edifact.com.mx/xsd`. La consulta de estatus ante el SAT
(`ConsultaEstatusFactura4`, document/literal) se implementó igual (HttpClient crudo) por
consistencia, aunque ese WSDL sí sería compatible con WCF.

**⚠️ No verificado contra el PAC/SAT reales** (instrucción explícita: no cancelar facturas
reales). Antes de ir a producción: probar `EnviarAcuseCancelacionAsync` contra
`UrlCancelacionPruebas` con un UUID de prueba y ajustar el envelope si el PAC lo rechaza.

## Certificado CSD: secreto real encontrado en el legado, NO portado
El legado firma la cancelación (XML-DSig) con un `.pfx` **embebido como recurso del assembly**
(`Resources/archivo2022_pfx.pfx`) y password en texto plano en `Web.config`
(`claveGeneraSellolluvia`). Es un secreto de producción real: no se copió ni el archivo ni el
password a este repo. Se creó la plomería completa (`CfdiXmlSignerService`, port fiel de
`ProcesaCfdi.GenerateXmlSignature` con `System.Security.Cryptography.Xml.SignedXml`) mas
**bloqueada** hasta que un humano configure `FacturacionCfdi:RutaCertificadoPfx` +
`PasswordCertificado` (User Secrets) copiando el `.pfx` real a una ruta accesible. Verificado en
runtime que el flujo falla ANTES de tocar el PAC cuando falta esta config (no hay riesgo de
cancelar algo por accidente).

## Gap de infraestructura: PDF/XML en servidor de archivos legado
`pathArchivoFactura` es relativo (`/Facturas/2026/JUNIO/Factura_768923_....pdf`); el legado le
antepone `urlDominio` para la URL pública y lee el `.xml` timbrado local (mismo folder,
`Factura_`→`Timbre_`) para el reenvío (adjuntos) y para leer el `Sello` (necesario para la
"expresión impresa" `&id=&re=&rr=&tt=&fe=` de consulta de estatus SAT). Este servidor de
archivos no es accesible desde el nuevo API todavía. Se agregó `FacturacionArchivos` (appsettings):
`UrlDominio` (URL pública, no es secreto) + `RutaBaseArchivos` (carpeta local/UNC compartida, a
configurar cuando exista el share). Sin `RutaBaseArchivos`, `reenviar` y `estatus-cancelacion`
devuelven un `Notificacion` de error claro (verificado en runtime) en vez de fallar por IO.

## IEmailService: nuevo overload multi-adjunto + Bcc
El reenvío legado manda PDF+XML (dos adjuntos) + copia oculta configurable
(`correoCCFacturas`) + copia opcional del usuario. `IEmailService` (transversal,
`Services/Email/`) solo soportaba un adjunto y no tenía Bcc — se agregó un **overload**
(`IEnumerable<EmailAdjunto> adjuntos, IEnumerable<string>? copiaOculta`) sin romper el método
existente (usado por Exportación). Reusable por otras features que necesiten adjuntos
múltiples.

## Otras decisiones
- `DetalleVentaFactura.Total` = suma de `Importe` de `SP_FACTURACION_OBTENER_DETALLE_VENTA`
  (sub-total **sin IVA**); difiere del `montoTotal` de la venta (que sí lo incluye — verificado
  en runtime: 46.80 vs 54.29 ≈ 46.80×1.16). Si el front necesita el total con impuestos, debe
  tomar `montoTotal` del listado, no el `total` del detalle.
- Catálogo de usuarios para el filtro: no existe un endpoint ligero dedicado; se reutiliza
  `GET /api/usuarios` (paginado) existente — no se creó uno nuevo (regla del contrato: revisar
  antes de crear otro).

## Archivos
`Models/Entities/{FacturaVenta,FacturaPedidoEspecial,DetalleVentaFactura,ConceptoVentaFactura,
DatosFacturaVenta,CancelacionFactura,ConfiguracionComprobante,AcuseEstatusCfdi,ArchivoFactura}.cs`,
`Models/Dtos/{FacturasQuery,ReenviarFacturaRequest,ReenviarFacturaPeRequest,CancelarFacturaRequest,
CancelarFacturaPeRequest,EstatusCancelacionRequest}.cs`,
`Models/Enums/EstatusFactura.cs`, `Repositories/Facturas/*`, `Services/Facturas/*`,
`Services/FacturacionPac/*`, `Controllers/FacturasController.cs`,
`store-procedures/SP_V2_CONSULTA_FACTURAS.sql` + `SP_V2_CONSULTA_FACTURAS_PEDIDOS_ESPECIALES.sql`,
DI + appsettings en `Program.cs`/`appsettings.json`.
`dotnet build` → 0 errores, 0 warnings (2026-07-15, ambas features). Verificado en runtime:
listar ventas y PE (200 con paginación real: 15,557 y 3,114 filas), detalle ventas y PE (200),
reenviar/estatus-cancelacion/cancelar (ventas y PE) devuelven errores de negocio controlados
(no 500) al faltar config de archivos/certificado — cancelación nunca llega a tocar el PAC real.

Relacionado: [[dapper-mapeo-columnas]], [[exportacion-reportes-csv]] (IEmailService), [[bd-local-desarrollo]].
