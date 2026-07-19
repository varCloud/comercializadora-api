/*
===============================================================================
 SP_V2_CONSULTA_DETALLE_PEDIDOS_INTERNOS
-------------------------------------------------------------------------------
 Versión 2 (migración) del detalle/timeline de un pedido interno para la pantalla
 "Bitácoras". Devuelve el historial de cambios de estatus del folio a partir de
 PedidosInternosLog. NO reemplaza al legado (SP_CONSULTA_DETALLE_PEDIDOS_INTERNOS).

 Cambios respecto al original:
   - Devuelve DOS resultsets (convención Notificacion<T>):
       1) cabecera: status, mensaje
       2) datos:    los pasos del timeline.
   - ORDER BY fechaAlta ASC explícito: el SP legado no ordenaba el resultset final
     y la vista asumía orden cronológico para calcular el tiempo transcurrido
     entre pasos. Se hace explícito para que ese cálculo (que hace el front) sea
     correcto y estable.

 Conserva la lógica del original:
   - Fuente PedidosInternosLog (un registro por transición de estatus).
   - LEFT JOIN a Almacén destino (puede ser null en algún paso), JOIN al resto.
   - La observación del paso "Rechazado" (estatus 3) se toma de
     MovimientosDeMercancia (idEstatusPedidoInterno = 3); el resto: "Sin Observación".

 Idempotente: usa CREATE OR ALTER.

 Autor migración: equipo lluvia-migracion · 2026-07-14
 Origen: SP_CONSULTA_DETALLE_PEDIDOS_INTERNOS (Jessica Almonte Acosta, 2020-04-27;
 definición leída de BD DB_A57E86_comercializadora, 2026-07-14)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_DETALLE_PEDIDOS_INTERNOS]
    @idPedidoInterno int
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK';

    BEGIN TRY

        SELECT
            pi.idPedidoInterno,
            pi.fechaAlta,
            CAST('Sin Observación' AS varchar(300))                                    AS observacion,
            pi.idAlmacenOrigen,
            ao.Descripcion                                                             AS almacenOrigen,
            pi.idAlmacenDestino,
            ad.Descripcion                                                             AS almacenDestino,
            pi.idUsuario,
            COALESCE(u.nombre, '') + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '') AS nombreCompleto,
            pi.IdEstatusPedidoInterno                                                  AS idStatus,
            s.descripcion                                                              AS descripcionEstatus
        INTO #log
        FROM        PedidosInternosLog pi
            JOIN    CatEstatusPedidoInterno s  ON pi.IdEstatusPedidoInterno = s.IdEstatusPedidoInterno
            JOIN    Almacenes               ao ON pi.idAlmacenOrigen = ao.idAlmacen
            LEFT JOIN Almacenes             ad ON pi.idAlmacenDestino = ad.idAlmacen
            JOIN    Usuarios                u  ON pi.idUsuario = u.idUsuario
        WHERE   pi.idPedidoInterno = @idPedidoInterno;

        -- Observación del paso "Rechazado" (estatus 3) desde MovimientosDeMercancia.
        UPDATE  #log
        SET     #log.observacion = COALESCE(a.observaciones, 'Sin Observación')
        FROM    (
                    SELECT TOP 1 idPedidoInterno, observaciones
                    FROM   MovimientosDeMercancia
                    WHERE  idPedidoInterno = @idPedidoInterno
                       AND idEstatusPedidoInterno = 3
                ) a
        WHERE   #log.idPedidoInterno = a.idPedidoInterno
            AND #log.idStatus = 3;

        IF NOT EXISTS (SELECT 1 FROM #log)
        BEGIN
            SET @status  = -1;
            SET @mensaje = 'No se encontraron coincidencias.';
        END

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje;

        -- Resultset 2: pasos del timeline (cronológico ascendente)
        SELECT
            idPedidoInterno,
            fechaAlta,
            observacion,
            idAlmacenOrigen,
            almacenOrigen,
            idAlmacenDestino,
            almacenDestino,
            idUsuario,
            nombreCompleto,
            idStatus,
            descripcionEstatus
        FROM   #log
        ORDER BY fechaAlta ASC;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje;
    END CATCH
END
GO
