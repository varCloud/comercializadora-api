/*
===============================================================================
 SP_V2_CONSULTA_PRECIOS_PRODUCTO
-------------------------------------------------------------------------------
 Versión 2 (migración) — Precios de un producto (Fase B). Reúne lo que el legado
 traía por separado: los precios base (tabla Productos) + los rangos de mayoreo
 (ProductosPorPrecio). NO reemplaza a SP_CONSULTA_TIPOS_DE_PRECIOS.

 Devuelve TRES resultsets:
   1) cabecera: status, mensaje
   2) precios base (1 fila): precioIndividual, precioMenudeo, ultimoCostoCompra,
      porcUtilidadIndividual, porcUtilidadMayoreo
   3) rangos activos: contador, idProducto, min, max, costo, porcUtilidad

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-21
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_PRECIOS_PRODUCTO]
    @idProducto int
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Resultset 1: cabecera
        SELECT 200 AS status, 'OK' AS mensaje;

        -- Resultset 2: precios base (tabla Productos)
        SELECT  p.precioIndividual,
                p.precioMenudeo,
                p.ultimoCostoCompra,
                p.porcUtilidadIndividual,
                p.porcUtilidadMayoreo
        FROM    Productos p
        WHERE   p.idProducto = @idProducto;

        -- Resultset 3: rangos de mayoreo activos
        SELECT  contador,
                idProducto,
                [min],
                [max],
                costo,
                porcUtilidad
        FROM    ProductosPorPrecio
        WHERE   idProducto = @idProducto AND activo = CAST(1 AS bit)
        ORDER BY [min];
    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje;
    END CATCH
END
GO
