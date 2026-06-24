/*
===============================================================================
 SP_V2_ACTUALIZA_STATUS_LINEAS_PRODUCTO
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_ACTUALIZA_STATUS_LINEAS_PRODUCTO. NO reemplaza al legado.

 Activa/desactiva (baja lógica) una línea de producto. Diferencia con el legado:
   - Al DESACTIVAR (@activo = 0), BLOQUEA la baja si la línea tiene productos
     ACTIVOS asociados (Productos.idLineaProducto), devolviendo error explícito.
     El SP legado no validaba esto (decisión P4 de la migración).
   - Reactivar (@activo = 1) no tiene restricción.

 Parámetros:
   @idLineaProducto  línea a modificar.
   @activo           nuevo estatus (1 activar, 0 desactivar).

 Devuelve UN resultset: status (200 ok / -1 error), mensaje.
 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-22
 Origen: SP_ACTUALIZA_STATUS_LINEAS_PRODUCTO (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_ACTUALIZA_STATUS_LINEAS_PRODUCTO]
    @idLineaProducto int,
    @activo          bit
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        IF NOT EXISTS (SELECT 1 FROM LineaProducto WHERE idLineaProducto = @idLineaProducto)
        BEGIN
            SELECT -1 AS status, 'No existe la línea de producto solicitada.' AS mensaje;
            RETURN;
        END

        -- Bloqueo de baja con productos asociados activos (decisión P4)
        IF (@activo = 0 AND EXISTS (
                SELECT 1 FROM Productos
                WHERE idLineaProducto = @idLineaProducto AND activo = 1))
        BEGIN
            SELECT -1 AS status,
                   'No se puede desactivar: la línea tiene productos activos asociados.' AS mensaje;
            RETURN;
        END

        UPDATE  LineaProducto
        SET     activo = @activo
        WHERE   idLineaProducto = @idLineaProducto;

        SELECT 200 AS status,
               CASE WHEN @activo = 1 THEN 'Línea de producto activada correctamente.'
                    ELSE 'Línea de producto desactivada correctamente.' END AS mensaje;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje;
    END CATCH
END
GO
