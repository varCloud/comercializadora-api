# Módulo Productos migrado (Fase A)

CRUD core del catálogo maestro de productos. Endpoints `api/productos` `[Authorize]` (sin
restricción por rol). Es prerequisito del módulo Compras (búsqueda de producto).

## Endpoints
- `GET    /api/productos?page=&perPage=&q=&idLineaProducto=&order=&sort=` → listado paginado
  (`Notificacion<IEnumerable<Producto>>` con links/meta).
- `GET    /api/productos/{id}` → producto por id.
- `POST   /api/productos` / `PUT /api/productos/{id}` → alta/edición (`GuardarProductoRequest`).
- `PATCH  /api/productos/{id}/estatus` → activar/desactivar (`{ activo }`).
- `GET    /api/productos/buscar?descripcion=` → autocomplete para Compras (trae fraccion).
- `GET    /api/productos/por-codigo?codigo=` → lectura exacta por código de barras.
- `GET    /api/productos/claves-sat?q=&page=&perPage=` → búsqueda servidor de claves SAT.
- `GET    /api/productos/catalogos/{lineas|unidades-medida|unidades-compra}` → catálogos.

## Stored procedures
- **Nuevos (`store-procedures/`, CREATE OR ALTER):**
  - `SP_V2_CONSULTA_PRODUCTOS` — catálogo puro paginado (OFFSET/FETCH), `@search`
    (descripción/artículo/código de barras), `@idLineaProducto`, orden whitelist
    (descripcion|articulo), `@idProducto>0` = by-id; `activo=1`; 2 resultsets.
  - `SP_V2_INSERTA_ACTUALIZA_PRODUCTOS` — **artículo y código de barras SEPARADOS** (el SP
    legado igualaba `codigoBarras=@articulo`); valida código de barras único entre activos;
    cabecera `status` minúscula.
  - `SP_V2_CONSULTA_CATALOGOS_PRODUCTO @tipo` — unifica líneas/unidades-medida/unidades-compra
    en forma uniforme (cabecera + id/descripcion), porque los SP legados de estos catálogos
    tienen 3 formas distintas (1 resultset con status en fila; 2 resultsets; sin cabecera).
  - `SP_V2_CONSULTA_CLAVES_SAT` — búsqueda servidor en `FactCatClaveProdServicio` (52 511
    filas). NO filtra por `activo` (solo 177 son activo=1; el legado tampoco filtra).
- **Reusados sin tocar:** `SP_ACTUALIZA_STATUS_PRODUCTOS` (bloquea baja con existencias en
  `InventarioDetalle`), `SP_APP_CONSULTA_PRODUCTOS_POR_DESCRIPCION` y
  `SP_CONSULTA_PRODUCTOS_POR_CODIGO_BARRAS` (cabecera `estatus` → leídos a mano en el repo).

## Datos / esquema verificados (BD DB_A57E86_comercializadora, 2026-06-21)
- Tabla `Productos` tiene `articulo varchar(100)` y `codigoBarras varchar(max)` SEPARADOS;
  `claveProdServ varchar(1000)` guarda la **cadena SAT** (no id). 1 948 productos activos.
- Catálogos: `LineaProducto` (28), `CatUnidadMedida` (5), `CatUnidadCompra` (4, idUnidadCompra
  es `bigint`), `FactCatClaveProdServicio` (52 511; `activo` es `int`). `dbo.LineaProductoFraccion` existe.

## Archivos
`Models/Entities/{Producto,ClaveSat}.cs`, `Models/Dtos/{GuardarProductoRequest,ProductosQuery}.cs`,
`Repositories/Productos/*`, `Services/Productos/*`, `Controllers/ProductosController.cs`, DI en
`Program.cs`. Reusa `CambiarEstatusRequest`, `CatalogoItem`.

## Submenú "Líneas de producto" (módulo dedicado, 2026-06-22)
CRUD de mantenimiento del catálogo `LineaProducto` (28 reg, 20 activas; `descripcion varchar(50)`).
Módulo **propio** (no anidado en Productos): `LineasProductoController` (`api/lineas-producto`,
`[Authorize]`), repo/service dedicados, entity `LineaProducto`, DTO `GuardarLineaProductoRequest`
(descripcion Required/MaxLength 50). Listado con `PagedQuery` directo. Endpoints: GET listado
paginado, GET {id}, POST, PUT {id}, PATCH {id}/estatus. Reusa `CambiarEstatusRequest`.
- **SP nuevos** (`store-procedures/`, CREATE OR ALTER, probados en BD):
  - `SP_V2_CONSULTA_LINEAS_PRODUCTO` — paginado + `@search` (descripcion) + by-id; solo `activo=1`.
  - `SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO` — unicidad de descripción con **error explícito**
    (-1) ante duplicado; el legado `SP_INSERTA_ACTUALIZA_LINEAS_PRODUCTO` hacía no-op silencioso
    con status 200 ("sin modificaciones").
  - `SP_V2_ACTUALIZA_STATUS_LINEAS_PRODUCTO` — **bloquea baja** si hay productos `activo=1` con esa
    `idLineaProducto`; el SP legado `SP_ACTUALIZA_STATUS_LINEAS_PRODUCTO` NO validaba.
- Front: submenú bajo Productos (grupo desplegable). Ver `.claude/docs/feature/productos/salida_productos_lineas.md`.

## Pendiente (fases B–F)
Precios/rangos, límites de inventario (+Excel), ubicaciones, códigos de barras/impresión,
líquidos/producción/granel. Ver `.claude/docs/feature/productos/task_productos.md`.
