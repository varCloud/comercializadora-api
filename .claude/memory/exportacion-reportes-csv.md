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

**Gap detectado y solo parcialmente resuelto:** ni el JWT ni `Usuario` tenían correo. Se agregó
`Usuario.Correo` (nullable) a la entidad, pero **ningún SP lo alimenta todavía** — falta agregar
la columna/alias en `SP_V2_CONSULTA_USUARIOS`. Hasta entonces, `Correo` llega `null` y los
controllers que exporten deben devolver error de negocio explícito (no fallar silenciosamente).

Primer consumidor real: `InventarioFisicoController.ExportarAjustes`
(`GET api/inventario-fisico/{id}/ajustes/exportar`), reusando los ajustes ya consultados por
`ObtenerAjustesAsync` (no agrega query nueva).

**Pendiente de wiring real:** `Smtp:Usuario`/`Smtp:Contrasena` deben cargarse vía User Secrets
antes de que el envío de correo funcione en desarrollo/producción.
