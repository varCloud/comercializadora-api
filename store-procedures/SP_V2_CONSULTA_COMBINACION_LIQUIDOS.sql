/*
===============================================================================
 SP_V2_CONSULTA_COMBINACION_LIQUIDOS
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_OBTENER_COMBINACION_PRODUCTOS_ENSAVDOS_A_AGRANEL.
 NO reemplaza al legado (este es el listado paginado del módulo "Relación Liquidos").

 El SP legado devuelve DOS resultsets (cabecera estatus/mensaje + todas las filas) sin
 paginación ni búsqueda. Esta versión sigue la convención uniforme de la API nueva:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por las descripciones de los 3 productos.
   - @idRelacionEnvasadoAgranel > 0 = obtener una (para precargar el form de edición).
   - Orden dinámico (@order + @sort) con whitelist (id | agranel | envasado | envase);
     default: id DESC (más recientes primero, igual que el legado).
   - Solo combinaciones activas (PE.activo = 1), igual que el legado.
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada con descripciones por JOIN a Productos

 Tabla origen: ProductosEnvasadosXAgranel (PE). Columnas (verificadas contra el script
   legado ALTER TABLE / SP_AGREGA_ACTUALIZA_...):
     idRelacionEnvasadoAgranel (PK), idProductoEnvasado, idProductoAgranel,
     idProducoEnvase (sic, typo legado), unidadMedidad (varchar, unidadSAT), valorUnidadMedida,
     activo, fechaAlta, idUnidadMedidad (FK CatUnidadMedida.idUnidadMedida).

 Idempotente: CREATE OR ALTER. Compat-120 safe (sin STRING_SPLIT/OPENJSON).
 Autor migración: equipo lluvia-migracion · 2026-06-24
 Origen: SP_OBTENER_COMBINACION_PRODUCTOS_ENSAVDOS_A_AGRANEL
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_COMBINACION_LIQUIDOS]
    @idRelacionEnvasadoAgranel int = 0,       -- > 0 = obtener una; 0 = listar
    @search                    varchar(100) = null,
    @order                     varchar(50)  = null,
    @sort                      varchar(4)   = null,   -- 'asc' | 'desc'
    @pageNumber                int = 1,
    @pageSize                  int = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4)  = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        SELECT @total = COUNT(1)
        FROM    ProductosEnvasadosXAgranel PE
            JOIN Productos PAgranel  ON PE.idProductoAgranel  = PAgranel.idProducto
            JOIN Productos PEnvasado ON PE.idProductoEnvasado = PEnvasado.idProducto
            JOIN Productos PEnvase   ON PE.idProducoEnvase    = PEnvase.idProducto
        WHERE   PE.activo = 1
            AND PE.idRelacionEnvasadoAgranel = CASE WHEN @idRelacionEnvasadoAgranel > 0
                                                    THEN @idRelacionEnvasadoAgranel
                                                    ELSE PE.idRelacionEnvasadoAgranel END
            AND (@search IS NULL OR @search = ''
                 OR PAgranel.descripcion  LIKE '%' + @search + '%'
                 OR PEnvasado.descripcion LIKE '%' + @search + '%'
                 OR PEnvase.descripcion   LIKE '%' + @search + '%');

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
                PE.idRelacionEnvasadoAgranel,
                PE.idProductoAgranel,
                PAgranel.descripcion   AS agranelDescripcion,
                PE.idProductoEnvasado,
                PEnvasado.descripcion  AS envasadoDescripcion,
                PE.idProducoEnvase,
                PEnvase.descripcion    AS envaseDescripcion,
                PE.idUnidadMedidad,
                PE.unidadMedidad,
                PE.valorUnidadMedida,
                PE.activo
        FROM    ProductosEnvasadosXAgranel PE
            JOIN Productos PAgranel  ON PE.idProductoAgranel  = PAgranel.idProducto
            JOIN Productos PEnvasado ON PE.idProductoEnvasado = PEnvasado.idProducto
            JOIN Productos PEnvase   ON PE.idProducoEnvase    = PEnvase.idProducto
        WHERE   PE.activo = 1
            AND PE.idRelacionEnvasadoAgranel = CASE WHEN @idRelacionEnvasadoAgranel > 0
                                                    THEN @idRelacionEnvasadoAgranel
                                                    ELSE PE.idRelacionEnvasadoAgranel END
            AND (@search IS NULL OR @search = ''
                 OR PAgranel.descripcion  LIKE '%' + @search + '%'
                 OR PEnvasado.descripcion LIKE '%' + @search + '%'
                 OR PEnvase.descripcion   LIKE '%' + @search + '%')
        ORDER BY
            CASE WHEN @col = 'id'       AND @dir = 'asc'  THEN PE.idRelacionEnvasadoAgranel END ASC,
            CASE WHEN @col = 'id'       AND @dir = 'desc' THEN PE.idRelacionEnvasadoAgranel END DESC,
            CASE WHEN @col = 'agranel'  AND @dir = 'asc'  THEN PAgranel.descripcion  END ASC,
            CASE WHEN @col = 'agranel'  AND @dir = 'desc' THEN PAgranel.descripcion  END DESC,
            CASE WHEN @col = 'envasado' AND @dir = 'asc'  THEN PEnvasado.descripcion END ASC,
            CASE WHEN @col = 'envasado' AND @dir = 'desc' THEN PEnvasado.descripcion END DESC,
            CASE WHEN @col = 'envase'   AND @dir = 'asc'  THEN PEnvase.descripcion   END ASC,
            CASE WHEN @col = 'envase'   AND @dir = 'desc' THEN PEnvase.descripcion   END DESC,
            PE.idRelacionEnvasadoAgranel DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
