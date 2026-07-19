/*
===============================================================================
 SP_V2_CONSULTA_PROCESO_PRODUCCION_AGRANEL
-------------------------------------------------------------------------------
 Versión 2 (migración) del listado SP_CONSULTA_PROCESO_PRODUCCION (pantalla
 "Producción Agranel" / costo de producción del legado). NO reemplaza al
 legado: el sistema viejo sigue usando el SP original; la API nueva
 (GET /api/produccion-agranel) usa este V2.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el
     legado devolvía TODO el resultset y paginaba con DataTables en el cliente.
   - Orden dinámico opcional (@order + @sort; whitelist
     fecha|producto|cantidad|estatus), default fechaAlta DESC (el legado no
     tenía ORDER BY explícito; se elige "más reciente primero" para el panel).
   - Filtro opcional @idAlmacen: cubre el caso del WS móvil
     SP_APP_OBTENER_PRODUCTOS_PRODUCCION_AGRANEL (listado por almacén), que no
     se migra como endpoint aparte (ver HU produccion_agranel).
   - Expone idProcesoProduccionAgranel/idProducto/idUbicacion/idAlmacen/
     idEstatusProduccionAgranel por renglón: los necesita el flujo de
     aprobación (SP_APP_APROBAR_PRODUCTOS_PRODCUCCION_AGRANEL).
   - cantidadRestante se calcula como ROUND(cantidad - cantidadAceptada, 2)
     (paridad con la vista legada _ObtenerListadoCostoProduccion.cshtml, que la
     calculaba en Razor; la columna física no siempre está poblada).
   - Corrige el texto del estatus agrupado 1/2: 'Pendiente de procesar'
     (el legado mostraba el typo 'Pendiende de procesar').
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada

 Conserva la MISMA lógica de filtrado que el original:
   - @idUsuario / @idEstatus: filtro exacto opcional (NULL = sin filtro).
   - @fechaIni / @fechaFin: rango opcional e independiente por fecha (CAST a
     date, igual que el legado, que usaba COALESCE con la propia columna).
   - JOINs (no LEFT) a Productos/LineaProducto/CatEstatusProcesoAgranel/
     Usuarios: un proceso sin producto/estatus/usuario no se lista (idéntico
     al legado).

 Idempotente: usa CREATE OR ALTER, se puede ejecutar N veces sin error.

 Autor migración: equipo lluvia-migracion · 2026-07-04
 Origen: SP_CONSULTA_PROCESO_PRODUCCION (Jessica Almonte, 2020-07-28;
 definición leída de BD DB_A57E86_comercializadora, 2026-07-04)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_PROCESO_PRODUCCION_AGRANEL]
    @idUsuario   int      = null,
    @idEstatus   int      = null,
    @idAlmacen   int      = null,
    @fechaIni    datetime = null,
    @fechaFin    datetime = null,
    @order       varchar(50) = null,
    @sort        varchar(4)  = null,   -- 'asc' | 'desc'
    @pageNumber  int      = 1,
    @pageSize    int      = 10
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
            r.idProcesoProduccionAgranel,
            r.idProducto,
            r.idUbicacion,
            r.idAlmacen,
            r.idUsuario,
            r.cantidad,
            r.cantidadAceptada,
            ROUND(COALESCE(r.cantidad, 0) - COALESCE(r.cantidadAceptada, 0), 2) AS cantidadRestante,
            r.fechaAlta,
            p.codigoBarras,
            p.descripcion                                    AS descripcionProducto,
            l.idLineaProducto,
            l.descripcion                                    AS descripcionLinea,
            r.idEstatusProduccionAgranel,
            CASE WHEN epa.idEstatusProcesoAgranel IN (1, 2)
                 THEN 'Pendiente de procesar'
                 ELSE epa.descripcion END                    AS descripcionEstatus,
            p.ultimoCostoCompra                              AS ultimoCostoCompra,
            COALESCE(u.nombre, '') + ' ' + COALESCE(u.apellidoPaterno, '') + ' '
                + COALESCE(u.apellidoMaterno, '')            AS nombreUsuario
        INTO #pagina
        FROM        ProcesoProduccionAgranel r
            JOIN    Productos p                ON p.idProducto = r.idProducto
            JOIN    LineaProducto l            ON l.idLineaProducto = p.idLineaProducto
            JOIN    CatEstatusProcesoAgranel epa ON epa.idEstatusProcesoAgranel = r.idEstatusProduccionAgranel
            JOIN    Usuarios u                 ON u.idUsuario = r.idUsuario
        WHERE       (@idUsuario IS NULL OR r.idUsuario = @idUsuario)
                AND (@idEstatus IS NULL OR r.idEstatusProduccionAgranel = @idEstatus)
                AND (@idAlmacen IS NULL OR r.idAlmacen = @idAlmacen)
                AND (@fechaIni IS NULL OR CAST(r.fechaAlta AS date) >= CAST(@fechaIni AS date))
                AND (@fechaFin IS NULL OR CAST(r.fechaAlta AS date) <= CAST(@fechaFin AS date));

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
            idProcesoProduccionAgranel,
            idProducto,
            idUbicacion,
            idAlmacen,
            idUsuario,
            cantidad,
            cantidadAceptada,
            cantidadRestante,
            fechaAlta,
            codigoBarras,
            descripcionProducto,
            idLineaProducto,
            descripcionLinea,
            idEstatusProduccionAgranel,
            descripcionEstatus,
            ultimoCostoCompra,
            nombreUsuario
        FROM   #pagina
        ORDER BY
            CASE WHEN @col = 'fecha'    AND @dir = 'asc'  THEN fechaAlta           END ASC,
            CASE WHEN @col = 'fecha'    AND @dir = 'desc' THEN fechaAlta           END DESC,
            CASE WHEN @col = 'producto' AND @dir = 'asc'  THEN descripcionProducto END ASC,
            CASE WHEN @col = 'producto' AND @dir = 'desc' THEN descripcionProducto END DESC,
            CASE WHEN @col = 'cantidad' AND @dir = 'asc'  THEN cantidad            END ASC,
            CASE WHEN @col = 'cantidad' AND @dir = 'desc' THEN cantidad            END DESC,
            CASE WHEN @col = 'estatus'  AND @dir = 'asc'  THEN descripcionEstatus  END ASC,
            CASE WHEN @col = 'estatus'  AND @dir = 'desc' THEN descripcionEstatus  END DESC,
            fechaAlta DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
