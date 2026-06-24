namespace comercializadora_api.Models.Common
{
    /// <summary>
    /// Página "cruda" que devuelven repositorios/servicios: solo las filas + el total. El
    /// controller la envuelve en <see cref="PagedResult{T}"/> (data/links/meta) con
    /// <c>IPaginationBuilder</c>, que es quien conoce la URL de la petición.
    /// </summary>
    public sealed class RawPage<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int Total { get; set; }

        public static RawPage<T> Empty() => new();
    }
}
