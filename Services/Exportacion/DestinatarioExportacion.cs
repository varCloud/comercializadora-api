namespace comercializadora_api.Services.Exportacion
{
    /// <summary>
    /// Quién solicita la exportación y a dónde enviarla si se difiere por correo. El controller
    /// la resuelve (JWT + <c>IUsuariosService</c> para <see cref="NombreCompleto"/>; el
    /// <see cref="Correo"/>/<see cref="CopiasOcultas"/> salen de <c>Usuario.Correo</c> en algunos
    /// reportes o de <c>ExportacionOptions.CorreoDestino</c> en otros, según el criterio de cada
    /// controller); este servicio no conoce sesión ni claims.
    /// </summary>
    public sealed record DestinatarioExportacion(
        int IdUsuario, string NombreCompleto, string Correo, IReadOnlyList<string>? CopiasOcultas = null);
}
