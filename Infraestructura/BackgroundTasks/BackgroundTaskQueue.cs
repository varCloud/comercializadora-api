using System.Threading.Channels;

namespace comercializadora_api.Infraestructura.BackgroundTasks
{
    /// <summary>
    /// Implementación de <see cref="IBackgroundTaskQueue"/> sobre <see cref="Channel{T}"/>.
    /// Acotada (bounded): si se llena, <see cref="EncolarAsync"/> espera en vez de perder trabajo
    /// o disparar memoria sin control.
    /// </summary>
    public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, ValueTask>> _cola;

        public BackgroundTaskQueue(int capacidad = 50)
        {
            var opciones = new BoundedChannelOptions(capacidad)
            {
                FullMode = BoundedChannelFullMode.Wait,
            };
            _cola = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(opciones);
        }

        public async ValueTask EncolarAsync(Func<CancellationToken, ValueTask> trabajo)
        {
            ArgumentNullException.ThrowIfNull(trabajo);
            await _cola.Writer.WriteAsync(trabajo);
        }

        public async ValueTask<Func<CancellationToken, ValueTask>> DesencolarAsync(CancellationToken cancellationToken)
            => await _cola.Reader.ReadAsync(cancellationToken);
    }
}
