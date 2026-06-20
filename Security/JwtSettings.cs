namespace comercializadora_api.Security
{
    /// <summary>
    /// Parámetros de configuración del JWT. Se enlazan desde la sección "Jwt".
    /// La <see cref="Key"/> debe venir de User Secrets / variables de entorno, nunca
    /// versionada en appsettings.json.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiraMinutos { get; set; } = 480; // 8 horas por defecto
        public string Key { get; set; } = string.Empty;
    }
}
