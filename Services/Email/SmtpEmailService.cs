using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace comercializadora_api.Services.Email
{
    /// <summary>
    /// Implementación de <see cref="IEmailService"/> vía SMTP (MailKit). Reemplaza el envoltorio
    /// estático <c>Email.EnviarCorreoConAdjunto</c> del legado por un servicio inyectable.
    /// </summary>
    public sealed class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _opciones;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> opciones, ILogger<SmtpEmailService> logger)
        {
            _opciones = opciones.Value;
            _logger = logger;
        }

        public Task EnviarAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            EmailAdjunto? adjunto = null,
            CancellationToken cancellationToken = default)
            => EnviarAsync(destinatario, asunto, cuerpo,
                adjunto is null ? Enumerable.Empty<EmailAdjunto>() : new[] { adjunto },
                copiaOculta: null, cancellationToken);

        public async Task EnviarAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            IEnumerable<EmailAdjunto> adjuntos,
            IEnumerable<string>? copiaOculta = null,
            CancellationToken cancellationToken = default)
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(_opciones.RemitenteNombre, _opciones.RemitenteCorreo));
            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            if (copiaOculta is not null)
                foreach (var copia in copiaOculta.Where(c => !string.IsNullOrWhiteSpace(c)))
                    mensaje.Bcc.Add(MailboxAddress.Parse(copia));
            mensaje.Subject = asunto;

            var cuerpoBuilder = new BodyBuilder { TextBody = cuerpo };
            foreach (var adjunto in adjuntos)
                cuerpoBuilder.Attachments.Add(adjunto.NombreArchivo, adjunto.Contenido,
                    ContentType.Parse(adjunto.TipoContenido));
            mensaje.Body = cuerpoBuilder.ToMessageBody();

            using var cliente = new SmtpClient();
            await cliente.ConnectAsync(
                _opciones.Host, _opciones.Puerto,
                _opciones.UsarSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                cancellationToken);
            await cliente.AuthenticateAsync(_opciones.Usuario, _opciones.Contrasena, cancellationToken);
            await cliente.SendAsync(mensaje, cancellationToken);
            await cliente.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Correo enviado a {Destinatario}: {Asunto}.", destinatario, asunto);
        }
    }
}
