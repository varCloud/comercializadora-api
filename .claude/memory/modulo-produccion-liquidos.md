# Módulo Producción Líquidos migrado (reporte solo lectura)

Reporte de consulta de "carga de mercancía de líquidos" (movimientos de inventario). Migra
`ProductosController.BuscarCargaMercanciaLiquidos` / `ProductosDAO.BuscarCargaMercanciaLiquidos`
/ `FiltroLiquidos` del legado. **Sin alta/edición/baja** — a diferencia de los módulos previos,
es puramente informativo: un único endpoint `GET /api/produccion-liquidos` `[Authorize]`.

## Endpoint
`GET /api/produccion-liquidos?page=&perPage=&idRol=&idUsuario=&fechaIni=&fechaFin=&order=&sort=`
→ `Notificacion<IEnumerable<CargaMercanciaLiquidos>>` (con `links`/`meta`). Sin `q` de texto
libre (filtros estructurados, no buscador genérico). `order` whitelist: `fecha|cantidad|producto`.

## SP
**Nuevo** `SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS` (`store-procedures/`, CREATE OR ALTER,
desplegado y smoke-tested en BD local 2026-07-03: `total=24377` sin filtros). **No modifica**
`SP_CONSULTA_CARGA_MERCANCIA_LIQUIDOS` (sigue sirviendo al legado).

**Cómo se construyó:** el legado no tenía el `.sql` versionado en ningún repo — solo existía en
la BD. Se leyó la definición real con `OBJECT_DEFINITION(OBJECT_ID('dbo.SP_CONSULTA_CARGA_...'))`
vía `sqlcmd` antes de portar la lógica (JOINs a `InventarioDetalleLog`/`Usuarios`/`CatRoles`/
`CatTipoMovimientoInventario`/`Productos`/`InventarioDetalle`/`Ubicacion`/`Almacenes`,
`dbo.redondear`). **No basta con `EXEC` al SP legado dentro del V2** para "agregar paginación":
el legado ya arma su propio resultset completo sin soporte de `OFFSET/FETCH`; hubo que replicar
la consulta subyacente completa en el V2 para paginar de verdad.

**Divergencias deliberadas del V2 respecto al legado** (documentadas en el propio `.sql` y en
`task_produccion_liquidos.md`):
1. El legado limitaba a `TOP 50` cuando no había ningún filtro (workaround por falta de
   paginación real). El V2 no lo replica: sin filtros pagina **todos** los movimientos, tal
   como pide la HU ("sin filtros, el reporte trae todos los movimientos (paginados)").
2. El legado forzaba `@fechaIni`/`@fechaFin` a la fecha actual si **solo una** llegaba `NULL`
   (otro workaround de UI). El V2 las trata como filtros de rango **independientes** — si una
   es `NULL`, simplemente no se aplica esa cota.
3. Sí se preserva: `@idTipoMovInventario` `NULL`/`0` → `idTipoMovInventario IN (26,27)`
   (tipos de movimiento de carga de líquidos); `>0` → coincidencia exacta. El repository de la
   API siempre manda `NULL` (igual que el legado, que no expone el filtro en el form).

## Archivos
`Models/Entities/CargaMercanciaLiquidos.cs`, `Models/Dtos/ProduccionLiquidosQuery.cs`,
`Repositories/ProduccionLiquidos/*`, `Services/ProduccionLiquidos/*`,
`Controllers/ProduccionLiquidosController.cs`. DI en `Program.cs`.

## Sin catálogos propios
Roles/usuarios para los filtros del front se consumen del módulo Usuarios ya migrado
(`GET /api/usuarios/catalogos/roles`, `GET /api/usuarios?idRol=`); no se duplican aquí.

Relacionado: [[modulo-productos]], [[dapper-mapeo-columnas]], [[bd-local-desarrollo]].
