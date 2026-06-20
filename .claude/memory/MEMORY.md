# Memoria del proyecto — comercializadora-api

Índice de memorias persistentes. Una línea por memoria.

- [Fase de migración](fase-migracion.md) — ahora mismo migramos el back-end legado lluviaBackEnd a este repo.
- [Arquitectura de datos](arquitectura-datos.md) — sin EF; Stored Procedures + Dapper + Repository ligero.
- [Reset a plantilla base .NET 10](reset-plantilla-net10.md) — proyecto limpiado y migrado a net10.0; credencial filtrada en git history.
- [Subagentes de migración](subagentes-migracion.md) — `.claude/agents/`: migrador-modulo-backend → revisor-backend.
- [BD local de desarrollo](bd-local-desarrollo.md) — instancia localhost\SQLEXPRESS01, base DB_A57E86_comercializadora, sa (password en User Secrets); contrato de SP_VALIDA_CONTRASENA.
- [Convenciones de endpoints](convenciones-endpoints.md) — verbos HTTP correctos, controladores devuelven entidad/Notificacion<T> (no IActionResult), JSON camelCase.
