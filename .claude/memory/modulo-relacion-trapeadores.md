# Módulo Relación Trapeadores migrado

CRUD del catálogo de combinaciones de producción de trapeadores (Materia Prima/Matra +
Bastón → Trapeador producido, con Unidad de Medida y Cantidad). Endpoints
`api/relacion-trapeadores` `[Authorize]`: `GET` (paginado), `GET {id}`, `GET
unidades-medida`, `POST`, `PUT {id}`, `DELETE {id}` (baja lógica). Migra
`ProduccionProductosController`/`ProduccionProductosDAO` del legado (menú "Relación
Trapeadores", distinto de "Producción Trapeadores" que no se migró — es otro módulo, ver HU).

**Listado** vía `SP_V2_CONSULTA_COMBINACION_PRODUCCION_PRODUCTOS` (nuevo, paginado, JOIN x3 a
Productos). **Guardar/desactivar/unidades de medida** reutilizan los SP legados exactos
(`SP_AGREGA_ACTUALIZA_COMBINACION_PRODUCCION_PRODUCTOS`,
`SP_DESACTIVAR_COMBINACION_PRODUCTOS_PRODUCCION`,
`SP_OBTENER_UNIDADES_DE_MEDIDA_TRAPEADORES`), cuya cabecera viene en la columna `estatus`
(no `status`) — se leen manualmente, mismo patrón que `RelacionLiquidosRepository`.

**Gotcha de fidelidad al legado:** `SP_DESACTIVAR_COMBINACION_PRODUCTOS_PRODUCCION` solo
acepta `@idProductoProduccion` (NO el id propio de la relación) — igual que el JS legado
(`EliminarRelacion(item.idProductoProduccion)`). Para mantener el contrato REST público
limpio (`DELETE /api/relacion-trapeadores/{id}` con el id de la relación),
`RelacionTrapeadoresService.DesactivarAsync(id)` resuelve primero la relación
(`ObtenerPorIdAsync`) y llama al repositorio con el `IdProductoProduccion` ya resuelto; si no
la encuentra, devuelve `Notificacion { Estatus = 404 }` sin llamar al SP.

**Riesgo abierto:** el esquema de la tabla del `SP_V2_CONSULTA_COMBINACION_PRODUCCION_PRODUCTOS`
(nombre `ProduccionProductos` y columnas) se infirió del modelo legado
(`ProduccionProductosModel`), sin visibilidad real de la BD — pendiente validar con el DBA
antes de desplegar el SP nuevo (mismo tratamiento que se le dio al SP de Relación Líquidos).

Detalle completo en `.claude/docs/feature/relacion_trapeadores/` del workspace raíz.
