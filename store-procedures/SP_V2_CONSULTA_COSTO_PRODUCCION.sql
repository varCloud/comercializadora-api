/*
===============================================================================
 SP_V2_CONSULTA_COSTO_PRODUCCION
-------------------------------------------------------------------------------
 Versión 2 (migración) del reporte SP_CONSULTA_COSTO_PRODUCCION (pantalla
 legada "Costo de Producción Agranel" / menú "Consumo de MPL"). NO reemplaza
 al legado: el sistema viejo sigue usando el SP original; la API nueva
 (GET /api/consumo-mpl) usa este V2.

 La lógica de CÁLCULO/CACHÉ se copia TAL CUAL del original (no se modifica):
   - Resuelve @mesCalculo/@anioCalculo al mes/año actual cuando vienen NULL.
   - Calcula @ultimoDiaMesCalculo (hoy si el mes de cálculo es el actual, si
     no el último día de ese mes) y @ultimoDiaMesAnterior.
   - Si el mes de cálculo es el mes actual, borra el caché de ese mes en
     ReporteCostoProduccion para forzar recálculo (el mes aún no cierra).
   - Si no existe ya el cálculo para @ultimoDiaMesCalculo, lo genera:
     INSERT agregando ProcesoProduccionAgranel (idEstatusProduccionAgranel>=3)
     del mes de cálculo, con dbo.redondear()/dbo.obtenerPrecioCompra(); luego
     dos UPDATE para porcCostoProduccion y costoProduccionMerma.
   - Mismos JOINs (INNER, no LEFT) a Productos/LineaProducto: un producto sin
     línea no se lista (idéntico al legado).
   - Mismo filtro por línea (p.idLineaProducto = COALESCE(@idLinea, ...)) y
     por almacén (dbo.ExisteProductoEnAlmancen — nombre real de la función en
     BD, con el typo "Almancen").

 Novedades respecto al original (mecánica de migración, no tocan el cálculo):
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el
     legado devolvía TODO el resultset y paginaba con DataTables en el cliente.
   - @search: filtra por descripción de producto o código de barras (LIKE);
     petición explícita del usuario, el legado no tenía buscador aquí.
   - Orden dinámico opcional (@order + @sort; whitelist
     producto|linea|cantidadsolicitada|cantidadaceptada|costo), default
     descripcionProducto ASC (el legado no tenía ORDER BY explícito).
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada
   - NO reproduce el "status = -1 / No se encontraron resultados" del
     original cuando el resultado es vacío: con paginación real, una página
     sin filas (o un filtro que no matchea) es un resultado válido (total=0,
     status=200); el front resuelve el estado vacío. Documentado en la
     memoria del repo API (regla dura del workspace: ajustar según hallazgos).
   - Expone únicamente los campos que consume la vista legada
     (_ObtenerCostoProduccion.cshtml): idProducto, codigoBarras,
     descripcionProducto, idLineaProducto, descripcionLinea,
     cantidadSolicitadaMesAnt, cantidadAceptadaFinalMesAnt, ultCostoCompra,
     costoProduccionMerma. No expone idReporteCostoProduccion,
     porcCostoProduccion, ultimoDiaMesCalculo/ultimoDiaMesAnterior, fechaAlta
     (no usados por esta pantalla). "Cantidad Restante" la calcula el front.

 Idempotente: usa CREATE OR ALTER, se puede ejecutar N veces sin error.

 Autor migración: equipo lluvia-migracion · 2026-07-05
 Origen: SP_CONSULTA_COSTO_PRODUCCION (Jessica Almonte Acosta, 2020-07-28;
 definición leída de BD DB_A57E86_comercializadora, 2026-07-05)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_COSTO_PRODUCCION]
    @mesCalculo  int         = null,
    @anioCalculo int         = null,
    @idLinea     int         = null,
    @idAlmacen   int         = null,
    @search      varchar(255) = null,
    @order       varchar(50)  = null,
    @sort        varchar(4)   = null,   -- 'asc' | 'desc'
    @pageNumber  int          = 1,
    @pageSize    int          = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, '')),
            @ultimoDiaMesCalculo  date,
            @ultimoDiaMesAnterior date,
            @primerDiaMesCalculo  date,
            @fechaActual          datetime;

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        -- ============================================================
        -- Lógica de cálculo/caché IDÉNTICA a SP_CONSULTA_COSTO_PRODUCCION
        -- ============================================================
        SELECT @fechaActual = dbo.FechaActual();

        SELECT @mesCalculo = COALESCE(@mesCalculo, MONTH(@fechaActual)),
               @anioCalculo = COALESCE(@anioCalculo, YEAR(@fechaActual));

        SELECT @primerDiaMesCalculo = DATEFROMPARTS(@anioCalculo, @mesCalculo, 01);

        SELECT @ultimoDiaMesCalculo =
                    CASE WHEN CAST(@fechaActual AS date) < EOMONTH(@primerDiaMesCalculo)
                         THEN CAST(@fechaActual AS date)
                         ELSE EOMONTH(@primerDiaMesCalculo) END,
               @ultimoDiaMesAnterior = EOMONTH(@primerDiaMesCalculo, -1);

        -- Si el mes calculado es el actual, se borra el caché para que se recalcule
        -- (el mes todavía no ha terminado).
        IF (MONTH(@ultimoDiaMesCalculo) = MONTH(@fechaActual) AND YEAR(@ultimoDiaMesCalculo) = YEAR(@fechaActual))
        BEGIN
            DELETE ReporteCostoProduccion
            WHERE  MONTH(UltimoDiaMesCalculo) = MONTH(@fechaActual)
               AND YEAR(UltimoDiaMesCalculo)  = YEAR(@fechaActual);
        END

        IF NOT EXISTS (SELECT 1 FROM ReporteCostoProduccion WHERE UltimoDiaMesCalculo = @ultimoDiaMesCalculo)
        BEGIN
            INSERT INTO ReporteCostoProduccion (
                idProducto, cantidadSolicitadaMesAnt, cantidadAceptadaFinalMesAnt,
                ultCostoCompra, ultimoDiaMesCalculo, ultimoDiaMesAnterior, fechaAlta)
            SELECT
                PPA.idProducto,
                dbo.redondear(SUM(PPA.cantidad)),
                dbo.redondear(SUM(PPA.cantidadAceptada)),
                dbo.obtenerPrecioCompra(PPA.idProducto, @ultimoDiaMesCalculo),
                @ultimoDiaMesCalculo,
                @ultimoDiaMesAnterior,
                @fechaActual
            FROM        ProcesoProduccionAgranel PPA
                JOIN    Productos P ON P.idProducto = PPA.idProducto
            WHERE       PPA.idEstatusProduccionAgranel >= 3
                    AND CAST(PPA.fechaAlta AS date) >= @primerDiaMesCalculo
                    AND CAST(PPA.fechaAlta AS date) <= @ultimoDiaMesCalculo
            GROUP BY    PPA.idProducto;

            UPDATE ReporteCostoProduccion
            SET    porcCostoProduccion = dbo.redondear((cantidadAceptadaFinalMesAnt * ultCostoCompra) / 100)
            WHERE  UltimoDiaMesCalculo = @ultimoDiaMesCalculo;

            UPDATE ReporteCostoProduccion
            SET    costoProduccionMerma = dbo.redondear(cantidadAceptadaFinalMesAnt * ultCostoCompra)
            WHERE  UltimoDiaMesCalculo = @ultimoDiaMesCalculo;
        END
        -- ============================================================
        -- Fin lógica idéntica al original
        -- ============================================================

        SELECT
            CAST(r.idProducto AS int) AS idProducto,
            p.codigoBarras,
            p.descripcion              AS descripcionProducto,
            l.idLineaProducto,
            l.descripcion               AS descripcionLinea,
            r.cantidadSolicitadaMesAnt,
            r.cantidadAceptadaFinalMesAnt,
            r.ultCostoCompra,
            r.costoProduccionMerma
        INTO #pagina
        FROM        ReporteCostoProduccion r
            JOIN    Productos p       ON p.idProducto = r.idProducto
            JOIN    LineaProducto l   ON l.idLineaProducto = p.idLineaProducto
        WHERE       r.UltimoDiaMesCalculo = @ultimoDiaMesCalculo
                AND p.idLineaProducto = COALESCE(@idLinea, p.idLineaProducto)
                AND 1 = CASE WHEN COALESCE(@idAlmacen, 0) > 0
                             THEN dbo.ExisteProductoEnAlmancen(@idAlmacen, r.idProducto)
                             ELSE 1 END
                AND (@search IS NULL OR @search = ''
                     OR p.descripcion LIKE '%' + @search + '%'
                     OR p.codigoBarras LIKE '%' + @search + '%');

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
            idProducto,
            codigoBarras,
            descripcionProducto,
            idLineaProducto,
            descripcionLinea,
            cantidadSolicitadaMesAnt,
            cantidadAceptadaFinalMesAnt,
            ultCostoCompra,
            costoProduccionMerma
        FROM   #pagina
        ORDER BY
            CASE WHEN @col = 'producto'           AND @dir = 'asc'  THEN descripcionProducto          END ASC,
            CASE WHEN @col = 'producto'           AND @dir = 'desc' THEN descripcionProducto          END DESC,
            CASE WHEN @col = 'linea'              AND @dir = 'asc'  THEN descripcionLinea              END ASC,
            CASE WHEN @col = 'linea'              AND @dir = 'desc' THEN descripcionLinea              END DESC,
            CASE WHEN @col = 'cantidadsolicitada' AND @dir = 'asc'  THEN cantidadSolicitadaMesAnt       END ASC,
            CASE WHEN @col = 'cantidadsolicitada' AND @dir = 'desc' THEN cantidadSolicitadaMesAnt       END DESC,
            CASE WHEN @col = 'cantidadaceptada'   AND @dir = 'asc'  THEN cantidadAceptadaFinalMesAnt    END ASC,
            CASE WHEN @col = 'cantidadaceptada'   AND @dir = 'desc' THEN cantidadAceptadaFinalMesAnt    END DESC,
            CASE WHEN @col = 'costo'              AND @dir = 'asc'  THEN costoProduccionMerma           END ASC,
            CASE WHEN @col = 'costo'              AND @dir = 'desc' THEN costoProduccionMerma           END DESC,
            descripcionProducto ASC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
