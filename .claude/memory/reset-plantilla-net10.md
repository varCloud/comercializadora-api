---
name: reset-plantilla-net10
description: El proyecto fue reseteado a plantilla base y migrado a .NET 10; credencial filtrada en git
metadata:
  type: project
---

El 2026-06-19 `comercializadora-api` se reseteó a la plantilla base (Controllers + Swagger) y se
actualizó de `net8.0` a `net10.0`. Se eliminó todo el código de dominio anterior (UsuariosController,
TestController, ApplicationDbContext/EF Core, Repository, Services, UnitofWork, Models).

⚠️ **Seguridad:** el commit inicial `1314284` contenía una cadena de conexión con credenciales
reales (`Server=SQL5063.site4now.net`, password `Abcde12345`) en `appsettings.json`. Se removió del
archivo pero **sigue en el historial git**; debe rotarse la contraseña y purgarse el historial.

Ver [[arquitectura-datos]] y [[fase-migracion]].
