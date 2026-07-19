namespace comercializadora_api.Infraestructura.BackgroundTasks
{
    /// <summary>
    /// Cola en memoria (Channel) para trabajo que no debe bloquear el hilo de la request HTTP.
    /// No persiste entre reinicios del proceso: si eso llega a ser un problema (picos de carga,
    /// necesidad de reintentos/monitoreo), la alternativa es un job runner persistido (Hangfire).
    /// </summary>
    public interface IBackgroundTaskQueue
    {
        /// <summary>Encola una unidad de trabajo para ejecutarse fuera del hilo de la request.</summary>
        ValueTask EncolarAsync(Func<CancellationToken, ValueTask> trabajo);

        /// <summary>Usado únicamente por <see cref="ExportacionQueuedHostedService"/> para consumir la cola.</summary>
        ValueTask<Func<CancellationToken, ValueTask>> DesencolarAsync(CancellationToken cancellationToken);
    }
}
