/*
===============================================================================
 SP_V2_CONSULTA_FACTURAS_PEDIDOS_ESPECIALES
-------------------------------------------------------------------------------
 Versión 2 (migración) del listado de SP_FACTURACION_OBTENER_FACTURAS_PEDIDOS_
 ESPECIALES. NO reemplaza al legado (sigue en uso por el sistema viejo).

 Espejo de SP_V2_CONSULTA_FACTURAS (ventas) sobre FacturasPedidosEspeciales +
 PedidosEspeciales. Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el legado
     devolvía TOP 50 solo cuando no había ningún filtro.
   - Búsqueda libre (@search) por # de pedido / cliente / usuario de facturación.
   - Orden dinámico (@order + @sort) con whitelist (fecha | idpedido | cliente |
     estatus); default fecha desc (igual que el legado).
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada
   - A diferencia de la variante de ventas, aquí el filtro @idUsuario del legado
     SÍ funcionaba (no estaba comentado); se conserva con la misma semántica.
   - Sin resultados → status=200 con total=0 (lista vacía, no error).

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-07-15
 Origen: SP_FACTURACION_OBTENER_FACTURAS_PEDIDOS_ESPECIALES
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_FACTURAS_PEDIDOS_ESPECIALES]
    @search          varchar(150) = null,
    @idStatusFactura int = null,
    @idUsuario       int = null,
    @fechaInicio     date = null,
    @fechaFin        date = null,
    @order           varchar(50) = null,
    @sort            varchar(4)  = null,   -- 'asc' | 'desc'
    @pageNumber      int = 1,
    @pageSize        int = 25
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'asc' THEN 'asc'
                                       WHEN LOWER(@sort) = 'desc' THEN 'desc'
                                       ELSE 'desc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 25;

    BEGIN TRY

        SELECT
            f.idFacturaPedidoEspecial,
            f.idPedidoEspecial,
            f.fecha,
            f.fechaTimbrado,
            f.UUID                                                                                AS uuid,
            f.idEstatusFactura,
            s.descripcion,
            COALESCE(c.nombres, '') + ' ' + COALESCE(c.apellidoPaterno, '') + ' ' + COALESCE(c.apellidoMaterno, '')   AS nombreCliente,
            COALESCE(u.nombre, '') + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '')    AS nombreUsuarioFacturacion,
            COALESCE(uc.nombre, '') + ' ' + COALESCE(uc.apellidoPaterno, '') + ' ' + COALESCE(uc.apellidoMaterno, '') AS nombreUsuarioCancelacion,
            f.fechaCancelacion,
            CASE WHEN f.idEstatusFactura = 2 THEN f.msjErrorCancelacion
                 WHEN f.idEstatusFactura = 3 THEN f.msjErrorFacturacion
                 ELSE '' END                                                                       AS mensajeError,
            v.codigoBarras,
            v.montoTotal,
            f.pathArchivoFactura
        INTO #pagina
        FROM    FacturasPedidosEspeciales f
            JOIN FacCatEstatusFactura s ON f.idEstatusFactura = s.idEstatusFactura
            JOIN PedidosEspeciales v    ON f.idPedidoEspecial = v.idPedidoEspecial
            JOIN Clientes c             ON v.idCliente = c.idCliente
            JOIN Usuarios u             ON f.idUsuarioFacturacion = u.idUsuario
            LEFT JOIN Usuarios uc       ON f.idUsuarioCancelacion = uc.idUsuario
        WHERE   (@idStatusFactura IS NULL OR f.idEstatusFactura = @idStatusFactura)
            AND (@idUsuario       IS NULL OR f.idUsuarioFacturacion = @idUsuario)
            AND (@fechaInicio     IS NULL OR CAST(f.fecha AS date) >= @fechaInicio)
            AND (@fechaFin        IS NULL OR CAST(f.fecha AS date) <= @fechaFin)
            AND (@search IS NULL OR @search = ''
                 OR CAST(f.idPedidoEspecial AS varchar(20)) LIKE '%' + @search + '%'
                 OR c.nombres         LIKE '%' + @search + '%'
                 OR u.nombre          LIKE '%' + @search + '%');

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT *
        FROM   #pagina
        ORDER BY
            CASE WHEN @col = 'fecha'    AND @dir = 'asc'  THEN fecha END ASC,
            CASE WHEN @col = 'fecha'    AND @dir = 'desc' THEN fecha END DESC,
            CASE WHEN @col = 'idpedido' AND @dir = 'asc'  THEN idPedidoEspecial END ASC,
            CASE WHEN @col = 'idpedido' AND @dir = 'desc' THEN idPedidoEspecial END DESC,
            CASE WHEN @col = 'cliente'  AND @dir = 'asc'  THEN nombreCliente END ASC,
            CASE WHEN @col = 'cliente'  AND @dir = 'desc' THEN nombreCliente END DESC,
            CASE WHEN @col = 'estatus'  AND @dir = 'asc'  THEN idEstatusFactura END ASC,
            CASE WHEN @col = 'estatus'  AND @dir = 'desc' THEN idEstatusFactura END DESC,
            fecha DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
