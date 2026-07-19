# Exportación de reportes a CSV (umbral descarga vs. correo)

> Servicio transversal, no ligado a un módulo. Cualquier reporte del sistema (inventario,
> consumo MPL, producción, compras, …) lo consume en vez de reimplementar la decisión.

## Regla de negocio

- **`Total <= Exportacion:UmbralDescargaInmediata`** (default `1000`, `appsettings.json`):
  se genera el CSV en el hilo de la request y se devuelve para descarga inmediata.
- **`Total > umbral`**: se encola el trabajo (no bloquea la request), se genera el CSV en
  segundo plano y se envía por correo; el front recibe un `Notificacion<string>` con el
  mensaje de aviso (no el archivo).

## Piezas (todas nuevas, sin equivalente previo en este repo)

```
Services/Exportacion/
  ColumnaExportable.cs        # (Header, Func<T, object?>) — el llamador arma la lista por reporte
  ICsvGeneratorService.cs / CsvGeneratorService.cs   # CsvHelper, en memoria, sin temporales
  DestinatarioExportacion.cs  # (IdUsuario, NombreCompleto, Correo) — lo resuelve el controller
  ResultadoExportacion.cs     # Descarga(bytes) | Diferido(mensaje)
  ExportacionOptions.cs       # sección "Exportacion"
  IExportacionService.cs / ExportacionService.cs         # la decisión de umbral vive aquí

Services/Email/
  IEmailService.cs / SmtpEmailService.cs   # MailKit; reemplaza el estático Email.* del legado
  SmtpOptions.cs               # sección "Smtp" (Usuario/Contrasena → User Secrets, no versionados)
  EmailAdjunto.cs

Infraestructura/BackgroundTasks/
  IBackgroundTaskQueue.cs / BackgroundTaskQueue.cs   # Channel<T> acotado (capacidad 50)
  ExportacionQueuedHostedService.cs                  # BackgroundService que consume la cola
```

Registro en DI (`Program.cs`): `ICsvGeneratorService`/`IEmailService`/`IExportacionService`
como **singleton** (sin estado, mismo criterio que `UbicacionesPdfService`/`CodigosBarrasPdfService`);
`IBackgroundTaskQueue` singleton; `ExportacionQueuedHostedService` como `AddHostedService`.

## Por qué una cola en memoria (`Channel<T>`) y no Hangfire

Es la opción más simple que cumple "no bloquear el hilo HTTP" sin agregar infraestructura
nueva (Redis, tabla de jobs, dashboard). **Trade-off aceptado:** si el proceso se reinicia con
trabajo pendiente en la cola, ese trabajo se pierde (no hay reintento ni persistencia). Si
esto se vuelve un problema real (colas largas, necesidad de reintentos/monitoreo), la
escalación natural es **Hangfire** (persistido en SQL Server, dashboard, reintentos) sin
tocar el contrato de `IExportacionService` — solo cambiaría la implementación de la cola.

## Por qué genérico (`ExportarAsync<T>`) pese a que `patron-repository.md` prohíbe genéricos

La prohibición de `patron-repository.md` es específica a **`IRepository<T>` estilo EF**
(`Add/Update/Delete/GetAll` genérico que asume change tracking). Este servicio es distinto:
no es una fachada de datos, es una utilidad de transformación (datos + mapeo de columnas →
archivo) que por diseño debe servir a cualquier entidad de reporte. Generics aquí son la
herramienta correcta, no el anti-patrón que evita esa regla.

## Envío de correo: Gmail (igual que el legado)

El legado (`Utilerias/Email.cs`, `EnviarCorreoConAdjunto`/`EnviarCorreoExternoUsuario`) ya envía
todo por **Gmail SMTP** (`smtp.gmail.com:587`, `EnableSsl=true`, credenciales de
`correoProveedor`/`contrasenaProveedor` en `Web.config`). `SmtpOptions` reproduce el mismo
host/puerto/TLS por defecto (`appsettings.json`); solo cambia dónde viven las credenciales
(User Secrets en vez de `Web.config` plano).

⚠️ **Gotcha real de Gmail**: si la cuenta usada tiene verificación en 2 pasos (2FA) activada
— condición cada vez más común/forzada por Google —, **no acepta la contraseña normal de la
cuenta por SMTP**; hace falta generar una **contraseña de aplicación** (App Password) en
`myaccount.google.com/apppasswords` y usar esa cadena de 16 caracteres como `Smtp:Contrasena`.
Si el correo del legado ya funciona hoy, lo más seguro es que `contrasenaProveedor` en su
`Web.config` **ya sea** una App Password (no la contraseña real de la cuenta) — reusar el mismo
valor en User Secrets debería funcionar sin gestionar nada nuevo en la cuenta de Google.

## Resuelto (2026-07-19): el destinatario NUNCA fue el correo del usuario

Se investigó el legado (`Utilerias/Email.cs`) para resolver este gap: la tabla `Usuarios` real
**nunca tuvo columna de correo** (ni en el legado ni en la migración — se había agregado
`Usuario.Correo` como preparación especulativa, pero ningún SP la alimentaba; se **eliminó**).
El legado tampoco lo necesitaba: `EnviarCorreoConAdjunto` (el único flujo de email de reportes
en el legado, usado por `generaCSVInventario`/`ReporteGeneral`) **nunca resolvía el correo del
usuario logueado** — enviaba siempre a una lista de distribución fija vía Bcc
(`correoCCFacturas` en `Web.config`, sin `.To` de nadie en particular), desde la cuenta
`correoProveedor`.

La migración reproduce ese mismo criterio con **`ExportacionOptions.CorreoDestino`**
(`IReadOnlyList<string>`, sección `Exportacion` — configurar en User Secrets, no versionar):
el primer correo de la lista es el destinatario principal (`DestinatarioExportacion.Correo`),
el resto va como Bcc (`DestinatarioExportacion.CopiasOcultas`, ver `IEmailService.EnviarAsync`).
Cada controller que llama `IExportacionService` resuelve `NombreCompleto` vía
`IUsuariosService.ObtenerPorIdAsync` (solo para personalizar el saludo del correo) y toma
`Correo`/`CopiasOcultas` de `ExportacionOptions.CorreoDestino` — nunca de `Usuario`. Si la lista
está vacía, error de negocio 400 claro (ver `ResolverDestinatarioAsync` en
`ReportesVentasController`/`ReportesInventarioController`/`InventarioFisicoController`).

## Ejemplo de consumo

`InventarioFisicoController.ExportarAjustes` (`GET api/inventario-fisico/{id}/ajustes/exportar`)
migra `generaCSVInventario` del legado (que siempre descargaba **y** enviaba por correo, sin
regla de umbral) aplicando esta nueva decisión. Reutiliza los ajustes ya consultados por
`ObtenerAjustesAsync` (lista completa sin paginar) — no se agregó una consulta nueva.
