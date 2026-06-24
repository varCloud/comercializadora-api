/*
===============================================================================
 SP_V2_CONSULTA_PRODUCTOS
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_PRODUCTOS. NO reemplaza al legado.

 El SP legado mezcla existencias por usuario/almacén, pedidos especiales y
 catálogo. Esta versión es CATÁLOGO PURO paginado para la pantalla de
 administración de Productos (Fase A):
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por descripción / artículo / código de barras.
   - Filtro opcional por línea de producto (@idLineaProducto).
   - @idProducto > 0 = obtener uno (para precargar el form de edición).
   - Orden dinámico (@order + @sort) con whitelist (descripcion | articulo);
     default descripcion asc.
   - Solo activos (activo = 1), igual que el legado.
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada (con descripciones de catálogo + fraccion)

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-21
 Origen: SP_CONSULTA_PRODUCTOS (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_PRODUCTOS]
    @idProducto      int = 0,                 -- > 0 = obtener uno; 0 = listar
    @search          varchar(100) = null,
    @idLineaProducto int = 0,                 -- 0 = todas
    @order           varchar(50)  = null,
    @sort            varchar(4)   = null,     -- 'asc' | 'desc'
    @pageNumber      int = 1,
    @pageSize        int = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, 'descripcion'));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        SELECT @total = COUNT(1)
        FROM    Productos p
        WHERE   p.activo = 1
            AND p.idProducto      = CASE WHEN @idProducto > 0 THEN @idProducto ELSE p.idProducto END
            AND p.idLineaProducto = CASE WHEN @idLineaProducto > 0 THEN @idLineaProducto ELSE p.idLineaProducto END
            AND (@search IS NULL OR @search = ''
                 OR p.descripcion  LIKE '%' + @search + '%'
                 OR p.articulo     LIKE '%' + @search + '%'
                 OR p.codigoBarras LIKE '%' + @search + '%');

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
                p.idProducto,
                p.descripcion,
                p.idUnidadMedida,
                um.descripcion              AS descripcionUnidadMedida,
                p.idLineaProducto,
                lp.descripcion              AS descripcionLinea,
                p.cantidadUnidadMedida,
                p.articulo,
                p.codigoBarras,
                p.claveProdServ,
                p.idUnidadCompra,
                uc.descripcion              AS descripcionUnidadCompra,
                p.cantidadUnidadCompra,
                p.precioIndividual,
                p.precioMenudeo,
                p.ultimoCostoCompra,
                p.activo,
                [dbo].[LineaProductoFraccion](p.idLineaProducto, null) AS fraccion
        FROM    Productos p
            LEFT JOIN LineaProducto  lp ON lp.idLineaProducto = p.idLineaProducto
            LEFT JOIN CatUnidadMedida um ON um.idUnidadMedida = p.idUnidadMedida
            LEFT JOIN CatUnidadCompra uc ON uc.idUnidadCompra = p.idUnidadCompra
        WHERE   p.activo = 1
            AND p.idProducto      = CASE WHEN @idProducto > 0 THEN @idProducto ELSE p.idProducto END
            AND p.idLineaProducto = CASE WHEN @idLineaProducto > 0 THEN @idLineaProducto ELSE p.idLineaProducto END
            AND (@search IS NULL OR @search = ''
                 OR p.descripcion  LIKE '%' + @search + '%'
                 OR p.articulo     LIKE '%' + @search + '%'
                 OR p.codigoBarras LIKE '%' + @search + '%')
        ORDER BY
            CASE WHEN @col = 'descripcion' AND @dir = 'asc'  THEN p.descripcion END ASC,
            CASE WHEN @col = 'descripcion' AND @dir = 'desc' THEN p.descripcion END DESC,
            CASE WHEN @col = 'articulo'    AND @dir = 'asc'  THEN p.articulo    END ASC,
            CASE WHEN @col = 'articulo'    AND @dir = 'desc' THEN p.articulo    END DESC,
            p.descripcion ASC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
