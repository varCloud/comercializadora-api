# Módulo Producción Trapeadores migrado (reporte solo lectura)

Reporte hermano de [[modulo-produccion-liquidos]]. Mismo shape de columnas, mismo SP,
**sin crear ni tocar ningún stored procedure**: reutiliza tal cual
`SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS`, que ya es genérico vía `@idTipoMovInventario`
(`NULL`/`0` → `IN (26,27)` para líquidos; `>0` → coincidencia exacta). El repository de
Trapeadores fija `@idTipoMovInventario = 32` **hardcodeado como constante** en
`ProduccionTrapeadoresRepository` — no se expone como parámetro de la query ni del front.

## Endpoint
`GET /api/produccion-trapeadores?page=&perPage=&idRol=&idUsuario=&fechaIni=&fechaFin=&order=&sort=`
`[Authorize]` → `Notificacion<IEnumerable<CargaMercanciaTrapeadores>>` (con `links`/`meta`).
Sin `q` de texto libre. `order` whitelist: `fecha|cantidad|producto` (mismo SP que Líquidos).

## Entidad separada
`Models/Entities/CargaMercanciaTrapeadores.cs` es una clase de dominio propia (no se reusa
`CargaMercanciaLiquidos`) aunque el shape de columnas es idéntico — mismo criterio que ya
separa `RelacionLiquidos`/`RelacionTrapeadores` en este repo: reportes/entidades hermanas se
modelan cada una con su propio tipo aunque compartan columnas, porque son agregados de
dominio distintos.

## Archivos
`Models/Entities/CargaMercanciaTrapeadores.cs`, `Models/Dtos/ProduccionTrapeadoresQuery.cs`,
`Repositories/ProduccionTrapeadores/*`, `Services/ProduccionTrapeadores/*`,
`Controllers/ProduccionTrapeadoresController.cs`. DI en `Program.cs` (junto al de
ProduccionLiquidos). `dotnet build` → 0 errores, 0 warnings (2026-07-03).

## Sin catálogos propios
Igual que Líquidos: roles/usuarios para los filtros del front se consumen del módulo
Usuarios ya migrado; no se duplican aquí.

Relacionado: [[modulo-produccion-liquidos]], [[modulo-productos]].
