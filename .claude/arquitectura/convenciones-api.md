# Convenciones de la API (comercializadora-api)

Reglas transversales para los endpoints. Complementan
[`patron-repository.md`](patron-repository.md). Aplican a **todo** lo que migremos.

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
