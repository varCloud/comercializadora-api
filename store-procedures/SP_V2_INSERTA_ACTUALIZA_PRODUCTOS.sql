/*
===============================================================================
 SP_V2_INSERTA_ACTUALIZA_PRODUCTOS
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_INSERTA_ACTUALIZA_PRODUCTOS. NO reemplaza al legado.

 Diferencia clave respecto al original:
   - El SP legado IGUALABA codigoBarras = @articulo (un solo dato). Aquí
     ARTÍCULO y CÓDIGO DE BARRAS son CAMPOS SEPARADOS: se reciben y persisten
     ambos por separado (@articulo, @codigoBarras).
   - La validación de "código de barras único entre activos" se hace contra la
     columna codigoBarras (no contra articulo).

 Salida: status / error_procedure / error_line / mensaje. NOTA: se usa la
 columna 'status' (minúscula) — convención de la API nueva que lee
 BaseRepository.EjecutarAsync (fila.status). El SP legado devolvía 'Estatus'
 (PascalCase); aquí se corrige para alinearse con el resto de SP_V2.

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-21
 Origen: SP_INSERTA_ACTUALIZA_PRODUCTOS (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_INSERTA_ACTUALIZA_PRODUCTOS]
    @idProducto            int,
    @descripcion           varchar(255),
    @idUnidadMedida        int,
    @idLineaProducto       int,
    @cantidadUnidadMedida  float,
    @codigoBarras          nvarchar(4000) = null,
    @activo                bit,
    @articulo              varchar(255),
    @claveProdServ         varchar(255),
    @idUnidadCompra        int = null,
    @cantidadUnidadCompra  int = null
AS
BEGIN

    BEGIN TRY

        DECLARE @status           int = 200,
                @mensaje          varchar(255) = 'Producto sin modificaciones',
                @error_line       varchar(255) = '',
                @error_procedure  varchar(255) = '',
                @existeProducto   bit = CAST(0 AS bit);

        IF EXISTS (SELECT 1 FROM Productos WHERE idProducto = @idProducto)
            SET @existeProducto = CAST(1 AS bit);

        -- ===== EDICIÓN =====
        IF (@idProducto > 0)
        BEGIN
            IF (@existeProducto = CAST(0 AS bit))
            BEGIN
                SET @mensaje = 'El Producto no existe.';
                RAISERROR (@mensaje, 11, -1);
            END

            -- Unicidad del CÓDIGO DE BARRAS (separado del artículo) entre activos
            IF (@codigoBarras IS NOT NULL AND LTRIM(RTRIM(@codigoBarras)) <> ''
                AND EXISTS (SELECT 1 FROM Productos
                            WHERE activo = 1
                              AND LTRIM(RTRIM(codigoBarras)) = LTRIM(RTRIM(@codigoBarras))
                              AND idProducto <> @idProducto))
            BEGIN
                SET @mensaje = 'Ya existe un codigo de barras asociado a un producto.';
                RAISERROR (@mensaje, 11, -1);
            END

            UPDATE  Productos
            SET     descripcion          = @descripcion,
                    idUnidadMedida       = @idUnidadMedida,
                    idLineaProducto      = @idLineaProducto,
                    cantidadUnidadMedida = @cantidadUnidadMedida,
                    codigoBarras         = @codigoBarras,
                    activo               = @activo,
                    articulo             = @articulo,
                    claveProdServ        = @claveProdServ,
                    idUnidadCompra       = @idUnidadCompra,
                    cantidadUnidadCompra = @cantidadUnidadCompra
            WHERE   idProducto = @idProducto;

            SET @mensaje = 'Producto modificado correctamente.';
        END
        -- ===== ALTA =====
        ELSE
        BEGIN
            IF (@codigoBarras IS NOT NULL AND LTRIM(RTRIM(@codigoBarras)) <> ''
                AND EXISTS (SELECT 1 FROM Productos
                            WHERE activo = 1
                              AND LTRIM(RTRIM(codigoBarras)) = LTRIM(RTRIM(@codigoBarras))))
            BEGIN
                SET @mensaje = 'Ya existe un codigo de barras asociado a un producto.';
                RAISERROR (@mensaje, 11, -1);
            END

            SET @activo = CAST(1 AS bit);

            INSERT INTO Productos
                (descripcion, idUnidadMedida, idLineaProducto, cantidadUnidadMedida,
                 codigoBarras, fechaAlta, activo, articulo, claveProdServ,
                 idUnidadCompra, cantidadUnidadCompra)
            VALUES
                (@descripcion, @idUnidadMedida, @idLineaProducto, @cantidadUnidadMedida,
                 @codigoBarras, dbo.FechaActual(), @activo, @articulo, @claveProdServ,
                 @idUnidadCompra, @cantidadUnidadCompra);

            SET @mensaje = 'Producto agregado correctamente.';
        END

    END TRY
    BEGIN CATCH
        SELECT  @status           = -ERROR_STATE(),
                @error_procedure  = ERROR_PROCEDURE(),
                @error_line       = ERROR_LINE(),
                @mensaje          = ERROR_MESSAGE();
    END CATCH

    SELECT  @status           AS status,
            @error_procedure  AS error_procedure,
            @error_line       AS error_line,
            @mensaje          AS mensaje;
END
GO
