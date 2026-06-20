# Agentes — comercializadora-api

Subagentes de Claude Code (invocables con la tool `Agent`) para la **migración del
back-end legado** `lluviaBackEnd` (.NET Framework, DAO/Entidades) a este repo
(.NET 10, Repository + Dapper + Stored Procedures).

Cada archivo es un subagente con frontmatter (`name`, `description`, `tools`). Su cuerpo
es el system prompt. Léelos antes de invocarlos y mantenlos alineados con
[`../arquitectura/patron-repository.md`](../arquitectura/patron-repository.md) y las
reglas de [`../../CLAUDE.md`](../../CLAUDE.md).

## Pipeline de migración de un módulo

```
migrador-modulo-backend   → porta 1 módulo legacy (Entidad → Repository → Controller)
        │
        ▼
revisor-backend           → dotnet build + checklist de adherencia al patrón
```

## Roles

| Agente | Archivo | Entrada | Salida |
|---|---|---|---|
| Migrador de módulo | [migrador-modulo-backend.md](migrador-modulo-backend.md) | Módulo legacy (`XxxDAO` + entidades + SP) | Model + `IXxxRepository`/`XxxRepository` + Controller |
| Revisor de back-end | [revisor-backend.md](revisor-backend.md) | Módulo migrado | Reporte de build + checklist |

## Reglas comunes
- Idioma: español. Identificadores en inglés/PascalCase (C#).
- **Sin Entity Framework**: Stored Procedures + Dapper + Repository ligero.
- Origen legacy: `E:\Documents\GitHub\comercializadora\lluviaBackEnd`.
- **No commitear automáticamente**; commitear solo cuando el usuario lo pida.
- Toda decisión/hallazgo relevante → registrarla en `../memory/` (y como regla dura,
  ajustar las reglas del proyecto según hallazgos).
