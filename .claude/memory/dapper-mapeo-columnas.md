# Dapper: mapeo de columnas de SP (gotchas)

Al envolver SP con Dapper en este repo aparecieron dos detalles de mapeo que conviene recordar:

## 1. Dapper NO estripa guiones bajos por defecto
`MatchNamesWithUnderscores` está desactivado, así que una columna `fecha_alta` **no** mapea a
la propiedad `FechaAlta` (queda en null silenciosamente). Solución usada en `SP_V2_CONSULTA_USUARIOS`:
**aliasar la columna en el SP** (`u.fecha_alta AS fechaAlta`). El mapeo case-insensitive sí
funciona (`idUsuario` → `IdUsuario`), solo falla por el `_`.

## 2. Nombre de propiedad = nombre de la clase/tipo no se permite en C# (CS0542)
La entidad `Usuario` no puede tener una propiedad `Usuario`. Se usó `NombreUsuario` con
`[JsonPropertyName("usuario")]` (conserva el contrato JSON `usuario`) y el SP aliasa la
columna como `nombreUsuario` para que Dapper la mapee.

**Variante (Dashboard):** el conflicto también ocurre cuando la propiedad coincide con su
**tipo**, no con la clase contenedora. La entidad `Categoria` no puede tener una propiedad
`Categoria` (CS0542). Se usó `NombreCategoria` + `[JsonPropertyName("categoria")]`. Como los
SP del dashboard **no se modifican** (regla del proyecto: reusar SP sin tocar), aquí **no**
se pudo aliasar la columna en el SP; en su lugar el `DashboardRepository` **proyecta a mano**
los resultsets de `Categoria` (helper `ConsultarCategoriasAsync` con
`Dictionary<string,object>` + `StringComparer.OrdinalIgnoreCase`, mismo enfoque que el punto 3).
Regla práctica: si puedes tocar el SP → aliasa la columna; si no → proyecta a mano en el repo.

## 3. SP legados con status/datos en el MISMO resultset → lookup case-sensitive
Los SP de catálogo legados (`SP_CONSULTA_ROLES/SUCURSALES/ALMACENES`) devuelven un solo
resultset donde la primera fila trae `status`/`mensaje` junto con los datos (no header
separado como `BaseRepository.ConsultarAsync`). Además los nombres de columna varían en
mayúsculas entre tablas (p. ej. `Almacenes.Descripcion` vs `catRoles.descripcion`). El
`DapperRow` indexado con string es **case-sensitive**: se normaliza cada fila a
`Dictionary<string,object>(..., StringComparer.OrdinalIgnoreCase)` antes de proyectar
(ver `UsuariosRepository.ConsultarCatalogoAsync`).

## 4. Cabecera de los SP de escritura: usar columna `status` (minúscula)
`BaseRepository.EjecutarAsync` lee `fila.status` (acceso dinámico de Dapper, **case-sensitive**
en la práctica para columnas dinámicas). Si el SP devuelve la cabecera como `Estatus`
(PascalCase) o `estatus`, `fila.status` resuelve a **null** → `Cannot convert null to 'int'`.
- Los SP legados de Productos lo hacían distinto: `SP_INSERTA_ACTUALIZA_PRODUCTOS` devuelve
  `Estatus` y `SP_APP_CONSULTA_PRODUCTOS_POR_DESCRIPCION` / `SP_CONSULTA_PRODUCTOS_POR_CODIGO_BARRAS`
  devuelven `estatus`.
- **Al crear un SP_V2** (que sí podemos definir): emitir la cabecera como `status` minúscula
  (como `SP_V2_INSERTA_ACTUALIZA_PRODUCTOS`).
- **Al reusar un SP legado sin tocarlo** cuya cabecera es `estatus`: leerlo a mano en el repo
  con `(int)cabecera.estatus` (ver `ProductosRepository.BuscarPorDescripcionAsync` /
  `ObtenerPorCodigoAsync`), no con los helpers `ConsultarAsync`/`ConsultarUnicoAsync`.

Relacionado: convenciones de endpoints, patrón Repository.
