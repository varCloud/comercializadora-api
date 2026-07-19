# Módulo Producción a granel migrado (listado + comandos operativos)

Migra el dominio "Producción a granel" del legado: `ProduccionAgranelController` (pantalla web
de consulta) + los webservices móviles `AdminProduccionAgranelController` y
`AdminLiquidosController.agregarLiquidosAInventario`. A diferencia de Producción
Líquidos/Trapeadores (solo lectura), este módulo **sí tiene comandos** (alta a producción,
aprobación, envasado) que reutilizan los SP legados `SP_APP_*` sin modificarlos.

## Endpoints (`/api/produccion-agranel`, todos `[Authorize]`)
- `GET /` — listado paginado (`page,perPage,idUsuario,idEstatus,idAlmacen,fechaIni,fechaFin,order,sort`);
  `order` whitelist `fecha|producto|cantidad|estatus`, default `fechaAlta DESC`.
- `GET /catalogos/estatus` — `CatEstatusProcesoAgranel` con id > 1 (la UI antepone TODOS).
- `POST /` — alta MPL→granel; `PATCH /aprobar` — aprobación por lote (XML);
  `POST /envasado` — granel→envasado. En los 3, `idUsuario` sale del claim JWT.

## SPs
- **Nuevo** `SP_V2_CONSULTA_PROCESO_PRODUCCION_AGRANEL` (`store-procedures/`, CREATE OR ALTER,
  desplegado y smoke-tested en BD local 2026-07-04: total=21844 sin filtros). Divergencias
  deliberadas documentadas en el .sql: default DESC (legado sin ORDER BY), corrige typo
  "Pendiende"→"Pendiente", `cantidadRestante = ROUND(cantidad - cantidadAceptada, 2)` (la
  columna física no siempre está poblada; el legado la calculaba en Razor), filtro extra
  `@idAlmacen` (cubre el WS móvil `obtenerProductosProduccionAgranel`, que NO se migró como
  endpoint aparte).
- **Reusados sin cambios:** `SP_CONSULTA_ESTATUS_PROCESO_PRODUCCION` (catálogo; resultsets
  header + `value/text`), `SP_APP_INVENTARIO_AGREGAR_PRODUCTO_PRODUCCION_AGRANEL`,
  `SP_APP_APROBAR_PRODUCTOS_PRODCUCCION_AGRANEL` (⚠️ el typo "PRODCUCCION" es el nombre real
  en BD), `SP_APP_AGREGAR_PRODUCTO_INVENTARIO_LIQUIDOS_ENVASADO`.

## Gotchas
- Los `SP_APP_*` devuelven cabecera `Estatus/Mensaje` (mayúscula inicial) y en error columnas
  extra (ErrorNumber/ErrorMessage): el repository los lee con `EjecutarAppAsync` (dict
  case-insensitive, mismo criterio que [[dapper-mapeo-columnas]]); **no** sirve el
  `EjecutarAsync` del BaseRepository (espera `status/mensaje`).
- El SP de aprobar espera `@xmlProductos` con raíz `ArrayOfProductosProduccionAgranel` (XPath
  `//ArrayOfProductosProduccionAgranel/ProductosProduccionAgranel`); se arma con XElement
  (InvariantCulture), mismo patrón que Compras. Solo lee idProcesoProduccionAgranel/idProducto/
  idUbicacion/cantidadAtendida/observaciones — el estatus final (3/4/5) lo calcula el propio SP.
- `AdminLiquidosController.BuscarCargaMercanciaLiquidos` y `obtenerProductosXLineaProducto`
  NO se migraron: ya cubiertos por `GET /api/produccion-liquidos` y `GET /api/productos`.

## Archivos
`Models/Entities/ProcesoProduccionAgranel.cs`, `Models/Dtos/ProduccionAgranelQuery.cs` +
`Agregar/Aprobar/AgregarEnvasado*Request.cs`, `Repositories/ProduccionAgranel/*`,
`Services/ProduccionAgranel/*`, `Controllers/ProduccionAgranelController.cs`, DI en
`Program.cs`. `dotnet build` → 0 errores, 0 warnings (2026-07-04).

Relacionado: [[modulo-produccion-liquidos]], [[dapper-mapeo-columnas]], [[bd-local-desarrollo]].
