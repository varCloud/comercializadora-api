/*
===============================================================================
 SP_V2_CONSULTA_COMPRAS
-------------------------------------------------------------------------------
 Versión 2 (migración) del listado de SP_CONSULTA_COMPRAS (@detalleCompra = 0).
 NO reemplaza al legado.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el legado
     devolvía TOP 50 sin paginar.
   - Búsqueda libre (@search) por proveedor / observaciones / idCompra.
   - Orden dinámico (@order + @sort) con whitelist (fecha | idcompra | proveedor);
     default fechaAlta desc (igual que el legado).
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada
   - Conserva las métricas del original por compra (montoTotal, totalCantProductos,
     recibidos, devueltos, montoTotalRecibido) y el cálculo de estadoCompra
     (0 = pendiente, 1 = correcta, 2 = incorrecta) y el filtro activo = 1.
   - Conserva el INNER JOIN al detalle: una compra sin productos no se lista
     (mismo comportamiento que el SP legado).

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-22
 Origen: SP_CONSULTA_COMPRAS (Ernesto Aguilar, 2020-07-23)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_COMPRAS]
    @search         varchar(150) = null,
    @idProveedor    int = null,
    @idStatusCompra int = null,
    @idUsuario      int = null,
    @idAlmacen      int = null,
    @fechaInicio    datetime = null,
    @fechaFin       datetime = null,
    @order          varchar(50) = null,
    @sort           varchar(4)  = null,   -- 'asc' | 'desc'
    @pageNumber     int = 1,
    @pageSize       int = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'asc' THEN 'asc'
                                       WHEN LOWER(@sort) = 'desc' THEN 'desc'
                                       ELSE 'desc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 25;

    BEGIN TRY

        SELECT
            c.idCompra,
            c.fechaAlta,
            c.observaciones,
            COALESCE(a.idAlmacen, 0)   AS idAlmacen,
            COALESCE(a.Descripcion, '') AS almacen,
            p.idProveedor,
            p.nombre                   AS proveedorNombre,
            s.idStatusCompra           AS idStatus,
            s.descripcion              AS estatusDescripcion,
            u.idUsuario,
            u.nombre + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '') AS nombreCompleto,
            det.montoTotal,
            det.totalCantProductos,
            det.totalCantProductosRecibidos,
            det.totalCantProductosDevueltos,
            det.montoTotalRecibido,
            CASE
                WHEN EXISTS (SELECT 1 FROM ComprasDetalle WHERE idCompra = c.idCompra AND idEstatusProductoCompra IN (3, 4, 5)) THEN 2
                WHEN EXISTS (SELECT 1 FROM ComprasDetalle WHERE idCompra = c.idCompra AND COALESCE(idEstatusProductoCompra, 0) = 0) THEN 0
                ELSE 1
            END AS estadoCompra
        INTO #pagina
        FROM    Compras c
            JOIN Proveedores p     ON c.idProveedor = p.idProveedor
            JOIN CatStatusCompra s ON c.idStatusCompra = s.idStatusCompra
            JOIN Usuarios u        ON c.idUsuario = u.idUsuario
            LEFT JOIN Almacenes a  ON c.idAlmacen = a.idAlmacen
            JOIN (
                SELECT d.idCompra,
                       SUM(d.precio * CAST(d.cantidad AS float))                        AS montoTotal,
                       SUM(d.cantidad)                                                  AS totalCantProductos,
                       SUM(COALESCE(d.cantidadRecibida, 0))                             AS totalCantProductosRecibidos,
                       SUM(COALESCE(d.cantidadDevuelta, 0))                             AS totalCantProductosDevueltos,
                       SUM(d.precio * CAST(COALESCE(d.cantidadRecibida, 0) AS float))   AS montoTotalRecibido
                FROM   ComprasDetalle d
                GROUP BY d.idCompra
            ) det ON det.idCompra = c.idCompra
        WHERE   c.activo = 1
            AND (@idProveedor    IS NULL OR c.idProveedor = @idProveedor)
            AND (@idStatusCompra IS NULL OR c.idStatusCompra = @idStatusCompra)
            AND (@idUsuario      IS NULL OR c.idUsuario = @idUsuario)
            AND (@idAlmacen      IS NULL OR c.idAlmacen = @idAlmacen)
            AND (@fechaInicio    IS NULL OR CAST(c.fechaAlta AS date) >= CAST(@fechaInicio AS date))
            AND (@fechaFin       IS NULL OR CAST(c.fechaAlta AS date) <= CAST(@fechaFin AS date))
            AND (@search IS NULL OR @search = ''
                 OR p.nombre        LIKE '%' + @search + '%'
                 OR c.observaciones LIKE '%' + @search + '%'
                 OR CAST(c.idCompra AS varchar(20)) LIKE '%' + @search + '%');

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT *, CAST(0 AS int) AS idProducto, CAST(0 AS int) AS idEstatusProducto
        FROM   #pagina
        ORDER BY
            CASE WHEN @col = 'fecha'     AND @dir = 'asc'  THEN fechaAlta END ASC,
            CASE WHEN @col = 'fecha'     AND @dir = 'desc' THEN fechaAlta END DESC,
            CASE WHEN @col = 'idcompra'  AND @dir = 'asc'  THEN idCompra  END ASC,
            CASE WHEN @col = 'idcompra'  AND @dir = 'desc' THEN idCompra  END DESC,
            CASE WHEN @col = 'proveedor' AND @dir = 'asc'  THEN proveedorNombre END ASC,
            CASE WHEN @col = 'proveedor' AND @dir = 'desc' THEN proveedorNombre END DESC,
            fechaAlta DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
