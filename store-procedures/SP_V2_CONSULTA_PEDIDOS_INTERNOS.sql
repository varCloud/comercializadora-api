/*
===============================================================================
 SP_V2_CONSULTA_PEDIDOS_INTERNOS
-------------------------------------------------------------------------------
 Versión 2 (migración) de la consulta "Bitácoras" (pedidos internos = traspasos
 de producto entre almacenes). NO reemplaza al legado: el sistema viejo sigue
 usando SP_CONSULTA_PEDIDOS_INTERNOS; la API nueva (reporte de solo lectura) usa
 este V2.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el legado
     devolvía TOP 50 sin filtros o TODO el resultset con filtros (DataTables
     paginaba en memoria). PedidosInternos tiene ~148k filas → paginación real.
   - Orden dinámico opcional (@order + @sort; whitelist folio|fecha|cantidad),
     default fechaAlta DESC (igual que el legado: "order by fechaAlta desc").
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada.

 Conserva la MISMA lógica de filtrado que el original:
   - Todos los filtros son opcionales (patrón COALESCE(@x, columna)).
   - @idTipoPedidoInterno default 1 (el reporte web solo muestra tipo 1, igual
     que el legado que lo forzaba a 1 por default; no se expone en el form).
   - @fechaIni / @fechaFin: rango opcional e independiente por fecha (CAST a date).
   - JOIN (no LEFT JOIN) a almacenes/usuario/producto: mismo comportamiento que
     el legado (un pedido sin esas relaciones no se lista).
   - La visibilidad por rol (no-admin → solo sus pedidos) la resuelve la API
     pasando @idUsuario desde el JWT; aquí el SP solo filtra por @idUsuario.

 Idempotente: usa CREATE OR ALTER.

 Autor migración: equipo lluvia-migracion · 2026-07-14
 Origen: SP_CONSULTA_PEDIDOS_INTERNOS (Jessica Almonte Acosta, 2020-04-27;
 definición leída de BD DB_A57E86_comercializadora, 2026-07-14)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_PEDIDOS_INTERNOS]
    @idPedidoInterno         int      = null,
    @idEstatusPedidoInterno  int      = null,
    @idAlmacenOrigen         int      = null,
    @idAlmacenDestino        int      = null,
    @idUsuario               int      = null,
    @idProducto              int      = null,
    @fechaIni                datetime = null,
    @fechaFin                datetime = null,
    @idTipoPedidoInterno     int      = 1,
    @order                   varchar(50) = null,
    @sort                    varchar(4)  = null,   -- 'asc' | 'desc'
    @pageNumber              int      = 1,
    @pageSize                int      = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'asc' THEN 'asc' ELSE 'desc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;
    IF (@idTipoPedidoInterno IS NULL) SET @idTipoPedidoInterno = 1;

    BEGIN TRY

        SELECT
            pi.idPedidoInterno,
            pi.fechaAlta,
            pi.idAlmacenOrigen,
            ao.Descripcion                                                             AS almacenOrigen,
            pi.idAlmacenDestino,
            ad.Descripcion                                                             AS almacenDestino,
            pi.idUsuario,
            COALESCE(u.nombre, '') + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '') AS nombreCompleto,
            pi.IdEstatusPedidoInterno                                                  AS idStatus,
            s.descripcion                                                              AS descripcionEstatus,
            pid.idProducto,
            p.descripcion                                                              AS descripcionProducto,
            pid.cantidad
        INTO #pagina
        FROM        PedidosInternos pi
            JOIN    PedidosInternosDetalle    pid ON pi.idPedidoInterno = pid.idPedidoInterno
            JOIN    CatEstatusPedidoInterno   s   ON pi.IdEstatusPedidoInterno = s.IdEstatusPedidoInterno
            JOIN    Almacenes                 ao  ON pi.idAlmacenOrigen = ao.idAlmacen
            JOIN    Almacenes                 ad  ON pi.idAlmacenDestino = ad.idAlmacen
            JOIN    Usuarios                  u   ON pi.idUsuario = u.idUsuario
            JOIN    Productos                 p   ON pid.idProducto = p.idProducto
        WHERE   pi.idPedidoInterno        = COALESCE(@idPedidoInterno, pi.idPedidoInterno)
            AND pi.IdEstatusPedidoInterno = COALESCE(@idEstatusPedidoInterno, pi.IdEstatusPedidoInterno)
            AND pi.idAlmacenOrigen        = COALESCE(@idAlmacenOrigen, pi.idAlmacenOrigen)
            AND pi.idAlmacenDestino       = COALESCE(@idAlmacenDestino, pi.idAlmacenDestino)
            AND pi.idUsuario              = COALESCE(@idUsuario, pi.idUsuario)
            AND pid.idProducto            = COALESCE(@idProducto, pid.idProducto)
            AND CAST(pi.fechaAlta AS date) >= COALESCE(CAST(@fechaIni AS date), CAST(pi.fechaAlta AS date))
            AND CAST(pi.fechaAlta AS date) <= COALESCE(CAST(@fechaFin AS date), CAST(pi.fechaAlta AS date))
            AND pi.idTipoPedidoInterno    = @idTipoPedidoInterno;

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
            idPedidoInterno,
            fechaAlta,
            idAlmacenOrigen,
            almacenOrigen,
            idAlmacenDestino,
            almacenDestino,
            idUsuario,
            nombreCompleto,
            idStatus,
            descripcionEstatus,
            idProducto,
            descripcionProducto,
            cantidad
        FROM   #pagina
        ORDER BY
            CASE WHEN @col = 'folio'    AND @dir = 'asc'  THEN idPedidoInterno    END ASC,
            CASE WHEN @col = 'folio'    AND @dir = 'desc' THEN idPedidoInterno    END DESC,
            CASE WHEN @col = 'fecha'    AND @dir = 'asc'  THEN fechaAlta          END ASC,
            CASE WHEN @col = 'fecha'    AND @dir = 'desc' THEN fechaAlta          END DESC,
            CASE WHEN @col = 'cantidad' AND @dir = 'asc'  THEN cantidad           END ASC,
            CASE WHEN @col = 'cantidad' AND @dir = 'desc' THEN cantidad           END DESC,
            fechaAlta DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
