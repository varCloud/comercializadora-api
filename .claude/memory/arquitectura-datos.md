---
name: arquitectura-datos
description: Decisión de capa de datos — Stored Procedures + Dapper + Repository ligero, sin EF
metadata:
  type: project
---

La capa de datos de `comercializadora-api` **NO usa Entity Framework**. Usa **Stored Procedures**
accedidos con **Dapper**, bajo un **patrón Repository ligero**.

**Why:** el legado ya usa SP + Dapper (`ConstructorDapper`) con envoltorio `Notificacion<T>`
(status/mensaje/modelo); migrar conservando esa filosofía reduce fricción. El argumento clásico
del Repository (ocultar `DbContext`, cambiar de ORM) no aplica, pero el patrón sigue dando valor:
centraliza el boilerplate de SP, mantiene Controllers/Services limpios y permite mockear datos.

**How to apply:**
- Flujo: Controllers → Services → Repositories → SP (SQL Server).
- Piezas base ya creadas: `Models/Common/Notificacion.cs`, `Repositories/Base/IDbConnectionFactory.cs`,
  `SqlConnectionFactory.cs`, `BaseRepository.cs` (métodos `ConsultarAsync`, `EjecutarAsync`,
  `ConsultarUnicoAsync`). Registrado en `Program.cs`: `IDbConnectionFactory` como Singleton.
- Un repositorio por entidad, con interfaz; métodos con nombre de negocio que envuelven un SP.
- NO usar `IRepository<T>` genérico CRUD (asume semántica EF). NO usar Unit of Work.
- Paquetes: Dapper 2.1.79, Microsoft.Data.SqlClient 7.0.1.
- Doc completo en `.claude/arquitectura/patron-repository.md`.

Relacionado con [[fase-migracion]].
