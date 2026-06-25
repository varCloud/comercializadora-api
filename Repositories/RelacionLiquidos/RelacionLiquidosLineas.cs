namespace comercializadora_api.Repositories.RelacionLiquidos
{
    /// <summary>
    /// IDs de línea de producto que definen los catálogos de cada selector del módulo
    /// "Relación Liquidos". Son magic numbers heredados del SP legado
    /// SP_OBTENER_PRODUCTOS_ENVASES_LIQUIDOS_AGRANEL (granel 12,20 / envasado 27 / envases 19);
    /// se centralizan aquí (constantes del back) para que el front no los conozca: pide por
    /// <c>tipo</c> semántico (granel | envasar | envase) y el back resuelve las líneas.
    /// Si el catálogo de líneas cambia en BD, se ajusta SOLO aquí.
    /// </summary>
    public static class RelacionLiquidosLineas
    {
        public const string Granel  = "granel";
        public const string Envasar = "envasar";
        public const string Envase  = "envase";

        /// <summary>Líneas de Materia Prima / Granel / Líquidos.</summary>
        public const string LineasGranel  = "12,20";

        /// <summary>Línea de Producto a Envasar.</summary>
        public const string LineasEnvasar = "27";

        /// <summary>Línea de Envases.</summary>
        public const string LineasEnvase  = "19";

        /// <summary>
        /// Devuelve el CSV de líneas para un <paramref name="tipo"/> semántico, o null si el tipo
        /// no es reconocido (el llamador trata null como "tipo inválido").
        /// </summary>
        public static string? CsvPorTipo(string? tipo) => (tipo ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            Granel  => LineasGranel,
            Envasar => LineasEnvasar,
            Envase  => LineasEnvase,
            _       => null
        };
    }
}
