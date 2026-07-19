/*
===============================================================================
 SP_V2_CONSULTA_INVENTARIO
-------------------------------------------------------------------------------
 Unifica, para la nueva pantalla "Reportes > Inventario", los DOS SP legados:
   - SP_CONSULTA_INVENTARIO                    (listado con filtros, @fecha único)
   - SP_CONSULTA_INVENTARIO_GENERAL_UBICACION  (export completo por @tipo)
 NO reemplaza a ninguno de los dos: ambos quedan intactos en BD como referencia
 histórica (el legado los sigue usando); la API nueva usa solo este V2.

 Modo LISTADO (@exportar = 0, default): pagina con OFFSET/FETCH aplicando los
 filtros de pantalla (@idLineaProducto, @idAlmacen, @search sobre
 descripción/artículo, rango @fechaIni/@fechaFin). Replica el mismo cálculo de
 "cantidad a la fecha" de SP_CONSULTA_INVENTARIO (última fila de
 InventarioDetalleLog <= fecha de corte, por producto+almacén, cruzado contra
 TODO el catálogo activo x almacenes — igual que el legado, que también lista
 el catálogo completo, no solo lo que tuvo movimientos).

 Modo EXPORTAR (@exportar = 1): ignora paginación Y los filtros de pantalla
 (idLineaProducto/idAlmacen/fecha/search) — devuelve TODO el inventario, igual
 que el legado. Replica exacto SP_CONSULTA_INVENTARIO_GENERAL_UBICACION según
 @tipo (1 General / 2 Ubicación), en un único resultset de columnas "unión"
 (las de Ubicación quedan NULL cuando @tipo = 1) para que el repositorio use
 una sola entidad de exportación.

-------------------------------------------------------------------------------
 DECISIÓN DOCUMENTADA — rango de fechas del modo LISTADO:
 SP_CONSULTA_INVENTARIO (legado) calcula "cantidad a la fecha" como snapshot a
 UN punto en el tiempo (@fecha, tope = hoy), cruzando TODO Productos activos x
 TODO Almacenes (no es un listado de movimientos, es un catálogo completo con
 existencia calculada). El propio SP legado trae, comentado/deshabilitado, un
 intento previo de expandir ese único @fecha en un rango día-a-día (CTE
 recursivo "#dias" + cross join) — evidencia de que el equipo original lo probó
 y lo descartó (probablemente por costo: InventarioDetalleLog tiene ~5.3M filas
 en la BD real, y ese cross-apply por día multiplica el costo por cada día del
 rango).
 Para esta migración se optó por NO revivir esa expansión día-a-día (mismo
 riesgo de performance) y en su lugar:
   - @fechaFin es la fecha de corte del snapshot (misma semántica que el
     @fecha único del legado; se topa a HOY si viene futura).
   - @fechaIni se acepta y se valida (si es mayor a @fechaFin se iguala a
     @fechaFin) para cumplir el contrato de rango pedido en la HU, pero NO
     multiplica filas: el snapshot sigue siendo uno solo, calculado en
     @fechaFin. Con el default de pantalla (hoy/hoy) el comportamiento es
     IDÉNTICO al legado (rango de 1 día = @fecha único).
 Si en el futuro se requiere de verdad un desglose día-a-día, es la CTE
 recursiva comentada en SP_CONSULTA_INVENTARIO el punto de partida, con un
 OPTION (MAXRECURSION n) acotado y evaluando el impacto en InventarioDetalleLog.
-------------------------------------------------------------------------------
 FIX DE PERFORMANCE (2026-07-15) — parameter sniffing:
 Se detectó que el plan cacheado del modo LISTADO podía quedar compilado con
 una combinación de parámetros no representativa (ej. el primer smoke-test tras
 el deploy) y reusarse para llamadas muy distintas, multiplicando el tiempo de
 respuesta por ~7x (13.9s con plan cacheado malo vs ~2s con plan fresco, medido
 directo contra BD). La cardinalidad de estas dos consultas varía mucho según
 qué filtros llegan (idLineaProducto/idAlmacen/search NULL o no), así que se
 agregó OPTION (RECOMPILE) a ambas: el costo de compilar en cada llamada es
 aceptable para una pantalla de reporte (no es un endpoint de alta frecuencia)
 y evita este tipo de regresión. Si en el futuro se vuelve un problema de CPU
 por volumen de llamadas, la alternativa es plan guides o forzar
 OPTIMIZE FOR UNKNOWN en vez de RECOMPILE completo.
-------------------------------------------------------------------------------
 Compatibilidad: BD con compatibility_level = 120 → sin OPENJSON/STRING_SPLIT/
 JSON_VALUE. Idempotente: CREATE OR ALTER.

 Autor migración: equipo lluvia-migracion · 2026-07-15
 Origen: SP_CONSULTA_INVENTARIO (Jessica Almonte, 2020-06-05) +
 SP_CONSULTA_INVENTARIO_GENERAL_UBICACION (definiciones leídas de BD
 DB_A57E86_comercializadora, 2026-07-15).
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_INVENTARIO]
    @idLineaProducto int          = null,
    @idAlmacen       int          = null,
    @search          varchar(255) = null,
    @fechaIni        datetime     = null,
    @fechaFin        datetime     = null,
    @page            int          = 1,
    @perPage         int          = 10,
    @order           varchar(50)  = null,
    @sort            varchar(4)   = null,
    @exportar        bit          = 0,
    @tipo            int          = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4)  = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@page     IS NULL OR @page     < 1) SET @page     = 1;
    IF (@perPage  IS NULL OR @perPage  < 1) SET @perPage  = 10;
    IF (@exportar IS NULL) SET @exportar = 0;
    IF (@tipo     IS NULL OR @tipo NOT IN (1, 2)) SET @tipo = 1;

    -- Fecha de corte del snapshot (ver decisión documentada arriba).
    SELECT @fechaFin = COALESCE(@fechaFin, dbo.FechaActual());
    IF (CAST(@fechaFin AS date) > CAST(dbo.FechaActual() AS date)) SELECT @fechaFin = dbo.FechaActual();
    SELECT @fechaIni = COALESCE(@fechaIni, @fechaFin);
    IF (@fechaIni > @fechaFin) SELECT @fechaIni = @fechaFin;

    BEGIN TRY

        IF (@exportar = 1)
        BEGIN
            -----------------------------------------------------------------
            -- Modo EXPORTAR: TODO el inventario, ignora filtros/paginación.
            -- Replica SP_CONSULTA_INVENTARIO_GENERAL_UBICACION según @tipo,
            -- en un resultset "unión" (columnas de Ubicación NULL si @tipo=1).
            -----------------------------------------------------------------
            SELECT @status AS status, @mensaje AS mensaje, @total AS total;

            IF (@tipo = 1)
            BEGIN
                SELECT
                    p.idProducto,
                    p.descripcion,
                    p.ultimoCostoCompra,
                    p.precioIndividual,
                    p.precioMenudeo,
                    ig.cantidad,
                    CAST(null AS int)     AS idPasillo,
                    CAST(null AS int)     AS idRaq,
                    CAST(null AS int)     AS idPiso,
                    CAST(null AS varchar(500)) AS almacen,
                    CAST(null AS varchar(500)) AS pasillo,
                    CAST(null AS varchar(500)) AS raq,
                    CAST(null AS varchar(500)) AS piso
                FROM    InventarioGeneral ig
                    JOIN Productos p ON p.idProducto = ig.idProducto
                ORDER BY p.descripcion;
            END
            ELSE
            BEGIN
                SELECT
                    p.idProducto,
                    p.descripcion,
                    p.ultimoCostoCompra,
                    p.precioIndividual,
                    p.precioMenudeo,
                    id.cantidad,
                    u.idPasillo,
                    u.idRaq,
                    u.idPiso,
                    a.Descripcion AS almacen,
                    pa.descripcion AS pasillo,
                    r.descripcion  AS raq,
                    ps.descripcion AS piso
                FROM    InventarioDetalle id
                    JOIN Productos p    ON p.idProducto = id.idProducto
                    JOIN Ubicacion u    ON u.idUbicacion = id.idUbicacion
                    JOIN Almacenes a    ON a.idAlmacen = u.idAlmacen
                    JOIN catPasillo pa  ON pa.idPasillo = u.idPasillo
                    JOIN catRaq r       ON r.idRaq = u.idRaq
                    JOIN catPiso ps     ON ps.idPiso = u.idPiso
                ORDER BY p.descripcion;
            END
        END
        ELSE
        BEGIN
            -----------------------------------------------------------------
            -- Modo LISTADO: pagina con filtros. Replica el cálculo de
            -- "cantidad a la fecha" de SP_CONSULTA_INVENTARIO.
            -----------------------------------------------------------------
            SELECT ultMov.idProducto, ultMov.idAlmacen, SUM(l.cantidadActual) cantidad
            INTO   #CANTIDAD_INVENTARIO
            FROM   InventarioDetalleLog l
                JOIN (
                    SELECT d.idProducto, u.idAlmacen, u.idUbicacion, MAX(d.idInventarioDetalleLOG) idInventarioDetalleLOG
                    FROM   InventarioDetalleLog d
                        JOIN Ubicacion u ON d.idUbicacion = u.idUbicacion
                    WHERE  CAST(d.fechaAlta AS date) <= CAST(@fechaFin AS date)
                    GROUP BY d.idProducto, u.idUbicacion, u.idAlmacen
                ) ultMov ON l.idInventarioDetalleLOG = ultMov.idInventarioDetalleLOG
            GROUP BY ultMov.idProducto, ultMov.idAlmacen
            OPTION (RECOMPILE);

            CREATE TABLE #INVENTARIO (
                fecha            date,
                almacen          varchar(200),
                descripcionLinea varchar(50),
                descripcion      varchar(100),
                codigoBarras     varchar(100),
                cantidad         float,
                costo            money
            );

            INSERT INTO #INVENTARIO (fecha, almacen, descripcionLinea, descripcion, codigoBarras, cantidad, costo)
            SELECT  CAST(@fechaFin AS date),
                    a.Descripcion,
                    ln.descripcion,
                    UPPER(p.descripcion),
                    p.codigoBarras,
                    COALESCE(cant.cantidad, 0),
                    COALESCE(p.ultimoCostoCompra, 0)
            FROM    Productos p
                JOIN LineaProducto ln ON p.idLineaProducto = ln.idLineaProducto
                CROSS JOIN Almacenes a
                LEFT JOIN #CANTIDAD_INVENTARIO cant ON cant.idProducto = p.idProducto AND cant.idAlmacen = a.idAlmacen
            WHERE   p.activo = 1
                AND p.idLineaProducto = COALESCE(@idLineaProducto, p.idLineaProducto)
                AND a.idAlmacen       = COALESCE(@idAlmacen, a.idAlmacen)
                AND (@search IS NULL OR @search = ''
                     OR p.descripcion LIKE '%' + @search + '%'
                     OR p.articulo    LIKE '%' + @search + '%')
            OPTION (RECOMPILE);

            SELECT @total = COUNT(1) FROM #INVENTARIO;

            SELECT @status AS status, @mensaje AS mensaje, @total AS total;

            SELECT
                fecha,
                almacen,
                descripcionLinea,
                descripcion,
                codigoBarras,
                cantidad,
                costo
            FROM #INVENTARIO
            ORDER BY
                CASE WHEN @col = 'fecha'    AND @dir = 'asc'  THEN fecha       END ASC,
                CASE WHEN @col = 'fecha'    AND @dir = 'desc' THEN fecha       END DESC,
                CASE WHEN @col = 'almacen'  AND @dir = 'asc'  THEN almacen     END ASC,
                CASE WHEN @col = 'almacen'  AND @dir = 'desc' THEN almacen     END DESC,
                CASE WHEN @col = 'producto' AND @dir = 'asc'  THEN descripcion END ASC,
                CASE WHEN @col = 'producto' AND @dir = 'desc' THEN descripcion END DESC,
                CASE WHEN @col = 'cantidad' AND @dir = 'asc'  THEN cantidad    END ASC,
                CASE WHEN @col = 'cantidad' AND @dir = 'desc' THEN cantidad    END DESC,
                CASE WHEN @col = 'costo'    AND @dir = 'asc'  THEN costo       END ASC,
                CASE WHEN @col = 'costo'    AND @dir = 'desc' THEN costo       END DESC,
                descripcion ASC
            OFFSET (@page - 1) * @perPage ROWS
            FETCH NEXT @perPage ROWS ONLY;
        END

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
