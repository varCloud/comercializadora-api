---
name: modulo-reportes-inventario
description: Primera sub-feature del módulo Reportes; SP único que unifica listado paginado + export completo vía bandera @exportar
type: project
---

Primera sub-feature del módulo legado **Reportes** (el más grande de los pendientes:
Ventas, Devoluciones, Merma, MargenBruto, DropSize, Cierres, CostoProducción,
DiasPromedioInventario, etc. — se migran uno a la vez, ver
`.claude/docs/feature/reporte_inventario/`).

## `SP_V2_CONSULTA_INVENTARIO` — un solo SP para 2 usos (decisión de diseño)

Unifica los dos SP legados (`SP_CONSULTA_INVENTARIO` + `SP_CONSULTA_INVENTARIO_GENERAL_UBICACION`,
ambos intactos en BD, no versionados en el repo — quedan como referencia histórica, convención
`SP_V2_*` no reemplaza) mediante parámetros:
- **Modo listado** (`@exportar = 0`, default): pagina con `@idLineaProducto, @idAlmacen, @search,
  @fechaIni, @fechaFin, @page, @perPage, @order, @sort` — OFFSET/FETCH.
- **Modo exportación** (`@exportar = 1`): ignora paginación y TODOS los filtros, devuelve el
  inventario completo con el set de columnas de `@tipo` (1 General / 2 Ubicación, paridad exacta
  con el legado — resultset "unión" con columnas de ubicación nulas cuando `tipo=1`).

Decisión del usuario tras aclarar que el legado tenía 2 endpoints independientes (listado
filtrado vs. export fijo sin filtros): en vez de replicar 2 SP, se unificó en 1 con la bandera,
más simple de mantener.

## Rango de fechas = snapshot en `@fechaFin`, no expansión día a día

El legado calculaba "cantidad a la fecha" como snapshot a **un** punto en el tiempo; el propio
`SP_CONSULTA_INVENTARIO` legado trae, comentado/deshabilitado, un intento previo de expandir a
rango día-a-día (CTE recursivo) — evidencia de que ya se probó y se descartó, probablemente por
costo (`InventarioDetalleLog` tiene **~5.3M filas** en la BD real). Se decidió **no revivir esa
expansión**: `@fechaFin` es la fecha de corte (tope a hoy); `@fechaIni` se acepta para el
contrato de rango que pide el Front (regla 18) pero no multiplica filas. Con el default hoy/hoy
el comportamiento es idéntico al legado. Si se necesita un desglose real día-a-día en el futuro,
retomar la CTE comentada en el `.sql` (evaluando costo antes).

## Endpoint de exportación — mismo patrón que Inventario Físico

`GET /api/reportes/inventario/exportar?tipo=1|2` sigue **exacto** el patrón de
`InventarioFisicoController.ExportarAjustes` (`IExportacionService`, CSV vía `ICsvGeneratorService`
— ver [[exportacion-reportes-csv]]): resuelve destinatario vía `IUsuariosService.ObtenerPorIdAsync`
(400 explícito si no tiene correo), arma `ColumnaExportable<T>` según `tipo`, devuelve
`File(bytes, ContentTypeCsv, nombre)` si es descarga inmediata o `Ok(Notificacion<string>)` si se
difiere. Con datasets reales (4094/42123 filas, tipo 1/2) siempre cae en "diferido" (> umbral 1000).

## Bug de performance descubierto y corregido (2026-07-15): parameter sniffing

El usuario reportó ~14s de respuesta en `GET /api/reportes/inventario` con filtros de fecha.
Diagnóstico directo contra BD (pyodbc, `SET STATISTICS TIME`/timing por statement):
- `EXEC SP_V2_CONSULTA_INVENTARIO` con el plan cacheado original: **13.9s** (repetible).
- Mismo `EXEC` tras `sp_recompile`: **2.0s**. Cuerpo del SP ejecutado inline (plan fresco
  sin pasar por el objeto cacheado): **2.6s**.
- Causa: el plan cacheado (compilado la primera vez, probablemente con el modo
  `@exportar=1` del smoke-test de deploy) se reusaba para llamadas con forma de parámetros
  muy distinta (filtros de fecha/línea/almacén/búsqueda NULL o no) — clásico *parameter
  sniffing* agravado por los dos statements que arman `#CANTIDAD_INVENTARIO` y `#INVENTARIO`
  contra `InventarioDetalleLog` (5.3M filas), muy sensibles a la cardinalidad real.
- **Fix:** `OPTION (RECOMPILE)` en esas dos consultas (no en todo el SP) — fuerza un plan
  ajustado a los parámetros reales de cada llamada. Verificado post-fix con 6 combinaciones
  de parámetros distintas intercaladas (incluyendo el modo export): **consistente ~1.8s**,
  sin ningún pico. El costo de recompilar en cada llamada es aceptable para una pantalla de
  reporte (no es un endpoint de alta frecuencia).
- Si en el futuro esto se vuelve un problema de CPU por volumen de llamadas, alternativa:
  `OPTIMIZE FOR UNKNOWN` en vez de `RECOMPILE` completo, o plan guides.
- **Patrón a vigilar en los próximos sub-reportes de Reportes** (Ventas, Merma, etc.): si
  arman resultados con temp tables + filtros muy variables sobre tablas grandes, considerar
  `OPTION (RECOMPILE)` desde el diseño inicial en vez de descubrirlo por un reporte de
  performance después del deploy.

## Gap descubierto: `Content-Disposition` no expuesto en CORS

El Front no puede leer el nombre real de archivo del header `Content-Disposition` porque el CORS
de la API no lo declara en `WithExposedHeaders`. No bloqueante (el Front mitiga con un nombre por
defecto client-side), pero si se quiere el nombre real del backend, agregar
`WithExposedHeaders("Content-Disposition")` a la policy de CORS en `Program.cs`.

## Contrato real (verificado en runtime, smoke-test con JWT real)

- `GET /api/reportes/inventario?idLineaProducto=&idAlmacen=&q=&fechaIni=&fechaFin=&page=&perPage=&order=&sort=`
  → `Notificacion<T[]>` con `links`/`meta`. Item: `{ fecha, almacen, descripcionLinea, descripcion,
  codigoBarras, cantidad, costo }`. `order` whitelist `fecha|almacen|producto|cantidad|costo`.
  Param de búsqueda es **`q`** (convención real `PagedQuery.Q`), no `search`.
- Smoke-test: sin filtros → 10180 filas; `q=agua` → 55; línea+almacén → 46; export tipo 1 → 4094
  filas, tipo 2 → 42123 filas.

Relacionado: [[exportacion-reportes-csv]], [[modulo-inventario-fisico]].
