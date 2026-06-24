# Convenciones de la API (comercializadora-api)

Reglas transversales para los endpoints. Complementan
[`patron-repository.md`](patron-repository.md).

> **Alcance: permanentes, no solo de migración.** Estas convenciones (verbos HTTP correctos,
> controladores devuelven la entidad/`Notificacion<T>`, JSON camelCase, **todo listado paginado +
> búsqueda**) son el estándar del proyecto y aplican a **cualquier desarrollo futuro**, sea
> migración o feature nueva. Lo que es *mecánica de migración* (crear `SP_V2_…` para no tocar el SP
> que el legado aún usa) se marca como tal: en desarrollo nuevo el SP nace ya paginado.

## 1. Verbos HTTP correctos (el legado los usaba mal)

El back-end legado exponía casi todo como POST/GET indistintamente. **Al migrar, corrige el
verbo** según la semántica de la operación:

| Operación | Verbo | Ejemplo |
|---|---|---|
| Consultar / listar | **GET** | `GET /api/clientes`, `GET /api/clientes/{id}` |
| Crear | **POST** | `POST /api/clientes` |
| Reemplazar (completo) | **PUT** | `PUT /api/clientes/{id}` |
| Actualizar (parcial) | **PATCH** | `PATCH /api/clientes/{id}` |
| Eliminar | **DELETE** | `DELETE /api/clientes/{id}` |
| Acción/comando sin recurso CRUD | **POST** | `POST /api/auth/login` |

- Usa `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, `[HttpDelete]` con rutas claras.
- GET nunca lleva body; los filtros van por query string o ruta.
- Si el SP legado mezclaba operaciones, **divídelas** en endpoints con el verbo correcto.
- Mantén el SP tal cual (no se reescribe); lo que cambia es **cómo se expone** en la API.

## 2. Los controladores devuelven la entidad, no `IActionResult`

Siempre que sea posible, las acciones del controlador **devuelven el modelo/entidad**
(o `Notificacion<T>`), no `IActionResult`:

```csharp
[HttpPost("login")]
public Task<Notificacion<Sesion>> Login([FromBody] LoginRequest login)
    => _authService.LoginAsync(login);

[HttpGet("{id}")]
public Task<Notificacion<Cliente>> ObtenerPorId(int id)
    => _clientesService.ObtenerPorIdAsync(id);
```

- El resultado de negocio (éxito/error) viaja en `Notificacion<T>` (`Estatus`/`Mensaje`),
  no en el código HTTP. Así el front siempre parsea la misma forma.
- `[ApiController]` sigue devolviendo **400** automáticamente ante `ModelState` inválido, y
  los endpoints `[Authorize]` siguen devolviendo **401** vía middleware. Eso es correcto.
- Usa `IActionResult`/`ActionResult<T>` **solo** cuando realmente necesites controlar el
  código HTTP (p. ej. devolver `201 Created` con `Location`, o `204 No Content`).

## 3. Todas las respuestas en JSON camelCase

Configurado globalmente en `Program.cs`:

```csharp
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});
```

- Los modelos en C# se nombran en **PascalCase** (`IdUsuario`); la API los serializa a
  **camelCase** (`idUsuario`) automáticamente. **No** uses `[JsonPropertyName]` para forzar
  nombres salvo que el contrato lo exija.
- El front consume directamente esos nombres camelCase (ver regla front
  `09-modelos-interface-clase.md`).

## 4. Listados: paginación + búsqueda

Todo endpoint de **listado** se expone paginado y con búsqueda. Forma de la respuesta:

- **`Notificacion<IEnumerable<T>>` con `links` y `meta`** junto a `estatus`/`mensaje`; los
  datos van en `modelo` (camelCase). `links`/`meta` se **omiten** cuando son null
  (`[JsonIgnore(WhenWritingNull)]`), así los endpoints NO-listado quedan igual que siempre.
  - `modelo`: filas de la página.
  - `links`: `{ first, last, prev, next }` — URLs absolutas; `prev`/`next` null en los extremos.
  - `meta`: `{ currentPage, from, lastPage, path, perPage, to, total }`.
  El front navega usando `links` (no recompone el número de página).
- **Query params como UN objeto** con `[FromQuery]` (no muchos parámetros sueltos en la firma):
  `PagedQuery` (`page`, `perPage`, `q`, `order`, `sort`); los listados con filtros extra
  heredan (`UsuariosQuery : PagedQuery` con `idRol`/`idAlmacen`). Ejemplo:
  `public async Task<Notificacion<IEnumerable<T>>> Listar([FromQuery] XQuery query)`.
- **Flujo**: el repositorio/servicio devuelve `RawPage<T>` (solo `Items` + `Total`) vía el
  helper `BaseRepository.ConsultarPaginaAsync<T>`; el controller arma la `Notificacion` con
  `modelo`/`links`/`meta` usando `IPaginationBuilder` (inyectado), que conoce la URL pública
  y la query de la request.
- **Dominio para los links**: `App:PublicBaseUrl` en `appsettings.{Environment}.json` (dev/prod).
  Si falta, se usa el host de la request.
- **SP**: `OFFSET/FETCH`, dos resultsets (cabecera `status/mensaje/total` + página), `@search`
  (`LIKE '%'+@search+'%'`, null/vacío = sin filtro) y orden opcional `@order`/`@sort`
  (whitelist por columna). Si el legado aún usa el SP, **crea un `SP_V2_…`** idempotente en
  `store-procedures/` en vez de alterarlo (memoria raíz `convencion-sp-v2`).

Ejemplo: `GET /api/proveedores?page=&perPage=&q=&order=&sort=` →
`{ "estatus":200, "mensaje":"OK", "modelo":[…], "links":{…}, "meta":{…} }`.
(Listados migrados: Usuarios, Estaciones, Proveedores.)
