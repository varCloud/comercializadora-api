namespace comercializadora_api.Services.Email
{
    /// <summary>
    /// Envío de correo transversal (hoy: reportes diferidos por exportación; a futuro puede
    /// servir para cualquier otra notificación por correo del sistema).
    /// </summary>
    public interface IEmailService
    {
        Task EnviarAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            EmailAdjunto? adjunto = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Variante con varios adjuntos + copia oculta (Bcc). La necesita el reenvío de
        /// facturas (PDF + XML, igual que <c>Email.EnviarCorreoExternoUsuario</c> del legado,
        /// que además mandaba una lista fija de correos en copia).
        /// </summary>
        Task EnviarAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            IEnumerable<EmailAdjunto> adjuntos,
            IEnumerable<string>? copiaOculta = null,
            CancellationToken cancellationToken = default);
    }
}
