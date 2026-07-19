namespace comercializadora_api.Services.Exportacion
{
    /// <summary>
    /// Quién solicita la exportación y a dónde enviarla si se difiere por correo. El controller
    /// la resuelve (JWT + <c>IUsuariosService</c>); este servicio no conoce sesión ni claims.
    /// </summary>
    public sealed record DestinatarioExportacion(int IdUsuario, string NombreCompleto, string Correo);
}
