/*
===============================================================================
 SP_V2_CONSULTA_COMBINACION_PRODUCCION_PRODUCTOS
-------------------------------------------------------------------------------
 Versión 2 (migración) de SP_OBTENER_COMBINACION_PRODUCTOS_PRODUCCION.
 NO reemplaza al legado (este es el listado paginado del módulo "Relación
 Trapeadores").

 El SP legado devuelve DOS resultsets (cabecera estatus/mensaje + todas las
 filas) sin paginación ni búsqueda. Esta versión sigue la convención uniforme
 de la API nueva:
   - Paginación server-side con OFFSET/FETCH (@pageNumber, @pageSize).
   - Búsqueda libre (@search) por las descripciones de los 3 productos.
   - @id > 0 = obtener una (para precargar el form de edición).
   - Orden dinámico (@order + @sort) con whitelist (id | materia1 | materia2 |
     produccion); default: id DESC (más recientes primero, igual que el legado).
   - Solo combinaciones activas (PP.activo = 1), igual que el legado.
   - Devuelve DOS resultsets (convención Notificacion<T> / RawPage):
       1) cabecera: status, mensaje, total
       2) datos:    la página solicitada con descripciones por JOIN a Productos

 ⚠️ ASUNCIÓN DE ESQUEMA (sin visibilidad del esquema real; riesgo ya aceptado
 y registrado en la HU/tablero de tareas — validar con DBA antes de aplicar):
   Tabla origen: ProduccionProductos (PP). Columnas asumidas:
     id (PK), idProductoMateria1, idProductoMateria2, idProductoProduccion,
     idUnidadMedidad (FK CatUnidadMedida.idUnidadMedida, typo legado
     "Medidad"), unidadMedidad (varchar, clave SAT), valorUnidadMedida,
     activo, fechaAlta.
   JOIN triple a Productos (una vez por cada uno de los 3 roles: Materia1,
   Materia2, Producción) para traer las descripciones.

 Idempotente: CREATE OR ALTER. Compat-120 safe (sin STRING_SPLIT/OPENJSON).
 Autor migración: equipo lluvia-migracion · 2026-07-02
 Origen: SP_OBTENER_COMBINACION_PRODUCTOS_PRODUCCION
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_COMBINACION_PRODUCCION_PRODUCTOS]
    @id          int = 0,       -- > 0 = obtener una; 0 = listar
    @search      varchar(100) = null,
    @order       varchar(50)  = null,
    @sort        varchar(4)   = null,   -- 'asc' | 'desc'
    @pageNumber  int = 1,
    @pageSize    int = 10
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @status  int = 200,
            @mensaje varchar(255) = 'OK',
            @total   int = 0,
            @dir     varchar(4)  = CASE WHEN LOWER(@sort) = 'desc' THEN 'desc' ELSE 'asc' END,
            @col     varchar(50) = LOWER(COALESCE(@order, ''));

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 10;

    BEGIN TRY

        SELECT @total = COUNT(1)
        FROM    ProduccionProductos PP
            JOIN Productos PMateria1   ON PP.idProductoMateria1   = PMateria1.idProducto
            JOIN Productos PMateria2   ON PP.idProductoMateria2   = PMateria2.idProducto
            JOIN Productos PProduccion ON PP.idProductoProduccion = PProduccion.idProducto
        WHERE   PP.activo = 1
            AND PP.id = CASE WHEN @id > 0 THEN @id ELSE PP.id END
            AND (@search IS NULL OR @search = ''
                 OR PMateria1.descripcion   LIKE '%' + @search + '%'
                 OR PMateria2.descripcion   LIKE '%' + @search + '%'
                 OR PProduccion.descripcion LIKE '%' + @search + '%');

        -- Resultset 1: cabecera
        SELECT @status AS status, @mensaje AS mensaje, @total AS total;

        -- Resultset 2: página
        SELECT
                PP.id,
                PP.idProductoMateria1,
                PMateria1.descripcion   AS productoMateria1Descripcion,
                PP.idProductoMateria2,
                PMateria2.descripcion   AS productoMateria2Descripcion,
                PP.idProductoProduccion,
                PProduccion.descripcion AS productoProduccionDescripcion,
                PP.idUnidadMedidad,
                PP.unidadMedidad,
                PP.valorUnidadMedida,
                PP.activo
        FROM    ProduccionProductos PP
            JOIN Productos PMateria1   ON PP.idProductoMateria1   = PMateria1.idProducto
            JOIN Productos PMateria2   ON PP.idProductoMateria2   = PMateria2.idProducto
            JOIN Productos PProduccion ON PP.idProductoProduccion = PProduccion.idProducto
        WHERE   PP.activo = 1
            AND PP.id = CASE WHEN @id > 0 THEN @id ELSE PP.id END
            AND (@search IS NULL OR @search = ''
                 OR PMateria1.descripcion   LIKE '%' + @search + '%'
                 OR PMateria2.descripcion   LIKE '%' + @search + '%'
                 OR PProduccion.descripcion LIKE '%' + @search + '%')
        ORDER BY
            CASE WHEN @col = 'id'         AND @dir = 'asc'  THEN PP.id END ASC,
            CASE WHEN @col = 'id'         AND @dir = 'desc' THEN PP.id END DESC,
            CASE WHEN @col = 'materia1'   AND @dir = 'asc'  THEN PMateria1.descripcion   END ASC,
            CASE WHEN @col = 'materia1'   AND @dir = 'desc' THEN PMateria1.descripcion   END DESC,
            CASE WHEN @col = 'materia2'   AND @dir = 'asc'  THEN PMateria2.descripcion   END ASC,
            CASE WHEN @col = 'materia2'   AND @dir = 'desc' THEN PMateria2.descripcion   END DESC,
            CASE WHEN @col = 'produccion' AND @dir = 'asc'  THEN PProduccion.descripcion END ASC,
            CASE WHEN @col = 'produccion' AND @dir = 'desc' THEN PProduccion.descripcion END DESC,
            PP.id DESC
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;

    END TRY
    BEGIN CATCH
        SELECT -ERROR_STATE() AS status, ERROR_MESSAGE() AS mensaje, 0 AS total;
    END CATCH
END
GO
