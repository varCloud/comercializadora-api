/*
===============================================================================
 SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS
-------------------------------------------------------------------------------
 Versión 2 (migración) del reporte SP_CONSULTA_CARGA_MERCANCIA_LIQUIDOS.
 NO reemplaza al legado: el sistema viejo sigue usando el SP original; la API
 nueva (reporte "Producción Líquidos", solo lectura) usa este V2.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el legado
     devolvía TODO el resultset sin paginar (TOP 50 en memoria vía DataTables
     cuando no había ningún filtro, o TOP "sin límite" con filtros). Con
     paginación real ya no se necesita ese tope de 50: "sin filtros, el
     reporte trae todos los movimientos (paginados)" (HU produccion_liquidos).
   - Orden dinámico opcional (@order + @sort; whitelist fecha|cantidad|producto),
     default fechaAlta asc (igual que el legado, que hacía "ORDER BY fechaAlta"
     sin dirección explícita = ascendente).
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada (mismas columnas que el legado)

 Conserva la MISMA lógica de filtrado que el original:
   - @idTipoMovInventario NULL o 0 → solo movimientos de carga de líquidos
     (idTipoMovInventario IN (26, 27)); >0 → coincidencia exacta. El repository
     de la API siempre manda NULL (el legado tampoco lo expone en el form).
   - @idRol / @idUsuario: filtro exacto opcional (NULL = sin filtro).
   - @fechaIni / @fechaFin: filtro de rango opcional e INDEPENDIENTE por fecha
     (CAST a date, igual que el legado). Deliberadamente NO se replica el
     quirk del original que forzaba ambas fechas a "hoy" cuando solo una
     llegaba NULL: ese workaround existía para acotar el reporte sin
     paginación real; con OFFSET/FETCH ya no aplica (ver HU: "sin filtros,
     el listado muestra todos los movimientos").
   - JOIN (no LEFT JOIN) a la ubicación/almacén y al producto: un movimiento
     sin ubicación mapeada no se lista (mismo comportamiento que el legado).

 Idempotente: usa CREATE OR ALTER, se puede ejecutar N veces sin error.

 Autor migración: equipo lluvia-migracion · 2026-07-03
 Origen: SP_CONSULTA_CARGA_MERCANCIA_LIQUIDOS (Ernesto Aguilar, 2022-06-25;
 definición leída de BD DB_A57E86_comercializadora, 2026-07-03)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_CARGA_MERCANCIA_LIQUIDOS]
    @idTipoMovInventario int      = null,
    @idRol               int      = null,
    @idUsuario           int      = null,
    @fechaIni            datetime = null,
    @fechaFin            datetime = null,
    @order               varchar(50) = null,
    @sort                varchar(4)  = null,   -- 'asc' | 'desc'
    @pageNumber          int      = 1,
    @pageSize            int      = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        SELECT
            idl.idProducto,
            ubi.descripcion                                                            AS descripcionUbicacion,
            p.descripcion                                                              AS descripcionProducto,
            idl.cantidad,
            u.nombre + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '') AS nombreUsuario,
            idl.fechaAlta,
            r.descripcion                                                              AS descripcionRol,
            dbo.redondear(p.ultimoCostoCompra)                                         AS ultimoCostoCompra,
            c.descripcion                                                              AS descTipoMovInventario
        INTO #pagina
        FROM        InventarioDetalleLog idl
            JOIN    Usuarios u  ON u.idUsuario = idl.idUsuario
            JOIN    CatRoles r  ON r.idRol = u.idRol
            JOIN    CatTipoMovimientoInventario c ON c.idTipoMovInventario = idl.idTipoMovInventario
            JOIN    Productos p ON p.idProducto = idl.idProducto
            JOIN    (
                        SELECT id.idUbicacion, ubc.idAlmacen, id.idProducto, alm.descripcion
                        FROM   InventarioDetalle id
                                   JOIN Ubicacion ubc ON ubc.idUbicacion = id.idUbicacion
                                   JOIN Almacenes alm ON alm.idAlmacen = ubc.idAlmacen
                    ) ubi ON ubi.idUbicacion = idl.idUbicacion AND ubi.idProducto = idl.idProducto
        WHERE       u.idUsuario = COALESCE(@idUsuario, u.idUsuario)
                AND u.idRol     = COALESCE(@idRol, u.idRol)
                AND (
                        (COALESCE(@idTipoMovInventario, 0) = 0 AND idl.idTipoMovInventario IN (26, 27))
                     OR (COALESCE(@idTipoMovInventario, 0) > 0 AND idl.idTipoMovInventario = @idTipoMovInventario)
                    )
                AND (@fechaIni IS NULL OR CAST(idl.fechaAlta AS date) >= CAST(@fechaIni AS date))
                AND (@fechaFin IS NULL OR CAST(idl.fechaAlta AS date) <= CAST(@fechaFin AS date));

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
            idProducto,
            descripcionUbicacion,
            descripcionProducto,
            cantidad,
            nombreUsuario,
            fechaAlta,
            descripcionRol,
            ultimoCostoCompra,
            descTipoMovInventario
        FROM   #pagina
        ORDER BY
            CASE WHEN @col = 'fecha'    AND @dir = 'asc'  THEN fechaAlta          END ASC,
            CASE WHEN @col = 'fecha'    AND @dir = 'desc' THEN fechaAlta          END DESC,
            CASE WHEN @col = 'cantidad' AND @dir = 'asc'  THEN cantidad           END ASC,
            CASE WHEN @col = 'cantidad' AND @dir = 'desc' THEN cantidad           END DESC,
            CASE WHEN @col = 'producto' AND @dir = 'asc'  THEN descripcionProducto END ASC,
            CASE WHEN @col = 'producto' AND @dir = 'desc' THEN descripcionProducto END DESC,
            fechaAlta ASC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
