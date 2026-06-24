/*
===============================================================================
 SP_V2_CONSULTA_CLAVES_SAT
-------------------------------------------------------------------------------
 Búsqueda servidor de claves de Producto/Servicio del SAT
 (FactCatClaveProdServicio, ~52 511 filas). NO se carga el catálogo completo:
 el ng-select del front consulta por término (@search) con un tope (@pageSize).

 Busca por clave o descripción. Devuelve DOS resultsets (cabecera + datos
 id/claveProdServ/descripcion) en la convención uniforme de la API.

 DESCARTE: se filtra por c.activo = 1 (las ~177 claves curadas que el negocio
 usa), igual que el SP legado SP_CONSULTA_CLAVES_PRODUCTOS. Las 52 334 filas con
 activo = 0 son el catálogo SAT crudo y NO se ofrecen en el selector.
 Orden por descripción (como el legado), que es lo que ve el usuario.

 Idempotente: CREATE OR ALTER.
 Autor migración: equipo lluvia-migracion · 2026-06-21
 Origen funcional: SP_CONSULTA_CLAVES_PRODUCTOS (sin paginar) del legado.
===============================================================================
*/
CREATE OR ALTER PROCEDURE [dbo].[SP_V2_CONSULTA_CLAVES_SAT]
    @search     varchar(200) = null,
    @pageNumber int = 1,
    @pageSize   int = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF (@pageNumber IS NULL OR @pageNumber < 1) SET @pageNumber = 1;
    IF (@pageSize   IS NULL OR @pageSize   < 1) SET @pageSize   = 20;
    IF (@pageSize > 100) SET @pageSize = 100;   -- tope de seguridad

    BEGIN TRY
        -- Resultset 1: cabecera
        SELECT 200 AS status, 'OK' AS mensaje;

        -- Resultset 2: datos
        SELECT  c.id,
                c.claveProdServ,
                c.descripcion
        FROM    FactCatClaveProdServicio c
        WHERE   c.activo = 1
            AND (@search IS NULL OR @search = ''
                 OR c.claveProdServ LIKE '%' + @search + '%'
                 OR c.descripcion   LIKE '%' + @search + '%')
        ORDER BY c.descripcion
        OFFSET (@pageNumber - 1) * @pageSize ROWS
        FETCH NEXT @pageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        SELECT CAST(NULL AS int) AS id,
               CAST(NULL AS varchar(50)) AS claveProdServ,
               CAST(NULL AS nvarchar(255)) AS descripcion
        WHERE 1 = 0;
    END CATCH
END
GO
