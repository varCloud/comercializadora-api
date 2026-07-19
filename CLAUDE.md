# CLAUDE.md

Guía base para trabajar en este repositorio con Claude Code.

## Proyecto

- **Nombre:** `comercializadora-api`
- **Tipo:** ASP.NET Core Web API (estilo *Controllers* + Swagger).
- **Framework:** .NET 10 (`net10.0`).
- **Estado:** Plantilla base limpia. El código de dominio anterior (Usuarios,
  Repository/UnitOfWork, EF Core) fue eliminado para reconstruir desde cero.
- **Namespace raíz:** `comercializadora_api`.

## 🚧 Fase actual: MIGRACIÓN

Estamos **migrando el backend legado** a este proyecto. Mientras dure esta fase,
el trabajo se centra en portar funcionalidad, no en construir features nuevas.

- **Origen (legacy):** `E:\Documents\GitHub\comercializadora\lluviaBackEnd`
  (ASP.NET sobre .NET Framework; ~28 controllers, ~24 DAOs, ~60 models).
- **Destino:** este repo (`comercializadora-api`, .NET 10).
- **Alcance:** todo lo relacionado con **back-end**.
- **Acceso a datos:** **NO se usa Entity Framework.** Se usan **stored procedures**
  accedidos con **Dapper**, igual que en el legado. La capa de datos sigue el
  **patrón Repository ligero** documentado en `.claude/arquitectura/patron-repository.md`.
- El legado ya usa SP + Dapper (`ConstructorDapper`) y un envoltorio `Notificacion<T>`
  (`status`/`mensaje`/`modelo`); esa convención se conserva.
- Equivalencias clave: `XxxDAO` → `XxxRepository` (+ interfaz); sin Unit of Work;
  métodos `async`; `Microsoft.Data.SqlClient` en lugar de `System.Data.SqlClient`.

## Stack y convenciones

- **Lenguaje:** C# con `Nullable` e `ImplicitUsings` habilitados.
- **API:** controladores en `Controllers/` heredando de `ControllerBase` con `[ApiController]`.
- **Documentación:** Swagger / Swashbuckle, disponible en `/swagger` solo en `Development`.
- **Configuración:** `appsettings.json` (base) y `appsettings.Development.json` (local).
  - ⚠️ **Nunca** commitear secretos ni cadenas de conexión con credenciales reales.
    Usar *User Secrets* (`dotnet user-secrets`) o variables de entorno.

## Estructura actual

```
comercializadora-api/
├── Controllers/        # Endpoints HTTP (WeatherForecastController de ejemplo)
├── Properties/         # launchSettings.json (perfiles de ejecución)
├── Program.cs          # Composición de la app y pipeline HTTP
├── WeatherForecast.cs  # Modelo de ejemplo de la plantilla
├── appsettings*.json   # Configuración
└── comercializadora-api.sln
```

## Comandos habituales

```powershell
dotnet restore                      # restaurar paquetes
dotnet build                        # compilar
dotnet run --launch-profile https   # ejecutar (HTTPS)
dotnet watch run                    # ejecutar con hot reload
```

URLs en desarrollo (ver `Properties/launchSettings.json`):
- HTTP  → http://localhost:5163/swagger
- HTTPS → https://localhost:7285/swagger

## Reglas para Claude

1. **Mantener el estilo base** del proyecto (controladores + Swagger) salvo que se pida lo contrario.
2. **Target framework `net10.0`**: no degradar a versiones anteriores.
3. **No introducir secretos** en archivos versionados.
4. **Antes de borrar** código o archivos, confirmar el alcance con el usuario.
5. **Verificar con `dotnet build`** tras cambios estructurales (0 warnings como objetivo).
6. Agregar dependencias con `dotnet add package` y fijar versiones explícitas en el `.csproj`.
7. Respetar las convenciones de nombres de C# (PascalCase para tipos/métodos, camelCase para locales).

## Arquitectura

- Capa de datos: **Repository + Dapper + Stored Procedures** (sin EF). Detalle completo en
  [`.claude/arquitectura/patron-repository.md`](.claude/arquitectura/patron-repository.md).
- Flujo: Controllers → Services → Repositories → SP (SQL Server).
- Convenciones de endpoints (verbos HTTP correctos, controladores devuelven la entidad no
  `IActionResult`, JSON camelCase global) en
  [`.claude/arquitectura/convenciones-api.md`](.claude/arquitectura/convenciones-api.md).
- Exportación transversal a Excel (regla de umbral descarga-vs-correo, cola en memoria,
  gap de correo de usuario) en
  [`.claude/arquitectura/exportacion-reportes.md`](.claude/arquitectura/exportacion-reportes.md).

## Agentes

Subagentes invocables (tool `Agent`) en [`.claude/agents/`](.claude/agents/) (índice en su
`README.md`): `migrador-modulo-backend` → `revisor-backend` para portar módulos del legado
`lluviaBackEnd` un módulo a la vez.

## Próximos pasos

- Crear piezas base de la capa de datos (`IDbConnectionFactory`, `BaseRepository`, `Notificacion<T>`).
- Migrar entidades/DAOs del legado a Repositories uno por uno.
- Autenticación / autorización (portar `LoginDAO` / `SesionDAO`).
- Pruebas (proyecto de tests).
