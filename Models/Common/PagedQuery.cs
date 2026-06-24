namespace comercializadora_api.Models.Common
{
    /// <summary>
    /// Parámetros de consulta de un listado paginado. Se enlaza con <c>[FromQuery]</c> como un
    /// solo objeto (en vez de muchos parámetros sueltos en la firma del action). Los listados
    /// que necesiten filtros extra heredan de esta clase (p. ej. UsuariosQuery).
    /// </summary>
    public class PagedQuery
    {
        /// <summary>Página solicitada (1-based).</summary>
        public int Page { get; set; } = 1;

        /// <summary>Tamaño de página.</summary>
        public int PerPage { get; set; } = 10;

        /// <summary>Búsqueda libre (texto).</summary>
        public string? Q { get; set; }

        /// <summary>Columna por la que ordenar (whitelist en cada SP).</summary>
        public string? Order { get; set; }

        /// <summary>Dirección de orden: "asc" | "desc".</summary>
        public string? Sort { get; set; }
    }
}
