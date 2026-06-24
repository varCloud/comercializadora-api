/*
===============================================================================
 SP_V2_CONSULTA_PROVEEDORES
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_PROVEEDORES. NO reemplaza al legado.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por nombre / descripción / teléfono / dirección.
   - Orden dinámico (@order + @sort) con whitelist (nombre | telefono); default nombre asc.
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada
   - Conserva las métricas del original (pedidos totales/incompletos/completos, % atendido)
     y el filtro activo = 1.

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-20
 Origen: SP_CONSULTA_PROVEEDORES (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_PROVEEDORES]
    @idProveedor int = 0,                -- > 0 = obtener uno; 0 = listar
    @search      varchar(100) = null,
    @order       varchar(50)  = null,
    @sort        varchar(4)   = null,   -- 'asc' | 'desc'
    @pageNumber  int = 1,
    @pageSize    int = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, 'nombre'));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        SELECT @total = COUNT(1)
        FROM    Proveedores p
        WHERE   p.activo = 1
            AND p.idProveedor = CASE WHEN @idProveedor > 0 THEN @idProveedor ELSE p.idProveedor END
            AND (@search IS NULL OR @search = ''
                 OR p.nombre      LIKE '%' + @search + '%'
                 OR p.descripcion LIKE '%' + @search + '%'
                 OR p.telefono    LIKE '%' + @search + '%'
                 OR p.direccion   LIKE '%' + @search + '%');

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
                p.idProveedor,
                p.nombre,
                p.descripcion,
                p.telefono,
                p.direccion,
                p.activo,
                COALESCE(pi.totalPedidosIncompletos, 0) AS totalPedidosIncompletos,
                COALESCE(pedidos.totalPedidosTotales, 0) AS totalPedidosTotales,
                COALESCE(pedidosCorrectos.totalPedidosCompletos, 0) AS totalPedidosCompletos,
                CASE WHEN COALESCE(pedidos.totalPedidosTotales, 0) = 0 THEN CAST(0 AS float)
                     ELSE (1 - ROUND(CAST((COALESCE(pi.totalPedidosIncompletos, 0) / COALESCE(pedidos.totalPedidosTotales, 0)) AS float), 2)) * 100
                END AS porcAtendido
        FROM    Proveedores p
            LEFT JOIN (
                SELECT COUNT(1) totalPedidosIncompletos, idProveedor
                FROM   Compras c
                WHERE  activo = 1 AND c.idStatusCompra IN (3, 5)
                   AND EXISTS (SELECT 1 FROM ComprasDetalle WHERE idCompra = c.idCompra AND idEstatusProductoCompra IN (3, 4, 5))
                GROUP BY idProveedor
            ) pi ON pi.idProveedor = p.idProveedor
            LEFT JOIN (
                SELECT COUNT(1) totalPedidosCompletos, idProveedor
                FROM   Compras c
                WHERE  activo = 1 AND c.idStatusCompra IN (3, 5)
                   AND NOT EXISTS (SELECT 1 FROM ComprasDetalle WHERE idCompra = c.idCompra AND idEstatusProductoCompra IN (2, 3, 4, 5))
                GROUP BY idProveedor
            ) pedidosCorrectos ON pedidosCorrectos.idProveedor = p.idProveedor
            LEFT JOIN (
                SELECT COUNT(1) totalPedidosTotales, idProveedor
                FROM   Compras c
                WHERE  activo = 1 AND c.idStatusCompra IN (3, 5)
                GROUP BY idProveedor
            ) pedidos ON pedidos.idProveedor = p.idProveedor
        WHERE   p.activo = 1
            AND p.idProveedor = CASE WHEN @idProveedor > 0 THEN @idProveedor ELSE p.idProveedor END
            AND (@search IS NULL OR @search = ''
                 OR p.nombre      LIKE '%' + @search + '%'
                 OR p.descripcion LIKE '%' + @search + '%'
                 OR p.telefono    LIKE '%' + @search + '%'
                 OR p.direccion   LIKE '%' + @search + '%')
        ORDER BY
            CASE WHEN @col = 'nombre'   AND @dir = 'asc'  THEN p.nombre   END ASC,
            CASE WHEN @col = 'nombre'   AND @dir = 'desc' THEN p.nombre   END DESC,
            CASE WHEN @col = 'telefono' AND @dir = 'asc'  THEN p.telefono END ASC,
            CASE WHEN @col = 'telefono' AND @dir = 'desc' THEN p.telefono END DESC,
            p.nombre ASC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
