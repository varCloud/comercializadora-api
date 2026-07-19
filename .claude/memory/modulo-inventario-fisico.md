# Módulo Inventario Físico migrado (listado + crear/renombrar + estatus + ajustes)

Migra la pantalla web `InventarioFisicoController` + `InventarioFisicoDAO` del legado
(conteo y ajuste de existencias por sucursal). Los WS móviles (`WsInventarioFisicoController`,
`SP_APP_*`, `SP_AJUSTA_PRODUCTO_INVENTARIO_FISICO_INDIVIDUAL`, `SP_VALIDA_EXISTE_INVENTARIO_FISICO_ACTIVO`)
y el bloqueo global "InventarioFisicoActivo" quedaron fuera de alcance (HU inventario_fisico).

## Endpoints (`/api/inventario-fisico`, todos `[Authorize]`)
- `GET /` — listado paginado (`page,perPage,idTipoInventario,fechaIni,fechaFin,order,sort`);
  `order` whitelist `fecha|nombre|estatus`, default `fechaAlta DESC`. **idSucursal sale del
  claim JWT** (paridad con la sesión legada), no de la query. Ítem con `sucursal`/`estatus`
  anidados (multi-mapping splitOn `idSucursal,idStatus`; el Usuario del DAO legado no se expone).
- `POST /` body `{nombre}` — alta (SP con id=0 → estatus 1); `PUT /{id}` — renombrar (mismo SP).
- `PATCH /{id}/estatus` body `{idEstatus, observaciones}` — 2 iniciar / 3 finalizar / 4 cancelar;
  las transiciones y la afectación de inventario las valida/ejecuta el SP legado.
- `GET /{id}/ajustes?idAlmacen=&idLineaProducto=` — lista completa SIN paginar (paridad con el
  modal legado); `producto` anidado (splitOn `idProducto`) con ubicación (almacén/piso/pasillo/
  raq con "SIN ACOMODAR" resuelto por el SP).

## SPs
- **Nuevo** `SP_V2_CONSULTA_INVENTARIO_FISICO` (`store-procedures/`, CREATE OR ALTER, compat 120,
  desplegado y smoke-tested en BD local 2026-07-04: total=903 sin filtros). Divergencias
  deliberadas documentadas en el .sql: default `fechaAlta DESC` (legado: `fechaInicio DESC`),
  quita filtros `@idInventarioFisico/@idEstatus` (la pantalla nueva no los usa), agrega texto
  `tipoInventario` (CASE 1=General/2=Individual; no hay tabla catálogo).
- **Reusados sin cambios:** `SP_INSERTA_ACTUALIZA_INVENTARIO_FISICO`,
  `SP_ACTUALIZA_ESTATUS_INVENTARIO_FISICO`, `SP_CONSULTA_AJUSTE_INVENTARIO`.

## Gotchas / hallazgos de BD
- Los dos SP legados de **escritura** devuelven cabecera `Estatus`/`Mensaje` (mayúscula
  inicial, no `status/mensaje`): se leen con `EjecutarLegadoAsync` (dict case-insensitive,
  mismo criterio que el `EjecutarAppAsync` de Producción a granel). **No** sirve el
  `EjecutarAsync` de BaseRepository.
- `SP_CONSULTA_AJUSTE_INVENTARIO`: `@idLineaProducto` es **int** en BD (el DAO legado lo
  pasaba como string, pero la firma real es int). Con 0 resultados devuelve `-1` +
  "No se encontraron resultados." → el repository lo pasa tal cual con `modelo: []` (el
  legado lo traducía a lista vacía); el front debe tratarlo como tabla vacía, no error.
- Catálogo `CatEstatusInventarioFisico`: 1 **Pendiente** (no "Creado"), 2 Iniciado,
  3 Finalizado, 4 Cancelado.
- `InventarioFisico.idTipoInventarioFisico` es nullable pero el insert legado no lo setea y
  la BD lo deja en 1 (General) por default; el enum del tipo es fijo (sin tabla).

## Archivos
`Models/Entities/InventarioFisico.cs` + `Sucursal.cs` + `AjusteInventarioFisico.cs` +
`ProductoAjusteInventario.cs`, `Models/Dtos/InventarioFisicoQuery.cs` + `AjustesInventarioQuery.cs`
+ `GuardarInventarioFisicoRequest.cs` + `ActualizarEstatusInventarioFisicoRequest.cs`,
`Repositories/InventariosFisicos/*`, `Services/InventariosFisicos/*`,
`Controllers/InventarioFisicoController.cs`, DI en `Program.cs`. ⚠️ Carpeta/namespace en
**plural** (`InventariosFisicos`) para no colisionar con la entidad `InventarioFisico`
(CS0118 namespace vs tipo). `dotnet build` → 0 errores, 0 warnings (2026-07-04).

Relacionado: [[modulo-produccion-agranel]], [[dapper-mapeo-columnas]], [[bd-local-desarrollo]].
