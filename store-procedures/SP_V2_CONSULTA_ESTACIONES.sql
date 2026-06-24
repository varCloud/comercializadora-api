/*
===============================================================================
 SP_V2_CONSULTA_ESTACIONES
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_ESTACIONES. NO reemplaza al legado:
 el sistema viejo sigue usando SP_CONSULTA_ESTACIONES; la API nueva usa este V2.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por nombre / número / almacén.
   - Devuelve DOS resultsets (convención nueva Notificacion<T>):
       1) cabecera: status, mensaje, total   (total = filas que cumplen el filtro)
       2) datos:    la página solicitada
   - Conserva los filtros del original (@idEstacion, @idAlmacen) y la regla de
     negocio (solo estaciones activas, idStatus = 1).

 Idempotente: usa CREATE OR ALTER, se puede ejecutar N veces sin error.

 Autor migración: equipo lluvia-migracion · 2026-06-20
 Origen: SP_CONSULTA_ESTACIONES (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_ESTACIONES]
    @idEstacion  int = 0,
    @idAlmacen   int = null,
    @search      varchar(100) = null,   -- búsqueda libre por nombre / número / almacén
    @pageNumber  int = 1,
    @pageSize    int = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0;

    -- Normalización de la paginación (defensa ante valores inválidos)
    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        -- Total de registros que cumplen el filtro (sin paginar) para el paginador
        SELECT @total = COUNT(1)
        FROM        Estaciones e
            INNER JOIN Almacenes a ON a.idAlmacen = e.idAlmacen
        WHERE   e.idStatus  = 1
            AND e.idEstacion = CASE WHEN @idEstacion > 0 THEN @idEstacion ELSE e.idEstacion END
            AND e.idAlmacen  = CASE WHEN @idAlmacen  > 0 THEN @idAlmacen  ELSE e.idAlmacen  END
            AND (@search IS NULL OR @search = ''
                 OR e.nombre        LIKE '%' + @search + '%'
                 OR a.Descripcion   LIKE '%' + @search + '%'
                 OR CAST(e.numero AS varchar(20)) LIKE '%' + @search + '%');

        -- Resultset 1: cabecera (status / mensaje / total)
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página solicitada
        SELECT
                e.idEstacion,
                e.idAlmacen,
                a.Descripcion AS nombreAlmacen,
                e.macAdress,
                e.nombre,
                e.numero,
                e.configurado,
                e.idUsuario,
                e.fechaAlta,
                e.idStatus,
                a.idSucursal
        FROM        Estaciones e
            INNER JOIN Almacenes a ON a.idAlmacen = e.idAlmacen
        WHERE   e.idStatus  = 1
            AND e.idEstacion = CASE WHEN @idEstacion > 0 THEN @idEstacion ELSE e.idEstacion END
            AND e.idAlmacen  = CASE WHEN @idAlmacen  > 0 THEN @idAlmacen  ELSE e.idAlmacen  END
            AND (@search IS NULL OR @search = ''
                 OR e.nombre        LIKE '%' + @search + '%'
                 OR a.Descripcion   LIKE '%' + @search + '%'
                 OR CAST(e.numero AS varchar(20)) LIKE '%' + @search + '%')
        ORDER BY e.nombre
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH

        -- En error: solo cabecera con status negativo. La API corta al ver status <> 200.
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;

    END CATCH
END
GO
