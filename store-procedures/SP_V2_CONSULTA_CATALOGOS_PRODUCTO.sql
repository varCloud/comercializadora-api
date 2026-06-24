/*
===============================================================================
 SP_V2_CONSULTA_CATALOGOS_PRODUCTO
-------------------------------------------------------------------------------
 Catálogos de apoyo del formulario de Productos, en la convención uniforme de
 la API nueva (DOS resultsets: cabecera status/mensaje + datos id/descripcion),
 para que el repositorio los lea con ConsultarAsync<CatalogoItem>.

 Los SP legados de estos catálogos tienen formas inconsistentes entre sí
 (SP_CONSULTA_LINEAS_PRODUCTO = 1 resultset con status en la fila;
  SP_CONSULTA_UNIDADES_MEDIDA = 2 resultsets;
  SP_CONSULTA_UNIDADES_COMPRA = sin cabecera). Este V2 los unifica sin tocarlos.

   @tipo = 'lineas'           -> LineaProducto activos
   @tipo = 'unidades-medida'  -> CatUnidadMedida
   @tipo = 'unidades-compra'  -> CatUnidadCompra

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-21
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_CATALOGOS_PRODUCTO]
    @tipo varchar(30)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- Resultset 1: cabecera
        SELECT 200 AS status, 'OK' AS mensaje;

        -- Resultset 2: datos (id, descripcion)
        IF (@tipo = 'lineas')
            SELECT idLineaProducto AS id, descripcion
            FROM   LineaProducto
            WHERE  activo = 1
            ORDER BY descripcion;
        ELSE IF (@tipo = 'unidades-medida')
            SELECT idUnidadMedida AS id, descripcion
            FROM   CatUnidadMedida
            ORDER BY descripcion;
        ELSE IF (@tipo = 'unidades-compra')
            SELECT CAST(idUnidadCompra AS int) AS id, descripcion
            FROM   CatUnidadCompra
            ORDER BY descripcion;
        ELSE
            SELECT CAST(NULL AS int) AS id, CAST(NULL AS varchar(255)) AS descripcion
            WHERE 1 = 0;
    END TRY
    BEGIN CATCH
        -- Si la cabecera ya se emitió, devolvemos un resultset de datos vacío coherente.
        SELECT CAST(NULL AS int) AS id, CAST(NULL AS varchar(255)) AS descripcion
        WHERE 1 = 0;
    END CATCH
END
GO
