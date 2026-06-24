/*
===============================================================================
 SP_V2_CONSULTA_USUARIOS
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_CONSULTA_USUARIOS. NO reemplaza al legado:
 el sistema viejo sigue usando SP_CONSULTA_USUARIOS; la API nueva usa este V2.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Devuelve DOS resultsets (convención nueva Notificacion<T>):
       1) cabecera: status, mensaje, total   (total = filas que cumplen el filtro)
       2) datos:    la página solicitada
   - Conserva los filtros del original (@idUsuario, @idAlmacen, @idRol) y las
     reglas de negocio (solo activos, excluye rol administrador idRol = 1).

 Idempotente: usa CREATE OR ALTER, se puede ejecutar N veces sin error.

 Autor migración: equipo lluvia-migracion · 2026-06-20
 Origen: SP_CONSULTA_USUARIOS (Ernesto Aguilar, 2020-02-17)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_USUARIOS]
    @idUsuario   int = 0,
    @idAlmacen   int = null,
    @idRol       int = null,
    @search      varchar(100) = null,   -- búsqueda libre por nombre/apellidos/usuario
    @pageNumber  int = 1,
    @pageSize    int = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0;

    -- Normalización de la paginación (defensa ante valores inválidos)
    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        -- Total de registros que cumplen el filtro (sin paginar) para el paginador
        SELECT @total = COUNT(1)
        FROM        Usuarios u
            LEFT JOIN catRoles      r ON r.idRol      = u.idRol
            LEFT JOIN CatSucursales s ON s.idSucursal = u.idSucursal
            LEFT JOIN Almacenes     a ON a.idAlmacen  = u.idAlmacen
        WHERE   u.idUsuario = CASE WHEN @idUsuario > 0 THEN @idUsuario ELSE u.idUsuario END
            AND u.activo    = CAST(1 AS bit)
            AND r.idRol    <> 1  -- excluye administrador
            AND a.idAlmacen = CASE WHEN @idAlmacen > 0 THEN @idAlmacen ELSE a.idAlmacen END
            AND u.idRol     = CASE WHEN @idRol     > 0 THEN @idRol     ELSE u.idRol     END
            AND (@search IS NULL OR @search = ''
                 OR u.nombre          LIKE '%' + @search + '%'
                 OR u.apellidoPaterno LIKE '%' + @search + '%'
                 OR u.apellidoMaterno LIKE '%' + @search + '%'
                 OR u.usuario         LIKE '%' + @search + '%');

        -- Resultset 1: cabecera (status / mensaje / total)
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página solicitada
        SELECT
                u.idUsuario,
                u.idRol,
                u.usuario AS nombreUsuario,                      -- alias para mapear a Usuario.NombreUsuario (JSON: "usuario")
                u.contrasena,                                   -- uso interno (la API la oculta con [JsonIgnore])
                u.telefono,
                u.idAlmacen,
                u.idSucursal,
                u.nombre,
                ISNULL(u.apellidoPaterno, '') AS apellidoPaterno,
                ISNULL(u.apellidoMaterno, '') AS apellidoMaterno,
                u.fecha_alta AS fechaAlta,                       -- alias para mapear a Usuario.FechaAlta (Dapper no estripa "_")
                u.activo,
                r.descripcion AS descripcionRol,
                s.descripcion AS descripcionSucursal,
                a.descripcion AS descripcionAlmacen,
                u.nombre + ' ' + COALESCE(u.apellidoPaterno, '') + ' ' + COALESCE(u.apellidoMaterno, '') AS nombreCompleto
        FROM        Usuarios u
            LEFT JOIN catRoles      r ON r.idRol      = u.idRol
            LEFT JOIN CatSucursales s ON s.idSucursal = u.idSucursal
            LEFT JOIN Almacenes     a ON a.idAlmacen  = u.idAlmacen
        WHERE   u.idUsuario = CASE WHEN @idUsuario > 0 THEN @idUsuario ELSE u.idUsuario END
            AND u.activo    = CAST(1 AS bit)
            AND r.idRol    <> 1
            AND a.idAlmacen = CASE WHEN @idAlmacen > 0 THEN @idAlmacen ELSE a.idAlmacen END
            AND u.idRol     = CASE WHEN @idRol     > 0 THEN @idRol     ELSE u.idRol     END
            AND (@search IS NULL OR @search = ''
                 OR u.nombre          LIKE '%' + @search + '%'
                 OR u.apellidoPaterno LIKE '%' + @search + '%'
                 OR u.apellidoMaterno LIKE '%' + @search + '%'
                 OR u.usuario         LIKE '%' + @search + '%')
        ORDER BY u.apellidoPaterno
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH

        -- En error: solo cabecera con status negativo. La API corta al ver status <> 200.
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;

    END CATCH
END
GO
