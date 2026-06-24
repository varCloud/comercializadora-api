namespace comercializadora_api.Models.Common
{
    /// <summary>
    /// URLs de navegación de la paginación. <c>prev</c>/<c>next</c> son null en los extremos.
    /// Viajan dentro de <see cref="Notificacion{T}.Links"/> en los listados paginados.
    /// </summary>
    public class PageLinks
    {
        public string? First { get; set; }
        public string? Last { get; set; }
        public string? Prev { get; set; }
        public string? Next { get; set; }
    }

    /// <summary>
    /// Metadatos de la paginación (página actual, rango, total…). Viajan dentro de
    /// <see cref="Notificacion{T}.Meta"/> en los listados paginados.
    /// </summary>
    public class PageMeta
    {
        public int CurrentPage { get; set; }
        public int? From { get; set; }
        public int LastPage { get; set; }
        public string? Path { get; set; }
        public int PerPage { get; set; }
        public int? To { get; set; }
        public int Total { get; set; }
    }
}
