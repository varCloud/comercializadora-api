/*
===============================================================================
 SP_V2_CONSULTA_COMPRA_DETALLE
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_COMPRAS con @detalleCompra = 1, acotado a
 una sola compra y con columnas LIMPIAS (sin las colisiones de nombre del SP
 legado, donde `descripcion`/`observaciones`/`idEstatusProducto` aparecían dos
 veces). Alimenta la precarga del modal de alta/edición de Compras. NO reemplaza
 al legado.

 Devuelve TRES resultsets:
   1) cabecera: status, mensaje
   2) cabecera de la compra (un registro)
   3) detalle de productos de la compra

 Replica el `fraccion` con dbo.LineaProductoFraccion y el estatus por producto
 (CatEstatusProductoCompra). idEstatusProducto = 0 => "Pendiente".

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-22
 Origen: SP_CONSULTA_COMPRAS (@detalleCompra = 1) (Ernesto Aguilar, 2020-07-23)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_COMPRA_DETALLE]
    @idCompra int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK';

    IF NOT EXISTS (SELECT 1 FROM Compras WHERE idCompra = @idCompra AND activo = 1)
    BEGIN
        SET @status  = -1;
        SET @mensaje = 'La compra no existe o fue eliminada.';
    END

    -- Resultset 1: cabecera (status / mensaje)
    SELECT @status AS status, @mensaje AS mensaje;

    IF (@status <> 200)
        RETURN;

    -- Resultset 2: cabecera de la compra
    SELECT
        c.idCompra,
        c.fechaAlta,
        c.observaciones,
        COALESCE(a.idAlmacen, 0)    AS idAlmacen,
        COALESCE(a.Descripcion, '') AS almacen,
        p.idProveedor,
        p.nombre                    AS proveedorNombre,
        s.idStatusCompra            AS idStatus,
        s.descripcion               AS estatusDescripcion,
        u.idUsuario,
        u.nombre + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '') AS nombreCompleto
    FROM    Compras c
        JOIN Proveedores p     ON c.idProveedor = p.idProveedor
        JOIN CatStatusCompra s ON c.idStatusCompra = s.idStatusCompra
        JOIN Usuarios u        ON c.idUsuario = u.idUsuario
        LEFT JOIN Almacenes a  ON c.idAlmacen = a.idAlmacen
    WHERE   c.idCompra = @idCompra;

    -- Resultset 3: detalle de productos
    SELECT
        d.idProducto,
        p.descripcion,
        COALESCE(d.idEstatusProductoCompra, 0) AS idEstatusProducto,
        COALESCE(ep.descripcion, 'Pendiente')  AS estatusProducto,
        d.observaciones,
        COALESCE(d.cantidadRecibida, 0)        AS cantidadRecibida,
        COALESCE(d.cantidadDevuelta, 0)        AS cantidadDevuelta,
        d.cantidad,
        d.precio,
        (d.precio * CAST(d.cantidad AS float)) AS total,
        dbo.LineaProductoFraccion(p.idLineaProducto, p.idProducto) AS fraccion
    FROM    ComprasDetalle d
        JOIN Productos p ON d.idProducto = p.idProducto
        LEFT JOIN CatEstatusProductoCompra ep ON d.idEstatusProductoCompra = ep.idEstatusProductoCompra
    WHERE   d.idCompra = @idCompra
    ORDER BY p.descripcion;
END
GO
