# Patrón Repository con Stored Procedures (sin Entity Framework)

> Proyecto: `comercializadora-api` (.NET 10). Documento de arquitectura de la capa de datos.

## 1. Contexto: ¿venimos de dónde?

El backend legado (`E:\Documents\GitHub\comercializadora\lluviaBackEnd`, ASP.NET .NET Framework)
ya usa **stored procedures** accedidos con **Dapper**, organizados en clases **DAO**
(`ClienteDAO`, `ProductosDAO`, etc.) y un helper `ConstructorDapper`. Los SP devuelven un
envoltorio estándar `{ status, mensaje, [resultset] }` que se mapea a `Notificacion<T>`.

La migración a .NET 10 conserva esa filosofía: **SP en la base de datos + acceso ligero con Dapper**.
Lo que llamábamos *DAO* pasa a llamarse *Repository*.

## 2. ¿Es necesario el patrón Repository si NO uso Entity Framework?

**Respuesta corta: sí, conviene — pero una versión ligera, no la “clásica de EF”.**

El argumento típico del Repository (ocultar el `DbContext` de EF, poder cambiar de ORM) **no aplica**
aquí porque no hay ORM. Pero el patrón sigue aportando valor real con stored procedures:

| Beneficio | ¿Aplica con SP + Dapper? |
|---|---|
| Centralizar el boilerplate de conexión/llamada a SP | ✅ Sí (un `BaseRepository` lo concentra) |
| Mantener Controllers/Services libres de ADO.NET/Dapper | ✅ Sí |
| Permitir mockear la capa de datos en pruebas unitarias | ✅ Sí (interfaces por repositorio) |
| Abstraer el motor/ORM | ⚠️ Marginal (seguimos atados a SQL Server + SP) |

### Lo que SÍ hacemos
- Un **repositorio por agregado/entidad de negocio** (`ClientesRepository`, `ProductosRepository`…),
  cada uno con su **interfaz** (`IClientesRepository`).
- Un **`BaseRepository`** que encapsula la apertura de conexión y la ejecución de SP con Dapper
  (equivalente moderno de `ConstructorDapper`).
- Métodos con **nombres de negocio** que envuelven un SP concreto
  (ej. `ObtenerPorId`, `Registrar`, `ListarPaginado`), no un CRUD genérico.

### Lo que NO hacemos (anti-patrones en este contexto)
- ❌ **`IRepository<T>` genérico con `Add/Update/Delete/GetAll`**: asume semántica de EF
  (change tracking, `IQueryable`). Con SP cada operación es a medida; un CRUD genérico estorba.
- ❌ **Unit of Work**: existe para coordinar el `SaveChanges` de EF. Con SP, la transacción vive
  dentro del propio procedimiento (o se maneja con `TransactionScope`/`IDbTransaction` puntual).
  Por eso se eliminó la carpeta `UnitofWork` del proyecto.
- ❌ Exponer `DataReader`/`DynamicParameters` fuera del repositorio.

## 3. Capas de la aplicación

```
HTTP
 │
 ▼
Controllers/         → validan request, devuelven IActionResult. Sin lógica de datos.
 │
 ▼
Services/            → reglas de negocio, orquestación, transformaciones DTO.
 │
 ▼
Repositories/        → llaman a stored procedures vía Dapper. Una clase por entidad.
 │
 ▼
BaseRepository / IDbConnectionFactory  → conexión SqlConnection + ejecución de SP.
 │
 ▼
SQL Server (Stored Procedures)
```

Regla de dependencia: cada capa solo conoce la inmediatamente inferior, siempre vía **interfaz**.

## 4. Estructura de carpetas propuesta

```
comercializadora-api/
├── Controllers/                 # endpoints HTTP
├── Services/
│   ├── IClientesService.cs
│   └── ClientesService.cs
├── Repositories/
│   ├── Base/
│   │   ├── IDbConnectionFactory.cs
│   │   ├── SqlConnectionFactory.cs
│   │   └── BaseRepository.cs
│   ├── IClientesRepository.cs
│   └── ClientesRepository.cs
├── Models/
│   ├── Entities/                # entidades que mapean resultados de SP
│   ├── Dtos/                    # request/response de la API
│   └── Common/
│       └── Notificacion.cs      # envoltorio estándar status/mensaje/modelo
└── Program.cs                   # registro de DI
```

## 5. Piezas base

### 5.1 Envoltorio de respuesta de los SP (`Notificacion<T>`)

Los SP devuelven `status` (200 = ok), `mensaje` y opcionalmente un resultset.

```csharp
namespace comercializadora_api.Models.Common;

public class Notificacion<T>
{
    public int Estatus { get; set; }      // mapea "status" del SP
    public string? Mensaje { get; set; }  // mapea "mensaje" del SP
    public T? Modelo { get; set; }

    public bool EsExitoso => Estatus == 200;
}
```

### 5.2 Fábrica de conexión

```csharp
using System.Data;
using Microsoft.Data.SqlClient;

namespace comercializadora_api.Repositories.Base;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
```

> Nota: usar **`Microsoft.Data.SqlClient`** (no el viejo `System.Data.SqlClient`) en .NET 10.

### 5.3 BaseRepository (equivalente moderno de `ConstructorDapper`)

```csharp
using System.Data;
using Dapper;
using comercializadora_api.Models.Common;

namespace comercializadora_api.Repositories.Base;

public abstract class BaseRepository
{
    private readonly IDbConnectionFactory _factory;

    protected BaseRepository(IDbConnectionFactory factory) => _factory = factory;

    // Consulta que devuelve status/mensaje + un resultset (lista)
    protected async Task<Notificacion<IEnumerable<T>>> ConsultarAsync<T>(
        string storedProcedure, object? parametros = null)
    {
        using IDbConnection db = _factory.CreateConnection();
        using var multi = await db.QueryMultipleAsync(
            storedProcedure, parametros, commandType: CommandType.StoredProcedure);

        var cabecera = await multi.ReadFirstAsync();
        var notificacion = new Notificacion<IEnumerable<T>>
        {
            Estatus = (int)cabecera.status,
            Mensaje = (string?)cabecera.mensaje
        };
        if (notificacion.EsExitoso)
            notificacion.Modelo = await multi.ReadAsync<T>();

        return notificacion;
    }

    // Ejecución (insert/update/delete) que solo devuelve status/mensaje
    protected async Task<Notificacion<string>> EjecutarAsync(
        string storedProcedure, object? parametros = null)
    {
        using IDbConnection db = _factory.CreateConnection();
        return await db.QuerySingleAsync<Notificacion<string>>(
            storedProcedure, parametros, commandType: CommandType.StoredProcedure);
    }
}
```

### 5.4 Repositorio concreto

```csharp
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;
using Dapper;

namespace comercializadora_api.Repositories;

public interface IClientesRepository
{
    Task<Notificacion<IEnumerable<Cliente>>> ListarAsync(int idSucursal);
    Task<Notificacion<string>> RegistrarAsync(Cliente cliente);
}

public sealed class ClientesRepository : BaseRepository, IClientesRepository
{
    public ClientesRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<Notificacion<IEnumerable<Cliente>>> ListarAsync(int idSucursal)
    {
        var p = new DynamicParameters();
        p.Add("@idSucursal", idSucursal);
        return ConsultarAsync<Cliente>("SP_CLIENTES_LISTAR", p);
    }

    public Task<Notificacion<string>> RegistrarAsync(Cliente cliente)
    {
        var p = new DynamicParameters();
        p.Add("@nombre", cliente.Nombre);
        p.Add("@telefono", cliente.Telefono);
        return EjecutarAsync("SP_CLIENTES_REGISTRAR", p);
    }
}
```

### 5.5 Registro en DI (`Program.cs`)

```csharp
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IClientesRepository, ClientesRepository>();
builder.Services.AddScoped<IClientesService, ClientesService>();
```

## 6. Convenciones de migración (legacy → nuevo)

| Legacy (`lluviaBackEnd`) | Nuevo (`comercializadora-api`) |
|---|---|
| `XxxDAO.cs` | `XxxRepository.cs` + `IXxxRepository.cs` |
| `ConstructorDapper` | `BaseRepository` |
| `Notificacion<T>` (status/mensaje) | `Notificacion<T>` (Estatus/Mensaje/Modelo) |
| `ConfigurationManager.AppSettings["conexionString"]` | `IConfiguration.GetConnectionString("DefaultConnection")` |
| `System.Data.SqlClient` | `Microsoft.Data.SqlClient` |
| Métodos síncronos | `async`/`await` (`...Async`) |
| `catch { throw ex; }` | dejar propagar / middleware de excepciones |

## 7. Paquetes NuGet necesarios

```powershell
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
```

> No se agrega `Microsoft.EntityFrameworkCore.*`. **No usamos EF.**

## 8. Reglas de oro

1. Los **stored procedures son la única fuente de SQL**; el C# nunca arma SQL a mano.
2. Cada SP nuevo se documenta (nombre, parámetros, resultsets) junto a su repositorio.
3. Repositorios devuelven `Notificacion<T>`; los Services deciden el `IActionResult`.
4. Todo método de datos es `async`.
5. Nada de secretos en `appsettings.json`: usar User Secrets / variables de entorno.
