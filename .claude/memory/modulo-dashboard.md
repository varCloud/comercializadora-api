# Módulo Dashboard migrado (comercializadora-api)

Migración de `DashBoardController` + `DashboardDAO` del legado. Reusa **7 SP existentes sin
modificarlos**. Fecha: 2026-06-20.

## Endpoints — base `api/dashboard`, todos `GET`, `[Authorize]` (cualquier autenticado)
- `kpis` → `Notificacion<DashboardKpis>` (compuesto: ventas día/sem/mes/año + info global id!=1 + merma [0]/[1] + costo [0]/[1]).
- `ventas-por-fecha?periodo=` (default 2) → `Notificacion<VentasPorFecha>` (categorías con fechaIni/fechaFin para drilldown).
- `ventas-por-estacion?fechaIni=&fechaFin=` → `Notificacion<IEnumerable<EstacionVenta>>`.
- `top-ten?periodo=&tipo=` → `Notificacion<IEnumerable<Categoria>>`.
- `informacion-global?periodo=` (default 4) → `Notificacion<IEnumerable<Categoria>>`.
- `merma` → `Notificacion<IEnumerable<MermaMensual>>`.
- `costo-produccion` → `Notificacion<IEnumerable<CostoProduccionMensual>>`.
- `iva-acumulado?periodo=` (default 2) → `Notificacion<IEnumerable<Categoria>>` — **expuesto pero
  código muerto en el legado** (CrearDataGraficoIVA calculaba el dato y la vista no lo renderizaba).

`periodo` → `EnumTipoReporteGrafico` (1=Semanal,2=Mensual,3=Anual,4=Día). `tipo` →
`EnumTipoGrafico` (2=Productos,3=Clientes,4=Proveedores). Los enums legados tenían valores
plurales (`Mensuales/Anuales`) y typo `Provedores`; se renombraron conservando el valor numérico.

## Regla de estación por rol (importante)
El filtro de estación se aplica en `DashboardService.ResolverEstacion(idRol, idEstacionToken)
=> idRol == 3 ? idEstacionToken : 0`. **`idRol` e `idEstacion` se leen de los claims del JWT**
(`DashboardController.ParseClaim`), NUNCA de query param. En el repo `idEstacion == 0` se
traduce a `NULL` para el SP (= todas las estaciones).

> Esto exigió **agregar el claim `idEstacion`** al `JwtTokenGenerator` (módulo Auth). El modelo
> `Sesion` ya tenía `IdEstacion`. Cualquier módulo futuro que filtre por estación lo lee de ahí.

## kpis: composición y degradación de errores
`ventas-por-estacion` (sin rango) es la fuente de los totales: si **falla** (Estatus != 200)
se **propaga** el error. Los sub-SP complementarios (info global, merma, costo) **degradan a
0/null** sin tumbar el KPI; en éxito el resultado final es `Estatus=200, Mensaje="OK"`.
Merma/Costo actual = índice [0], anterior = índice [1].

## Mapeo de `Categoria`
Propiedad `NombreCategoria` + `[JsonPropertyName("categoria")]` (CS0542: prop no puede llamarse
como su tipo). Como NO se tocan los SP, el repo proyecta a mano los resultsets de `Categoria`
(ver memoria `dapper-mapeo-columnas` punto 2). `EstacionVenta/MermaMensual/CostoProduccionMensual`
mapean directo con `ConsultarAsync<T>`.

## Archivos
- `Models/Entities/`: `Categoria`, `EstacionVenta`, `MermaMensual`, `CostoProduccionMensual`.
- `Models/Enums/`: `EnumTipoReporteGrafico`, `EnumTipoGrafico`.
- `Models/Dtos/`: `DashboardKpis`, `VentasPorFecha`.
- `Repositories/Dashboard/`, `Services/Dashboard/`, `Controllers/DashboardController.cs`.
- `Security/JwtTokenGenerator.cs` (claim idEstacion), `Program.cs` (DI).
</content>
