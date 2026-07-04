/*
===============================================================================
 SP_V2_CONSULTA_TIPOS_CLIENTES
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_TIPOS_CLIENTES. NO reemplaza al legado.

 El SP legado devuelve cabecera + resultset completo sin paginar (con contador
 ROW_NUMBER). Esta versión es el listado paginado de la pantalla "Tipos de
 cliente" (la antigua "Descuentos"), en la convención uniforme de la API nueva:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por descripción.
   - @idTipoCliente > 0 = obtener uno (para precargar el form de edición).
   - Orden dinámico (@order + @sort) con whitelist (descripcion | descuento);
     default descripcion asc (paridad con el legado).
   - Solo activos (activo = 1), igual que el legado.
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada (idTipoCliente, descripcion,
                    descuento, activo)

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-07-02
 Origen: SP_CONSULTA_TIPOS_CLIENTES (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_TIPOS_CLIENTES]
    @idTipoCliente int = 0,                -- > 0 = obtener uno; 0 = listar
    @search        varchar(100) = null,
    @order         varchar(50)  = null,    -- 'descripcion' | 'descuento'
    @sort          varchar(4)   = null,    -- 'asc' | 'desc'
    @pageNumber    int = 1,
    @pageSize      int = 10
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
        FROM    CatTipoCliente t
        WHERE   t.activo = 1
            AND t.idTipoCliente = CASE WHEN @idTipoCliente > 0 THEN @idTipoCliente ELSE t.idTipoCliente END
            AND (@search IS NULL OR @search = ''
                 OR t.descripcion LIKE '%' + @search + '%');

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
                t.idTipoCliente,
                t.descripcion,
                CAST(t.descuento AS decimal(18,2)) AS descuento,
                t.activo
        FROM    CatTipoCliente t
        WHERE   t.activo = 1
            AND t.idTipoCliente = CASE WHEN @idTipoCliente > 0 THEN @idTipoCliente ELSE t.idTipoCliente END
            AND (@search IS NULL OR @search = ''
                 OR t.descripcion LIKE '%' + @search + '%')
        ORDER BY
            CASE WHEN @col = 'descripcion' AND @dir = 'asc'  THEN t.descripcion END ASC,
            CASE WHEN @col = 'descripcion' AND @dir = 'desc' THEN t.descripcion END DESC,
            CASE WHEN @col = 'descuento'   AND @dir = 'asc'  THEN t.descuento   END ASC,
            CASE WHEN @col = 'descuento'   AND @dir = 'desc' THEN t.descuento   END DESC,
            t.descripcion ASC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
