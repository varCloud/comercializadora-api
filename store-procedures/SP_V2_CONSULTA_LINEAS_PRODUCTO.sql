/*
===============================================================================
 SP_V2_CONSULTA_LINEAS_PRODUCTO
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_LINEAS_PRODUCTO. NO reemplaza al legado.

 El SP legado devuelve UN solo resultset (status en cada fila) sin paginación.
 Esta versión es el listado paginado del submenú "Líneas de producto" (CRUD de
 mantenimiento del catálogo), en la convención uniforme de la API nueva:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por descripción.
   - @idLineaProducto > 0 = obtener una (para precargar el form de edición).
   - Orden dinámico (@order + @sort) con whitelist (descripcion); default asc.
   - Solo activas (activo = 1), igual que el legado / Fase A de Productos.
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada (idLineaProducto, descripcion, activo)

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-22
 Origen: SP_CONSULTA_LINEAS_PRODUCTO (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_LINEAS_PRODUCTO]
    @idLineaProducto int = 0,                 -- > 0 = obtener una; 0 = listar
    @search          varchar(100) = null,
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
        FROM    LineaProducto lp
        WHERE   lp.activo = 1
            AND lp.idLineaProducto = CASE WHEN @idLineaProducto > 0 THEN @idLineaProducto ELSE lp.idLineaProducto END
            AND (@search IS NULL OR @search = ''
                 OR lp.descripcion LIKE '%' + @search + '%');

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
                lp.idLineaProducto,
                lp.descripcion,
                lp.activo
        FROM    LineaProducto lp
        WHERE   lp.activo = 1
            AND lp.idLineaProducto = CASE WHEN @idLineaProducto > 0 THEN @idLineaProducto ELSE lp.idLineaProducto END
            AND (@search IS NULL OR @search = ''
                 OR lp.descripcion LIKE '%' + @search + '%')
        ORDER BY
            CASE WHEN @col = 'descripcion' AND @dir = 'asc'  THEN lp.descripcion END ASC,
            CASE WHEN @col = 'descripcion' AND @dir = 'desc' THEN lp.descripcion END DESC,
            lp.descripcion ASC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
