/*
===============================================================================
 SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_INSERTA_ACTUALIZA_LINEAS_PRODUCTO. NO reemplaza al legado.

 Alta/edición de una línea de producto del catálogo. Diferencias con el legado:
   - La unicidad de descripción se valida y, ante duplicado, devuelve ERROR
     EXPLÍCITO (status -1 + mensaje) en vez del no-op silencioso con status 200
     "sin modificaciones" del SP legado (que confundía: parecía guardado OK).
   - La descripción se recorta (LTRIM/RTRIM) antes de validar/persistir.

 Parámetros:
   @idLineaProducto  0 = alta; > 0 = edición.
   @descripcion      requerida; única (case-insensitive) entre todas las líneas.
   @activo           estatus (en alta el SP lo fuerza a 1).

 Devuelve UN resultset: status (200 ok / -1 error), mensaje.
 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-22
 Origen: SP_INSERTA_ACTUALIZA_LINEAS_PRODUCTO (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_INSERTA_ACTUALIZA_LINEAS_PRODUCTO]
    @idLineaProducto int,
    @descripcion     varchar(50),
    @activo          bit = 1
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK';

    BEGIN TRY

        SET @descripcion = LTRIM(RTRIM(@descripcion));

        IF (@descripcion IS NULL OR @descripcion = '')
        BEGIN
            SELECT -1 AS status, 'La descripción es obligatoria.' AS mensaje;
            RETURN;
        END

        -- Edición
        IF (@idLineaProducto > 0)
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM LineaProducto WHERE idLineaProducto = @idLineaProducto)
            BEGIN
                SELECT -1 AS status, 'La línea de producto no existe.' AS mensaje;
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM LineaProducto
                       WHERE descripcion = @descripcion
                         AND idLineaProducto <> @idLineaProducto)
            BEGIN
                SELECT -1 AS status, 'Ya existe otra línea de producto con esa descripción.' AS mensaje;
                RETURN;
            END

            UPDATE  LineaProducto
            SET     descripcion = @descripcion,
                    activo      = @activo
            WHERE   idLineaProducto = @idLineaProducto;

            SELECT 200 AS status, 'Línea de producto modificada correctamente.' AS mensaje;
            RETURN;
        END

        -- Alta
        IF EXISTS (SELECT 1 FROM LineaProducto WHERE descripcion = @descripcion)
        BEGIN
            SELECT -1 AS status, 'Ya existe una línea de producto con esa descripción.' AS mensaje;
            RETURN;
        END

        INSERT INTO LineaProducto (descripcion, activo)
        VALUES (@descripcion, 1);

        SELECT 200 AS status, 'Línea de producto agregada correctamente.' AS mensaje;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje;
    END CATCH
END
GO
