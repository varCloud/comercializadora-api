/*
===============================================================================
 SP_V2_CONSULTA_INVENTARIO_FISICO
-------------------------------------------------------------------------------
 Versión 2 (migración) del listado SP_CONSULTA_INVENTARIO_FISICO (pantalla
 "Inventario Físico" del legado). NO reemplaza al legado: el sistema viejo
 sigue usando el SP original; la API nueva (GET /api/inventario-fisico) usa
 este V2.

 Novedades respecto al original:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize); el
     legado devolvía TODO el resultset y paginaba con DataTables en el cliente.
   - Orden dinámico opcional (@order + @sort; whitelist fecha|nombre|estatus),
     default fechaAlta DESC (el legado ordenaba por fechaInicio DESC; se elige
     "más reciente primero" por fecha de alta para el panel, igual que
     Producción a granel).
   - Se quitan los filtros @idInventarioFisico/@idEstatus del legado: la
     pantalla migrada no los usa (solo sucursal del JWT, tipo y rango de
     fechas). Ver task_inventario_fisico.md.
   - Expone idTipoInventario + su texto (tipoInventario): General=1,
     Individual=2 (enum fijo del legado EnumTipoInventarioFisico; no hay tabla
     catálogo en BD).
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada, con columnas ordenadas para el
                    multi-mapping InventarioFisico/Sucursal/Status de Dapper
                    (splitOn "idSucursal,idStatus"; el legado hacía lo mismo
                    con Usuario en medio, que la pantalla nueva no muestra).

 Conserva la MISMA lógica de filtrado que el original (predicados
 col = coalesce(@param, col), que además excluyen filas con columna NULL,
 idéntico al legado):
   - @idSucursal / @idTipoInventario: filtro exacto opcional (NULL = sin filtro).
   - @fechaIni / @fechaFin: rango opcional sobre fechaAlta (CAST a date).
   - JOINs (no LEFT) a Usuarios/CatSucursales/CatEstatusInventarioFisico: un
     inventario sin usuario/sucursal/estatus no se lista (idéntico al legado).

 Compatibilidad: BD con compatibility_level = 120 → sin OPENJSON/STRING_SPLIT/
 JSON_VALUE. Idempotente: CREATE OR ALTER.

 Autor migración: equipo lluvia-migracion · 2026-07-04
 Origen: SP_CONSULTA_INVENTARIO_FISICO (Jessica Almonte, 2020-07-28;
 definición leída de BD DB_A57E86_comercializadora, 2026-07-04)
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_INVENTARIO_FISICO]
    @idSucursal        int      = null,
    @idTipoInventario  int      = null,
    @fechaIni          datetime = null,
    @fechaFin          datetime = null,
    @order             varchar(50) = null,
    @sort              varchar(4)  = null,   -- 'asc' | 'desc'
    @pageNumber        int      = 1,
    @pageSize          int      = 10
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

        SELECT
            f.idInventarioFisico,
            f.nombre,
            f.observaciones,
            f.fechaInicio,
            f.FechaFin                                       AS fechaFin,
            f.fechaAlta,
            f.idTipoInventarioFisico                         AS idTipoInventario,
            CASE f.idTipoInventarioFisico
                 WHEN 1 THEN 'General'
                 WHEN 2 THEN 'Individual'
                 ELSE '' END                                 AS tipoInventario,
            s.idSucursal,
            s.descripcion                                    AS descripcionSucursal,
            f.idEstatusInventarioFisico                      AS idStatus,
            est.descripcion                                  AS descripcionEstatus
        INTO #pagina
        FROM        InventarioFisico f
            JOIN    Usuarios u                     ON f.idUsuario = u.idUsuario
            JOIN    CatSucursales s                ON f.idSucursal = s.idSucursal
            JOIN    CatEstatusInventarioFisico est ON f.idEstatusInventarioFisico = est.idEstatusInventarioFisico
        WHERE       f.idSucursal = COALESCE(@idSucursal, f.idSucursal)
                AND f.idTipoInventarioFisico = COALESCE(@idTipoInventario, f.idTipoInventarioFisico)
                AND CAST(f.fechaAlta AS date) >= COALESCE(CAST(@fechaIni AS date), CAST(f.fechaAlta AS date))
                AND CAST(f.fechaAlta AS date) <= COALESCE(CAST(@fechaFin AS date), CAST(f.fechaAlta AS date));

        SELECT @total = COUNT(1) FROM #pagina;

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página (orden de columnas = contrato del multi-mapping)
        SELECT
            idInventarioFisico,
            nombre,
            observaciones,
            fechaInicio,
            fechaFin,
            fechaAlta,
            idTipoInventario,
            tipoInventario,
            idSucursal,
            descripcionSucursal AS descripcion,
            idStatus,
            descripcionEstatus  AS descripcion
        FROM   #pagina
        ORDER BY
            CASE WHEN @col = 'fecha'   AND @dir = 'asc'  THEN fechaAlta          END ASC,
            CASE WHEN @col = 'fecha'   AND @dir = 'desc' THEN fechaAlta          END DESC,
            CASE WHEN @col = 'nombre'  AND @dir = 'asc'  THEN nombre             END ASC,
            CASE WHEN @col = 'nombre'  AND @dir = 'desc' THEN nombre             END DESC,
            CASE WHEN @col = 'estatus' AND @dir = 'asc'  THEN descripcionEstatus END ASC,
            CASE WHEN @col = 'estatus' AND @dir = 'desc' THEN descripcionEstatus END DESC,
            fechaAlta DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
