---
name: modulo-reportes-merma
description: Módulo Reportes → Merma migrado (API-1..API-4) — reporte solo lectura con paginación en memoria desde el día uno, catálogos año/mes en cascada, y descubrimiento de que el patrón "ExportacionOptions.CorreoDestino" que debía copiarse vivía en una rama sin mergear
type: decision
---

Migra el tercer sub-reporte del módulo legado "Reportes" (tras Inventario y Ventas):
`ReportesController.Merma/ObtenerMerma/ObtenerMesesAnio` + `ReportesDAO.ObtenerMerma/
ObtenerAnios/ObtenerMeses`. API-1..API-4 completos en la rama `feature/reporte_merma` (creada
desde `main`). Front (FE-1..FE-4) pendiente.

## Hallazgo crítico: el patrón a copiar vivía en una rama sin mergear
La tarea pedía seguir "tal cual" el patrón de `VentaReporteRepository.cs`/
`ReportesVentasController.cs`/`ExportacionOptions.CorreoDestino` (paginación en memoria +
exportación con destinatario fijo). **Ninguno de esos archivos existe en `main`**: viven en
`feature/reporte_ventas`, que aún no está mergeada. El índice de CodeGraph del proyecto SÍ tenía
esos archivos indexados (probablemente se indexó estando esa rama activa) y los devolvió como
"verbatim, current on-disk source" — **falso positivo**: hay que verificar con `git status`/
`git ls-tree <rama> --name-only` cuando CodeGraph devuelve contenido de un módulo que no
aparece al listar el directorio real. Se resolvió leyendo el contenido real con
`git show feature/reporte_ventas:<path>` (sin cambiar de rama, la tarea lo prohibía) y portando a
`feature/reporte_merma` solo lo mínimo indispensable para que el patrón compilara y funcionara:
`ExportacionOptions.CorreoDestino` (`IReadOnlyList<string>`, nueva propiedad),
`appsettings.json` (`Exportacion:CorreoDestino: []`), `DestinatarioExportacion` (parámetro
opcional `CopiasOcultas`) y `ExportacionService.ExportarAsync` (cambiado al overload
`adjuntos`+`copiaOculta` de `IEmailService`, que ya existía en `main` desde el módulo
Facturación — no hubo que tocar la interfaz). **No se portaron**
`VentaReporteRepository.cs`/`ReportesVentasController.cs` (fuera de alcance de Merma).
User Secrets ya traía `Exportacion:CorreoDestino:0/1` configurados de antes de esta sesión —
solo faltaba el binding C#.

**Riesgo documentado para cuando `feature/reporte_ventas` se mergee**: `ExportacionOptions.cs`,
`DestinatarioExportacion.cs`, `ExportacionService.cs` y `appsettings.json` van a tener cambios
paralelos equivalentes → conflicto de merge esperado, resolver quedándose con una sola versión
(son textualmente casi idénticos).

## Bug encontrado (y NO copiado) en el patrón de origen
`ReportesVentasController.ResolverDestinatarioAsync` (en `feature/reporte_ventas`) usa
`_exportacionOpciones.CorreoDestino[1]` como destinatario principal (To), lo cual contradice su
propio comentario XML ("el primer correo de la lista va como destinatario principal") y
truena con `IndexOutOfRangeException` si solo hay 1 correo configurado. En
`ReportesMermaController` se implementó `CorreoDestino[0]` como To y `Skip(1)` como
`CopiasOcultas`/Bcc — el fix, no la copia literal del bug. Reportarlo si/cuando se revise
`reporte_ventas`.

## Confirmado con SQL real contra BD de desarrollo (no asumido)
- `SP_CONSULTA_MERMA(@mesCalculo, @anioCalculo, @idLinea, @idAlmacen, @silent=0)`: sin año/mes,
  usa `coalesce(@anioCalculo, year(fechaActual))`/`coalesce(@mesCalculo, month(fechaActual))` →
  **mes/año calendario ACTUAL**, no "el más reciente con datos". `@silent` no lo pasa el DAO
  legado (queda en su default 0); si se manda ≠0 el SP omite hasta el resultset de cabecera —
  el repository nuevo lo deja sin pasar a propósito.
- No pagina (sin `@page`/`@pageSize`, sin OFFSET/FETCH) — mismo patrón de
  `VentaReporteRepository` post-corrección: `ListarAsync`/`ExportarAsync` comparten un helper
  privado (`ObtenerTodosAsync`) que llama al SP una sola vez; la página se resuelve en memoria
  con `Skip`/`Take`.
- Resultset de datos = `ReporteMerma.*` + `codigoBarras`/`descripcionProducto` (Productos) +
  `idLineaProducto`/`descripcionLinea` (LineaProducto); columnas coinciden case-insensitive con
  `MermaItem`, no hizo falta una clase de fila intermedia (a diferencia de
  `VentaReporteRepository`, que sí necesita `VentaReporteRow` por nombres de columna
  distintos al contrato). Se omite `errorHumano` (columna interna sin uso en pantalla).
- El SP tiene efecto colateral de cálculo/caché en la tabla `ReporteMerma` (INSERT guardado con
  `IF NOT EXISTS`, idempotente; si el mes calculado es el actual, primero borra su caché para
  forzar recálculo) — llamarlo dos veces por request (listar+exportar comparten helper, pero
  cada request nueva SÍ vuelve a invocar el SP) no es un problema de datos, solo de performance.
- `SP_CONSULTA_ANIOS`/`SP_CONSULTA_MESES(@anio)` devuelven columnas `Value`/`Text` (no
  `id`/`descripcion`) — mismo shape que ya resolvía `ConsumoMplRepository.
  LeerCatalogoValueTextAsync`; se duplicó el mismo helper privado en
  `MermaReporteRepository` (convención ya establecida en este repo: SP de catálogo genérico
  compartidos entre reportes NO se centralizan, cada repository trae su copia).
- **Catálogo Línea de Producto**: se confirmó que `ObtenerLineasAlmacen(0)` (legado, SP
  `SP_OBTENER_LINEAS_ALMACEN`) con `@idAlmacen` nulo/0 cae en su rama `else`
  (`select ... from LineaProducto where activo=1`, sin filtrar) — funcionalmente idéntico
  (mismas 21 filas verificadas en BD) al catálogo ya reusado en Ventas/Inventario/Límites de
  Inventario (`SP_V2_CONSULTA_CATALOGOS_PRODUCTO @tipo=lineas`). **No se creó endpoint nuevo**;
  el Front de Merma debe reusar el catálogo de líneas existente.
- **`Usuario.Correo` NO fue eliminado** en `main`/`feature/reporte_merma` (sigue en
  `Models/Entities/Usuario.cs`); la tarea decía que sí se había eliminado — esa eliminación
  aparentemente solo ocurrió/está planeada en `feature/reporte_ventas`. Se siguió la instrucción
  de no usarlo de todos modos (decisión cerrada de la HU), documentando la discrepancia para que
  no se asuma erróneamente en `main`.

## Verificación runtime real (JWT firmado localmente, sin credencial de usuario a mano)
Se firmó un JWT HS256 manualmente con la `Jwt:Key` de User Secrets (mismos claims que
`JwtTokenGenerator`: `idUsuario`/`idRol`/`idSucursal`/`idAlmacen`/`idEstacion`) para evitar
depender de una contraseña de usuario real — reutilizable en futuros módulos que no tengan
credencial de prueba a mano (ver también `modulo-consumo-mpl.md`, que no pudo hacer smoke test
por esta misma razón). Resultado contra la BD real de desarrollo:
- Sin token → `401` en listado y exportar.
- `GET /api/reportes/merma?anioCalculo=2025&mesCalculo=6&page=1&perPage=3` → `200`, 199 filas
  totales, 67 páginas.
- `GET /api/reportes/merma/exportar?anioCalculo=2025&mesCalculo=6` → `200 File` CSV (199 filas
  ≤ umbral 1000, descarga inmediata).
- `GET /api/reportes/merma/exportar?anioCalculo=2020&mesCalculo=1` (mes sin datos) → `400` con
  el mensaje real del SP ("No se encontraron resultados.").
- `GET /api/reportes/merma/anios` → `200`, 2020..2026.
- `GET /api/reportes/merma/meses?anio=2025` → `200`, 12 meses (Diciembre..Enero).
- `GET /api/reportes/merma` sin filtros → `200`, 117 filas (mes actual del entorno, julio 2026).

## Archivos
`Models/Entities/MermaItem.cs`, `Models/Dtos/MermaQuery.cs`, `Repositories/ReportesMerma/*`,
`Services/ReportesMerma/*`, `Controllers/ReportesMermaController.cs`, DI en `Program.cs`.
Cambios en infraestructura compartida de exportación (portados desde `feature/reporte_ventas`,
ver arriba): `Services/Exportacion/ExportacionOptions.cs`,
`Services/Exportacion/DestinatarioExportacion.cs`, `Services/Exportacion/ExportacionService.cs`,
`appsettings.json`. `dotnet build` → 0 errores, 0 warnings (2026-07-19).

Relacionado: [[modulo-reportes-inventario]], [[modulo-consumo-mpl]], [[exportacion-reportes-csv]],
[[dapper-mapeo-columnas]], [[bd-local-desarrollo]].
