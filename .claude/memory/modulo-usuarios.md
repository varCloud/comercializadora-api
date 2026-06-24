# Módulo Usuarios (migrado)

Migración del módulo de Usuarios del legado (`UsuariosController` + `UsuarioDAO`). 2026-06-20.

## Qué se hizo
- **Entidades/DTOs:** `Models/Entities/Usuario.cs` (`Contrasena` con `[JsonIgnore]`),
  `CatalogoItem.cs`; `Models/Dtos/GuardarUsuarioRequest.cs`, `CambiarEstatusRequest.cs`;
  `Models/Common/PagedResult.cs`.
- **Datos:** `IUsuariosRepository` + `UsuariosRepository` (Dapper). Listado vía
  `SP_V2_CONSULTA_USUARIOS` (multi-resultset: header status/mensaje/total + página);
  CRUD y catálogos reutilizan los SP legados sin tocarlos.
- **Negocio:** `IUsuariosService` + `UsuariosService` (Title Case nombre/apellidos;
  en edición, si la contraseña llega vacía, conserva la actual leyéndola del repo).
- **API:** `UsuariosController` (`[Authorize]`, ruta `api/usuarios`).
- **DI:** registrados en `Program.cs`.

## Endpoints (api/usuarios, todos [Authorize])
- `GET /` (pageNumber,pageSize,idRol,idAlmacen) → `Notificacion<PagedResult<Usuario>>`
- `GET /{id}` → `Notificacion<Usuario>`
- `POST /` · `PUT /{id}` (`GuardarUsuarioRequest`) → `Notificacion<string>`
- `PATCH /{id}/estatus` (`{activo}`) → `Notificacion<string>` (borrado lógico)
- `GET /catalogos/roles|sucursales|almacenes` → `Notificacion<IEnumerable<CatalogoItem>>`

## SP
- **Nuevo:** `SP_V2_CONSULTA_USUARIOS` (en `store-procedures/`, `CREATE OR ALTER`, idempotente).
  Paginación `OFFSET/FETCH`. NO se modificó `SP_CONSULTA_USUARIOS` (lo usa el legado). Ver
  convención SP_V2 + paginación en la memoria raíz del workspace.
- **Reusados sin cambios:** `SP_INSERTA_ACTUALIZA_USUARIOS`, `SP_ACTUALIZA_STATUS_USUARIO`,
  `SP_CONSULTA_ROLES` (requiere `@idRol=1`), `SP_CONSULTA_SUCURSALES`, `SP_CONSULTA_ALMACENES`.

## Probado
End-to-end contra BD local (`SQLEXPRESS01`): listar paginado (total 18), obtener, crear,
editar (contraseña vacía = conserva; Title Case aplicado), desactivar, catálogos; 401 sin token.

## Búsqueda
`GET /api/usuarios` acepta `search` (`[FromQuery] string?`) → `@search` en
SP_V2_CONSULTA_USUARIOS (LIKE por nombre/apellidos/usuario). Regla transversal: todo listado
lleva buscador (front + back).

## Organización por feature (convención)
`Repositories/` y `Services/` se organizan en **carpetas por feature** con el namespace
siguiendo a la carpeta: `Repositories/Auth`, `Repositories/Usuarios`, `Services/Auth`,
`Services/Usuarios` (`comercializadora_api.{Repositories|Services}.<Feature>`).
`Repositories/Base` es infraestructura compartida. Documentado en `patron-repository.md` §4.

## Pendientes / deuda
- Contraseña en texto plano (heredado); hashing = otra HU.
- Autorización solo `[Authorize]`; permiso fino (`Puede_visualizar_Usuarios`) = otra HU.

Relacionado: [[dapper-mapeo-columnas]], [[bd-local-desarrollo]], [[convenciones-endpoints]].
