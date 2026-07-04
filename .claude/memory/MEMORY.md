# Memoria del proyecto — comercializadora-api

Índice de memorias persistentes. Una línea por memoria.

- [Fase de migración](fase-migracion.md) — ahora mismo migramos el back-end legado lluviaBackEnd a este repo.
- [Arquitectura de datos](arquitectura-datos.md) — sin EF; Stored Procedures + Dapper + Repository ligero.
- [Reset a plantilla base .NET 10](reset-plantilla-net10.md) — proyecto limpiado y migrado a net10.0; credencial filtrada en git history.
- [Subagentes de migración](subagentes-migracion.md) — `.claude/agents/`: migrador-modulo-backend → revisor-backend.
- [BD local de desarrollo](bd-local-desarrollo.md) — instancia localhost\SQLEXPRESS01, base DB_A57E86_comercializadora, sa (password en User Secrets); contrato de SP_VALIDA_CONTRASENA.
- [Convenciones de endpoints](convenciones-endpoints.md) — verbos HTTP correctos, controladores devuelven entidad/Notificacion<T> (no IActionResult), JSON camelCase.
- [Dapper: mapeo de columnas](dapper-mapeo-columnas.md) — Dapper no estripa `_` (aliasar en SP); SP legados de catálogo traen status+datos en un resultset con lookup case-sensitive (normalizar a dict OrdinalIgnoreCase).
- [Módulo Usuarios migrado](modulo-usuarios.md) — CRUD+catálogos+paginación; SP_V2_CONSULTA_USUARIOS idempotente en store-procedures/; endpoints api/usuarios [Authorize].
- [Módulo Dashboard migrado](modulo-dashboard.md) — 8 endpoints GET api/dashboard [Authorize]; kpis compuesto; filtro estación por rol leído del JWT (claim idEstacion); 7 SP reusados sin modificar.
- [Módulo Productos migrado](modulo-productos.md) — CRUD catálogo api/productos [Authorize]; SP_V2 paginado + artículo/código separados + claves SAT; incluye submenú "Líneas de producto" (módulo dedicado api/lineas-producto, SP_V2 con unicidad y bloqueo de baja).
- [Módulo Producción Líquidos migrado](modulo-produccion-liquidos.md) — reporte solo lectura api/produccion-liquidos [Authorize], un único GET paginado; SP_V2 reconstruido leyendo la definición real del legado en BD (no había .sql versionado); diverge deliberadamente del legado en el TOP 50 y en el quirk de fechas NULL→hoy.
- [Módulo Producción Trapeadores migrado](modulo-produccion-trapeadores.md) — reporte hermano de Producción Líquidos, api/produccion-trapeadores [Authorize]; reusa el MISMO SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS sin tocarlo, fijando @idTipoMovInventario=32 hardcodeado en el repository; entidad CargaMercanciaTrapeadores propia (no reusa CargaMercanciaLiquidos).
