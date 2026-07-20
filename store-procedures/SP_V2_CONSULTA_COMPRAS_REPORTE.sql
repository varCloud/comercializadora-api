/*
===============================================================================
 SP_V2_CONSULTA_COMPRAS_REPORTE
-------------------------------------------------------------------------------
 Listado paginado para "Reportes > Compras" (migración de
 ReportesController.BuscarCompras + ComprasDAO.ObtenerCompras(detalleCompra=true)
 del legado). NO reemplaza al legado ni a SP_V2_CONSULTA_COMPRAS.

 Por qué un SP nuevo y no reutilizar SP_V2_CONSULTA_COMPRAS (el que ya usa el
 módulo CRUD de Compras, Repositories/Compras/ComprasRepository.cs):
   - Mismas columnas de cabecera por compra (proveedor, comprador, fecha, total,
     estatus, conteo de productos) que ya calcula SP_V2_CONSULTA_COMPRAS — se
     parte de esa consulta para no reinventar la agregación.
   - Pero el reporte necesita filtrar por Línea de Producto (@idLineaProducto),
     que SP_V2_CONSULTA_COMPRAS no soporta (el CRUD de Compras no lo requiere).
   - Se aisla en un SP propio en vez de agregar el parámetro al SP existente
     para no tocar un SP ya usado por una pantalla distinta y ya aprobada
     (ComprasController/ComprasRepository); el costo de una consulta casi
     idéntica es bajo comparado con el riesgo de modificar un SP compartido.

 Novedades respecto a SP_CONSULTA_COMPRAS (legado, @detalleCompra = true):
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el
     legado no paginaba (traía todo a la pantalla vía DataTable).
   - Resultado agregado por compra (una fila por compra, con conteo de
     productos), no una fila por producto como hacía el legado con
     @detalleCompra = true.
   - Búsqueda libre (@search) por proveedor / observaciones / idCompra.
   - Filtro por línea de producto (@idLineaProducto) vía EXISTS contra
     ComprasDetalle + Productos.
   - Orden dinámico (@order + @sort) con whitelist (fecha | idcompra | proveedor).
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-07-20
 Origen: SP_CONSULTA_COMPRAS (Ernesto Aguilar, 2020-07-23) / SP_V2_CONSULTA_COMPRAS
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_COMPRAS_REPORTE]
    @search         varchar(150) = null,
    @idProveedor    int = null,
    @idStatusCompra int = null,
    @idUsuario      int = null,
    @idLineaProducto int = null,
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
            p.idProveedor,
            p.nombre                   AS proveedorNombre,
            s.idStatusCompra           AS idStatus,
            s.descripcion              AS estatusDescripcion,
            u.idUsuario,
            u.nombre + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '') AS nombreCompleto,
            det.montoTotal,
            det.totalCantProductos
        INTO #pagina
        FROM    Compras c
            JOIN Proveedores p     ON c.idProveedor = p.idProveedor
            JOIN CatStatusCompra s ON c.idStatusCompra = s.idStatusCompra
            JOIN Usuarios u        ON c.idUsuario = u.idUsuario
            JOIN (
                SELECT d.idCompra,
                       SUM(d.precio * CAST(d.cantidad AS float)) AS montoTotal,
                       SUM(d.cantidad)                            AS totalCantProductos
                FROM   ComprasDetalle d
                GROUP BY d.idCompra
            ) det ON det.idCompra = c.idCompra
        WHERE   c.activo = 1
            AND (@idProveedor     IS NULL OR c.idProveedor = @idProveedor)
            AND (@idStatusCompra  IS NULL OR c.idStatusCompra = @idStatusCompra)
            AND (@idUsuario       IS NULL OR c.idUsuario = @idUsuario)
            AND (@fechaInicio     IS NULL OR CAST(c.fechaAlta AS date) >= CAST(@fechaInicio AS date))
            AND (@fechaFin        IS NULL OR CAST(c.fechaAlta AS date) <= CAST(@fechaFin AS date))
            AND (@idLineaProducto IS NULL OR EXISTS (
                    SELECT 1
                    FROM   ComprasDetalle cd
                        JOIN Productos pr ON pr.idProducto = cd.idProducto
                    WHERE  cd.idCompra = c.idCompra
                        AND pr.idLineaProducto = @idLineaProducto))
            AND (@search IS NULL OR @search = ''
                 OR p.nombre        LIKE '%' + @search + '%'
                 OR c.observaciones LIKE '%' + @search + '%'
                 OR CAST(c.idCompra AS varchar(20)) LIKE '%' + @search + '%');

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT *
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
