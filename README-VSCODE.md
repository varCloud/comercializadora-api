# Configurar VS Code para C# (.NET 10) — `comercializadora-api`

Guía para abrir, compilar, ejecutar y depurar esta solución de **Visual Studio 2022**
(`comercializadora-api.sln`) usando **Visual Studio Code** en Windows.

> Proyecto: ASP.NET Core Web API · **.NET 10** · Swagger (plantilla base, sin EF Core por ahora).

---

## 1. Requisitos previos (instalar en este orden)

| Herramienta | Para qué sirve | Descarga / comando |
|---|---|---|
| **.NET SDK 10.0** | Compilar y ejecutar el proyecto (`TargetFramework: net10.0`) | https://dotnet.microsoft.com/download/dotnet/10.0 |
| **Visual Studio Code** | Editor | https://code.visualstudio.com |
| **Git** | Control de versiones | https://git-scm.com |

Verifica que el SDK quedó instalado:

```powershell
dotnet --version        # debe mostrar 10.0.x
dotnet --list-sdks
```

> ⚠️ No basta con el *Runtime*. Necesitas el **SDK 10.0** para compilar.

---

## 2. Extensiones de VS Code (obligatorias y recomendadas)

Instálalas desde la pestaña de Extensiones (`Ctrl+Shift+X`) o por línea de comandos:

```powershell
# Imprescindibles
code --install-extension ms-dotnettools.csdevkit          # C# Dev Kit (incluye C# + IntelliCode + explorador de soluciones .sln)
code --install-extension ms-dotnettools.csharp            # Lenguaje C# (servidor de lenguaje / OmniSharp - Roslyn)

# Muy recomendadas para este proyecto
code --install-extension ms-dotnettools.vscode-dotnet-runtime
code --install-extension humao.rest-client                # Probar el archivo comercializadora-api.http
code --install-extension ms-mssql.mssql                   # Conectar y consultar SQL Server desde VS Code
```

- **C# Dev Kit** es la clave: añade la vista **Solution Explorer**, por lo que puedes
  abrir el `.sln` igual que en Visual Studio 2022 (build, agregar clases, gestionar proyectos).
- Requiere una cuenta Microsoft/Visual Studio para activarse (gratis para uso individual,
  educación y open source).

---

## 3. Abrir la solución

1. Abre la carpeta raíz del proyecto en VS Code:

   ```powershell
   code "E:\Documents\BlueCloud\Proyectos\lluvia-migracion\comercializadora-api"
   ```

2. Con **C# Dev Kit** instalado, aparecerá el panel **SOLUTION EXPLORER**
   en la barra lateral, mostrando `comercializadora-api.sln`.
3. La primera vez, VS Code restaura los paquetes NuGet automáticamente. Si no:

   ```powershell
   dotnet restore
   ```

---

## 4. Compilar y ejecutar

Desde la terminal integrada (``Ctrl+` ``):

```powershell
dotnet build                       # compilar
dotnet run                         # ejecutar (perfil por defecto)
dotnet run --launch-profile https  # ejecutar con HTTPS
```

URLs del proyecto (definidas en `Properties/launchSettings.json`):

- HTTP  → http://localhost:5163/swagger
- HTTPS → https://localhost:7285/swagger

Swagger se abre automáticamente en `/swagger` en entorno `Development`.

---

## 5. Depuración (F5)

Los archivos `.vscode/launch.json` y `.vscode/tasks.json` ya están incluidos en este repo.
Con ellos puedes:

- Pulsar **F5** → compila y arranca la API con el depurador adjunto y abre el navegador en Swagger.
- Poner *breakpoints* en controladores, servicios y repositorios.

Si VS Code te pregunta por la configuración, elige **".NET Core Launch (web)"**.

---

## 6. Base de datos (pendiente)

> El proyecto está en plantilla base y **no incluye EF Core ni base de datos** por el momento.
> Cuando se reintroduzca la capa de datos, esta sección documentará el proveedor,
> la cadena de conexión y los comandos `dotnet ef`. Recuerda usar *User Secrets*
> (`dotnet user-secrets`) para credenciales en lugar de versionarlas.

---

## 7. Probar los endpoints

- **Swagger UI**: navega a `/swagger` con la app corriendo.
- **REST Client**: abre `comercializadora-api.http` y pulsa *"Send Request"*
  (requiere la extensión `humao.rest-client`).

---

## 8. Diferencias clave VS 2022 → VS Code

| Visual Studio 2022 | Equivalente en VS Code |
|---|---|
| Abrir `.sln` | C# Dev Kit → Solution Explorer |
| `Ctrl+Shift+B` (Build) | `dotnet build` / tarea `build` |
| `F5` (Depurar) | `F5` con `launch.json` |
| NuGet Package Manager | `dotnet add package` / panel NuGet del Dev Kit |
| Package Manager Console (EF) | `dotnet ef ...` en la terminal |
| IIS Express | Kestrel (`dotnet run`) |

---

## 9. Solución de problemas

- **No aparece IntelliSense / Solution Explorer**: confirma que C# Dev Kit y C# están
  habilitadas y recarga con `Ctrl+Shift+P → Developer: Reload Window`.
- **"SDK not found"**: instala el SDK 10.0 y reinicia VS Code para refrescar el `PATH`.
- **Restaurar de cero**: borra `bin/` y `obj/` y ejecuta `dotnet restore`.
- **Certificado HTTPS de desarrollo**:

  ```powershell
  dotnet dev-certs https --trust
  ```
