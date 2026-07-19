namespace comercializadora_api.Services.Exportacion
{
    /// <summary>Parámetros de la regla de umbral. Se enlazan desde la sección "Exportacion".</summary>
    public class ExportacionOptions
    {
        public const string SectionName = "Exportacion";

        /// <summary>Registros hasta los cuales se descarga inmediato; por encima, se envía por correo.</summary>
        public int UmbralDescargaInmediata { get; set; } = 1000;

        /// <summary>
        /// Destinatarios fijos del envío diferido (mismo criterio que <c>correoCCFacturas</c> del
        /// legado, ver <c>Utilerias/Email.cs</c>): el legado NUNCA resolvía el correo del usuario
        /// que solicitaba el reporte (esa columna no existe en <c>Usuarios</c>, ni existió) — todo
        /// se enviaba a una lista de distribución fija configurada por ambiente. El primer correo
        /// de la lista va como destinatario principal (To); el resto como copia oculta (Bcc).
        /// Configurar en <c>appsettings.{Environment}.json</c>, vacío deshabilita el envío diferido
        /// con un error de negocio claro (ver <c>ResolverDestinatarioAsync</c> en cada controller).
        /// </summary>
        public IReadOnlyList<string> CorreoDestino { get; set; } = Array.Empty<string>();
    }
}
