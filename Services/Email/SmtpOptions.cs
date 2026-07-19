namespace comercializadora_api.Services.Email
{
    /// <summary>
    /// Parámetros de conexión SMTP. Se enlazan desde la sección "Smtp". <see cref="Usuario"/> y
    /// <see cref="Contrasena"/> deben venir de User Secrets / variables de entorno, nunca
    /// versionados en appsettings.json (mismo criterio que <c>JwtSettings.Key</c>).
    /// </summary>
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;
        public int Puerto { get; set; } = 587;
        public bool UsarSsl { get; set; } = true;
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string RemitenteCorreo { get; set; } = string.Empty;
        public string RemitenteNombre { get; set; } = "Comercializadora Lluvia";
    }
}
