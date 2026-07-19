using comercializadora_api.Infraestructura.BackgroundTasks;
using comercializadora_api.Services.Email;
using Microsoft.Extensions.Options;

namespace comercializadora_api.Services.Exportacion
{
    /// <summary>Implementación de <see cref="IExportacionService"/>. Ver la interfaz para el contrato.</summary>
    public sealed class ExportacionService : IExportacionService
    {
        public const string ContentTypeCsv = "text/csv";

        private readonly ICsvGeneratorService _csv;
        private readonly IBackgroundTaskQueue _cola;
        private readonly IEmailService _email;
        private readonly ExportacionOptions _opciones;
        private readonly ILogger<ExportacionService> _logger;

        public ExportacionService(
            ICsvGeneratorService csv,
            IBackgroundTaskQueue cola,
            IEmailService email,
            IOptions<ExportacionOptions> opciones,
            ILogger<ExportacionService> logger)
        {
            _csv = csv;
            _cola = cola;
            _email = email;
            _opciones = opciones.Value;
            _logger = logger;
        }

        public async Task<ResultadoExportacion> ExportarAsync<T>(
            IReadOnlyCollection<T> datos,
            IReadOnlyList<ColumnaExportable<T>> columnas,
            string nombreReporte,
            DestinatarioExportacion destinatario,
            CancellationToken cancellationToken = default)
        {
            var nombreArchivo = $"{nombreReporte}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            if (datos.Count <= _opciones.UmbralDescargaInmediata)
            {
                var archivo = _csv.Generar(datos, columnas);
                return ResultadoExportacion.Descarga(archivo, nombreArchivo);
            }

            _logger.LogInformation(
                "Exportación diferida: {Reporte} con {Total} registros (umbral {Umbral}) → {Correo}.",
                nombreReporte, datos.Count, _opciones.UmbralDescargaInmediata, destinatario.Correo);

            // Los datos ya están cargados en memoria (el llamador consultó el universo completo
            // antes de invocar este servicio); el trabajo de fondo solo genera el archivo y lo
            // envía, no vuelve a tocar la base de datos ni depende de servicios con scope de request.
            await _cola.EncolarAsync(async ct =>
            {
                var archivo = _csv.Generar(datos, columnas);
                await _email.EnviarAsync(
                    destinatario.Correo,
                    asunto: $"Reporte: {nombreReporte}",
                    cuerpo: $"Hola {destinatario.NombreCompleto}, adjunto el reporte '{nombreReporte}' que solicitaste.",
                    adjuntos: new[] { new EmailAdjunto(nombreArchivo, archivo, ContentTypeCsv) },
                    copiaOculta: destinatario.CopiasOcultas,
                    cancellationToken: ct);
            });

            return ResultadoExportacion.Diferido(destinatario.Correo);
        }
    }
}
