# Servicio transversal de exportación a CSV (umbral descarga vs. correo)

Diseñado e implementado (no migración de módulo, feature transversal nueva): `IExportacionService`
decide, por volumen de datos, entre descarga inmediata (`<= Exportacion:UmbralDescargaInmediata`,
default 1000) o generación en segundo plano + envío por correo (`>` umbral). Sustituye el patrón
legado `generaCSVInventario` (`ReportesController` de `lluviaBackEnd`), que siempre descargaba
**y** enviaba por correo sin regla de umbral.

**Formato: CSV, no Excel.** Primera versión usó ClosedXML (.xlsx) por mala lectura del
requerimiento; se corrigió a CSV (`ICsvGeneratorService`/`CsvGeneratorService` con **CsvHelper**,
UTF-8 con BOM para que Excel abra acentos/ñ correctamente) porque `generaCSVInventario` en el
legado genera CSV plano, no un libro de Excel real. Se quitó el paquete `ClosedXML` y se agregó
`CsvHelper` (33.1.0).

Piezas: `Services/Exportacion/` (`IExportacionService`, `ICsvGeneratorService`), `Services/Email/`
(`IEmailService` con **MailKit** 4.17.0, reemplaza el estático `Email.*` del legado),
`Infraestructura/BackgroundTasks/` (`IBackgroundTaskQueue` sobre `Channel<T>` +
`ExportacionQueuedHostedService`). Detalle completo y justificación de decisiones en
`.claude/arquitectura/exportacion-reportes.md`.

**Envío de correo: Gmail, igual que el legado.** Confirmado en `Utilerias/Email.cs` del legado
(`smtp.gmail.com:587`, `EnableSsl=true`, credenciales `correoProveedor`/`contrasenaProveedor` en
`Web.config`). `SmtpOptions` reproduce ese mismo host/puerto por defecto en `appsettings.json`.
Gotcha real: si la cuenta de Gmail tiene 2FA, la contraseña de `Smtp:Contrasena` debe ser una
**App Password** (`myaccount.google.com/apppasswords`), no la contraseña normal — probablemente
`contrasenaProveedor` del legado ya es una App Password, reusable tal cual en User Secrets.

**Gap RESUELTO (2026-07-19), no era lo que parecía:** se había agregado `Usuario.Correo`
(nullable) especulativamente, asumiendo que el destinatario debía ser el correo del usuario que
solicita el reporte. Al debuguear el 400 real en `reporte_ventas`, se investigó el legado
(`Utilerias/Email.cs`) y se confirmó que **la tabla `Usuarios` nunca tuvo columna de correo, ni
en el legado** — y que el legado tampoco lo necesitaba: `EnviarCorreoConAdjunto` (único flujo de
email de reportes del legado) mandaba siempre a una lista fija (`correoCCFacturas` en
`Web.config`) por Bcc, nunca al correo de quien pidió el reporte. Se **eliminó `Usuario.Correo`**
(campo muerto) y se reemplazó por `ExportacionOptions.CorreoDestino`
(`IReadOnlyList<string>`, sección `Exportacion`, valores reales en User Secrets): el primer
correo es el destinatario (`DestinatarioExportacion.Correo`), el resto va como Bcc
(`CopiasOcultas`). `IEmailService.EnviarAsync`/`ExportacionService.ExportarAsync` ya soportaban
Bcc pero no se usaba desde este flujo; ahora sí. Los 3 controllers que resuelven destinatario
(`ReportesVentasController`, `ReportesInventarioController`, `InventarioFisicoController`, código
duplicado idéntico en los 3) se actualizaron igual. Detalle en
`.claude/arquitectura/exportacion-reportes.md` (sección "Resuelto 2026-07-19").

Primer consumidor real: `InventarioFisicoController.ExportarAjustes`
(`GET api/inventario-fisico/{id}/ajustes/exportar`), reusando los ajustes ya consultados por
`ObtenerAjustesAsync` (no agrega query nueva).

**Wiring real ya configurado (2026-07-19, User Secrets, dev):** `Smtp:Usuario`/`Contrasena`/
`RemitenteCorreo` = cuenta Gmail real del legado (`comercializadoralluviadev@gmail.com`, misma
App Password que `contrasenaProveedor` en el `Web.config` legado — confirma que sí es una App
Password reusable). `Exportacion:CorreoDestino:0`/`:1` = los mismos 2 correos de
`correoCCFacturas` legado. Antes de este fix ningún envío real había sido posible (credenciales
vacías + bloqueo previo del destinatario).
