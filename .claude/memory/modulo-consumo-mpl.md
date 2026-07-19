---
name: modulo-consumo-mpl
description: Módulo Consumo de MPL (Costo de Producción Agranel) migrado — reporte de solo lectura con SP con caché/cálculo, catálogos años/meses, y extensión del catálogo de líneas por almacén
type: decision
---

Migra el reporte de solo lectura "Costo de Producción Agranel" (menú legado **"Consumo de
MPL"**): `ReportesController.CostoProduccionAgranel/ObtenerCostoProduccion` +
`ReportesDAO.ObtenerReporteCostoProduccionAgranel/ObtenerAnios/ObtenerMeses`. Pantalla
**distinta** de "Producción a granel" ([[modulo-produccion-agranel]]); comparten dominio
(conversión MPL) pero son vistas/controllers legados independientes.

## Endpoints
- `GET /api/consumo-mpl?anioCalculo=&mesCalculo=&idAlmacen=&idLineaProducto=&q=&page=&perPage=&order=&sort=`
  (`[Authorize]`, sin restricción de rol, igual que el legado) — listado paginado. Los 4 filtros
  son `int?` (0/ausente = TODOS); `order` whitelist
  `producto|linea|cantidadsolicitada|cantidadaceptada|costo`, default `descripcionProducto ASC`.
- `GET /api/consumo-mpl/catalogos/anios` (`SP_CONSULTA_ANIOS`) y
  `GET /api/consumo-mpl/catalogos/meses?anio=` (`SP_CONSULTA_MESES`) — catálogos derivados,
  reusados tal cual del legado.
- `GET /api/productos/catalogos/lineas?idAlmacen=` — **extendido** (no duplicado): el endpoint
  ya existente de `ProductosController` ganó un parámetro opcional `idAlmacen`; 0/ausente sigue
  yendo por `SP_V2_CONSULTA_CATALOGOS_PRODUCTO` (tipo=lineas, todas las activas), > 0 rama a
  `SP_OBTENER_LINEAS_ALMACEN` (legado, reusado tal cual).

## Hallazgo clave: el SP legado NO es una consulta pura — tiene caché con side-effects
`SP_CONSULTA_COSTO_PRODUCCION` (extraído de BD local con `OBJECT_DEFINITION`, no estaba
versionado como .sql en el legado) **no solo consulta**: por cada combinación mes/año calcula y
cachea el resultado en la tabla real `ReporteCostoProduccion` (INSERT si no existe ya el cálculo
para `UltimoDiaMesCalculo`; si el mes calculado es el mes actual, primero **borra** el caché de
ese mes para forzar recálculo, porque el mes aún no cierra). El `SP_V2_CONSULTA_COSTO_PRODUCCION`
nuevo **copia esa lógica de cálculo/caché tal cual** (mismo INSERT/UPDATE de
`cantidadSolicitadaMesAnt`/`cantidadAceptadaFinalMesAnt`/`ultCostoCompra`/`porcCostoProduccion`/
`costoProduccionMerma`, mismas funciones `dbo.redondear`/`dbo.obtenerPrecioCompra`/
`dbo.FechaActual`/`dbo.ExisteProductoEnAlmancen` — este último con el typo real "Almancen" en
BD) y solo agrega `@search`/`@order`/`@sort`/paginación **sobre el SELECT final**. Verificado
con `EXEC` directo contra BD local: `anioCalculo=2023,mesCalculo=4` devuelve total=83 (datos
reales cacheados desde 2022-10 hasta 2026-06), paginación/orden/búsqueda funcionando.

## Divergencia deliberada respecto al legado
El SP original, si el resultado queda vacío, fija `status=-1` con mensaje "No se encontraron
resultados." El `SP_V2` **no replica eso**: con paginación real, una página vacía (filtro que no
matchea, o página fuera de rango) es un resultado válido — `status=200`, `total=0`. Mismo
criterio que los demás listados paginados de este repo (el front resuelve el estado vacío).
Documentado en el propio .sql.

## Entidad recortada a propósito
El modelo legado `CostoProduccionAgranel.cs` trae más campos de los que la vista
`_ObtenerCostoProduccion.cshtml` realmente pinta. Se confirmó contra el resultset real del SP en
BD que **no existen** en absoluto en el `SELECT` final: `nombreUsuario`, `descripcionEstatus`,
`cantidad`/`cantidadAceptada`/`cantidadRestante` (pertenecen a otro reporte — el modelo legado
solo los declara porque se comparte laxamente). Sí existen en la tabla/resultset pero **se
omiten a propósito** por no ser usados por la vista: `idReporteCostoProduccion`,
`porcCostoProduccion`, `ultimoDiaMesCalculo`/`ultimoDiaMesAnterior`, `fechaAlta`. La entidad
nueva `CostoProduccionAgranel` (`Models/Entities/`) solo trae los 9 campos que la vista consume.
"Cantidad Restante" no viaja en el resultset — la calcula el front (regla ya en la HU).

## Catálogos años/meses: forma de resultset distinta al resto
`SP_CONSULTA_ANIOS`/`SP_CONSULTA_MESES` traen cabecera `status/mensaje` (+ `error_procedure`/
`error_line` que se ignoran) pero el resultset de datos usa columnas `Value`/`Text` (patrón
`SelectListItem` del legado), no `id`/`descripcion`. Por eso **no** se pudo reusar
`BaseRepository.ConsultarCatalogoAsync` (asume columna `descripcion`); se leyó manualmente con
diccionario case-insensitive, mismo precedente que
`ProduccionAgranelRepository.ObtenerEstatusAsync` ([[modulo-produccion-agranel]]).

## Catálogo líneas-por-almacén: sin envoltura status/mensaje
`SP_OBTENER_LINEAS_ALMACEN` no devuelve NINGUNA cabecera (ni `status` ni `Estatus`) — solo las
filas crudas (`contador, idLineaProducto, idAlmacen, descripcion`), confirmado en BD. Se envuelve
manualmente en `Notificacion` con `Estatus=200` fijo, mismo precedente que
`LimitesInventarioRepository.ObtenerEstatusAsync` (`SP_OBTENER_ESTATUS_LIMITES_INVENTARIO`,
también sin cabecera).

## Pendiente para el revisor
No se hizo smoke test HTTP end-to-end con JWT real (sin credencial de prueba a mano en esta
sesión); se verificó exhaustivamente a nivel SQL (SP nuevo + los 3 legados reusados) con `EXEC`
directo contra BD local, y `dotnet build` en 0 warnings/0 errores.

## Archivos
`Models/Entities/CostoProduccionAgranel.cs`, `Models/Dtos/ConsumoMplQuery.cs`,
`store-procedures/SP_V2_CONSULTA_COSTO_PRODUCCION.sql` (nuevo, desplegado en BD local),
`Repositories/ConsumoMpl/*`, `Services/ConsumoMpl/*`, `Controllers/ConsumoMplController.cs`, DI
en `Program.cs`. Cambios en módulo Productos (no nuevos archivos):
`Repositories/Productos/IProductosRepository.cs`/`ProductosRepository.cs`,
`Services/Productos/IProductosService.cs`/`ProductosService.cs`,
`Controllers/ProductosController.cs` — `ObtenerLineasAsync`/`ObtenerLineas` ganaron parámetro
opcional `idAlmacen`. `dotnet build` → 0 errores, 0 warnings (2026-07-05).

Relacionado: [[modulo-produccion-agranel]], [[modulo-productos]], [[dapper-mapeo-columnas]],
[[bd-local-desarrollo]].
