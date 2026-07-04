/*
===============================================================================
 SP_V2_CONSULTA_CLIENTES
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_CLIENTES. NO reemplaza al legado.

 El SP legado devuelve el cliente + su tipo en un multi-resultset sin paginar
 (splitOn idTipoCliente) e incluye adeudos de PedidosEspecialesCuentasPorCobrar.
 Esta versión:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por nombre/razón social (nombres + apellidos),
     RFC, teléfono, correo y municipio.
   - Orden dinámico (@order + @sort) con whitelist (nombre | rfc | municipio);
     default fechaAlta desc (paridad con el legado: más recientes primero).
   - @idCliente > 0 = obtener uno (para precargar el form de edición).
   - Resultset APLANADO: datos del tipo de cliente (idTipoCliente,
     tipoClienteDescripcion, descuento) y del régimen fiscal (idRegimenFiscal,
     regimenFiscal) como columnas del cliente, sin objeto anidado.
   - Persona moral: la razón social vive en la columna nombres (contrato de
     SP_INSERTA_ACTUALIZA_CLIENTES); aquí se separa en nombres/razonSocial
     según esPersonaMoral para que el front precargue el form sin lógica extra.
   - Solo activos (activo = 1), igual que el legado.
   - NO incluye adeudo/diasTranscurridos (módulo de crédito/cobranza, otra feature).
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-07-02
 Origen: SP_CONSULTA_CLIENTES (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_CLIENTES]
    @idCliente   int = 0,                -- > 0 = obtener uno; 0 = listar
    @search      varchar(100) = null,
    @order       varchar(50)  = null,    -- 'nombre' | 'rfc' | 'municipio'
    @sort        varchar(4)   = null,    -- 'asc' | 'desc'
    @pageNumber  int = 1,
    @pageSize    int = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4) = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        SELECT @total = COUNT(1)
        FROM    Clientes c
            INNER JOIN CatTipoCliente t ON t.idTipoCliente = c.idTipoCliente
        WHERE   c.activo = 1
            AND c.idCliente = CASE WHEN @idCliente > 0 THEN @idCliente ELSE c.idCliente END
            AND (@search IS NULL OR @search = ''
                 OR c.nombres         LIKE '%' + @search + '%'
                 OR c.apellidoPaterno LIKE '%' + @search + '%'
                 OR c.apellidoMaterno LIKE '%' + @search + '%'
                 OR c.rfc             LIKE '%' + @search + '%'
                 OR c.telefono        LIKE '%' + @search + '%'
                 OR c.correo          LIKE '%' + @search + '%'
                 OR c.municipio       LIKE '%' + @search + '%');

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
                c.idCliente,
                CAST(CASE WHEN COALESCE(c.esPersonaMoral, 0) = 1 THEN 1 ELSE 0 END AS bit) AS esPersonaMoral,
                CASE WHEN COALESCE(c.esPersonaMoral, 0) = 1 THEN '' ELSE COALESCE(c.nombres, '') END AS nombres,
                CASE WHEN COALESCE(c.esPersonaMoral, 0) = 1 THEN '' ELSE COALESCE(c.apellidoPaterno, '') END AS apellidoPaterno,
                CASE WHEN COALESCE(c.esPersonaMoral, 0) = 1 THEN '' ELSE COALESCE(c.apellidoMaterno, '') END AS apellidoMaterno,
                CASE WHEN COALESCE(c.esPersonaMoral, 0) = 1 THEN COALESCE(c.nombres, '') ELSE '' END AS razonSocial,
                COALESCE(c.sociedadMercantil, '') AS sociedadMercantil,
                LTRIM(RTRIM(UPPER(COALESCE(c.nombres, '') + ' ' + COALESCE(c.apellidoPaterno, '') + ' ' + COALESCE(c.apellidoMaterno, '')))) AS nombreCompleto,
                c.rfc,
                c.telefono,
                c.correo,
                c.calle,
                c.numeroExterior,
                c.numeroInterior,
                c.colonia,
                c.localidad,
                c.municipio,
                c.estado,
                c.cp,
                c.nombreContacto,
                c.latitud,
                c.longitud,
                c.nombreCompletoContacto AS nombreContactoPE,
                c.telefonoContacto       AS telefonoContactoPE,
                c.correoContacto         AS correoContactoPE,
                COALESCE(c.diasCredito, 0)        AS diasCredito,
                COALESCE(c.montoMaximoCredito, 0) AS montoMaximoCredito,
                CAST(CASE WHEN COALESCE(c.usarDatosCliente, 0) = 1 THEN 1 ELSE 0 END AS bit) AS usarDatosCliente,
                COALESCE(c.idCatRegimenFiscal, 0) AS idRegimenFiscal,
                COALESCE(rf.descripcion, '')      AS regimenFiscal,
                c.idTipoCliente,
                t.descripcion AS tipoClienteDescripcion,
                CAST(t.descuento AS decimal(18,2)) AS descuento,
                c.activo,
                c.fechaAlta
        FROM    Clientes c
            INNER JOIN CatTipoCliente t ON t.idTipoCliente = c.idTipoCliente
            LEFT  JOIN FactCatRegimenFiscal rf ON rf.idRegimenFiscal = c.idCatRegimenFiscal
        WHERE   c.activo = 1
            AND c.idCliente = CASE WHEN @idCliente > 0 THEN @idCliente ELSE c.idCliente END
            AND (@search IS NULL OR @search = ''
                 OR c.nombres         LIKE '%' + @search + '%'
                 OR c.apellidoPaterno LIKE '%' + @search + '%'
                 OR c.apellidoMaterno LIKE '%' + @search + '%'
                 OR c.rfc             LIKE '%' + @search + '%'
                 OR c.telefono        LIKE '%' + @search + '%'
                 OR c.correo          LIKE '%' + @search + '%'
                 OR c.municipio       LIKE '%' + @search + '%')
        ORDER BY
            CASE WHEN @col = 'nombre'    AND @dir = 'asc'  THEN c.nombres   END ASC,
            CASE WHEN @col = 'nombre'    AND @dir = 'desc' THEN c.nombres   END DESC,
            CASE WHEN @col = 'rfc'       AND @dir = 'asc'  THEN c.rfc       END ASC,
            CASE WHEN @col = 'rfc'       AND @dir = 'desc' THEN c.rfc       END DESC,
            CASE WHEN @col = 'municipio' AND @dir = 'asc'  THEN c.municipio END ASC,
            CASE WHEN @col = 'municipio' AND @dir = 'desc' THEN c.municipio END DESC,
            c.fechaAlta DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
