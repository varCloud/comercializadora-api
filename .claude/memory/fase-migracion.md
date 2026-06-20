---
name: fase-migracion
description: Fase actual del proyecto comercializadora-api — migración del back-end legado
metadata:
  type: project
---

Desde 2026-06-19 el trabajo en `comercializadora-api` está en **fase de migración**: se porta
todo el back-end del proyecto legado a este repo .NET 10. No se construyen features nuevas
mientras dure la migración.

- **Origen (legacy):** `E:\Documents\GitHub\comercializadora\lluviaBackEnd` (ASP.NET sobre .NET
  Framework; ~28 controllers, ~24 DAOs, ~60 models).
- **Destino:** `comercializadora-api` (.NET 10, Controllers + Swagger).
- **Alcance:** todo lo relacionado con back-end.

Equivalencias clave: `XxxDAO` → `XxxRepository` (+ interfaz); `ConstructorDapper` → `BaseRepository`;
métodos síncronos → `async`; `System.Data.SqlClient` → `Microsoft.Data.SqlClient`.

Ver [[arquitectura-datos]] para el patrón de la capa de datos.
