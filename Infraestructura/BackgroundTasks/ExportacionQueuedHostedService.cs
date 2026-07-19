namespace comercializadora_api.Infraestructura.BackgroundTasks
{
    /// <summary>
    /// Worker en proceso que consume <see cref="IBackgroundTaskQueue"/> y ejecuta cada trabajo
    /// (hoy: generar el Excel diferido + enviarlo por correo). Un error en un trabajo se loguea
    /// y no detiene la cola (el siguiente trabajo se sigue procesando).
    /// </summary>
    public sealed class ExportacionQueuedHostedService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _cola;
        private readonly ILogger<ExportacionQueuedHostedService> _logger;

        public ExportacionQueuedHostedService(IBackgroundTaskQueue cola, ILogger<ExportacionQueuedHostedService> logger)
        {
            _cola = cola;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var trabajo = await _cola.DesencolarAsync(stoppingToken);
                try
                {
                    await trabajo(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error ejecutando una tarea de fondo de exportación.");
                }
            }
        }
    }
}
