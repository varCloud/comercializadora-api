---
name: subagentes-migracion
description: Subagentes invocables en .claude/agents/ para migrar módulos del legacy lluviaBackEnd
type: decision
---

Se crearon **subagentes invocables** (tool `Agent`) en `.claude/agents/`:
`migrador-modulo-backend` → `revisor-backend`, para portar módulos del back-end legado
`lluviaBackEnd` (.NET Framework, DAO + entidades + SP) a este repo (.NET 10), un módulo a
la vez, siguiendo el patrón Repository + Dapper + Stored Procedures.

**Por qué:** automatizar la migración módulo por módulo con un flujo migrar→revisar
reproducible. Índice en `.claude/agents/README.md`.

**Cómo aplicar:** al migrar un módulo (Clientes, Productos, Login…), invoca
`migrador-modulo-backend` y luego `revisor-backend`. Ambos exigen respetar
[[arquitectura-datos]] (`.claude/arquitectura/patron-repository.md`). Ver [[fase-migracion]].
