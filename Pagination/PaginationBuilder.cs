using comercializadora_api.Models.Common;

namespace comercializadora_api.Pagination
{
    /// <summary>
    /// Arma la respuesta paginada (data/links/meta) a partir de la página cruda y la petición.
    /// Es el único que conoce la URL pública (App:PublicBaseUrl) y la ruta/query de la request.
    /// </summary>
    public interface IPaginationBuilder
    {
        Notificacion<IEnumerable<T>> Build<T>(RawPage<T> page, PagedQuery query, HttpRequest request);
    }

    /// <inheritdoc />
    public sealed class PaginationBuilder : IPaginationBuilder
    {
        private readonly string _configuredBaseUrl;

        public PaginationBuilder(IConfiguration configuration)
            => _configuredBaseUrl = configuration["App:PublicBaseUrl"]?.TrimEnd('/') ?? string.Empty;

        public Notificacion<IEnumerable<T>> Build<T>(RawPage<T> page, PagedQuery query, HttpRequest request)
        {
            int perPage = query.PerPage < 1 ? 10 : query.PerPage;
            int currentPage = query.Page < 1 ? 1 : query.Page;
            int total = page.Total;
            int lastPage = total <= 0 ? 1 : (int)Math.Ceiling(total / (double)perPage);

            var items = page.Items as IList<T> ?? page.Items.ToList();
            int count = items.Count;
            int? from = total == 0 || count == 0 ? null : ((currentPage - 1) * perPage) + 1;
            int? to = from is null ? null : from + count - 1;

            // Dominio configurado por ambiente (appsettings); si falta, se usa el host de la request.
            string baseUrl = string.IsNullOrWhiteSpace(_configuredBaseUrl)
                ? $"{request.Scheme}://{request.Host}"
                : _configuredBaseUrl;
            string path = $"{baseUrl}{request.Path}";

            string LinkFor(int p) => BuildUrl(path, request.Query, p, perPage);

            var links = new PageLinks
            {
                First = LinkFor(1),
                Last = LinkFor(lastPage),
                Prev = currentPage > 1 ? LinkFor(currentPage - 1) : null,
                Next = currentPage < lastPage ? LinkFor(currentPage + 1) : null
            };

            var meta = new PageMeta
            {
                CurrentPage = currentPage,
                From = from,
                LastPage = lastPage,
                Path = path,
                PerPage = perPage,
                To = to,
                Total = total
            };

            return new Notificacion<IEnumerable<T>>
            {
                Estatus = 200,
                Mensaje = "OK",
                Modelo = items,
                Links = links,
                Meta = meta
            };
        }

        /// <summary>Reconstruye la URL conservando filtros (q/order/sort/…) y fijando page/perPage.</summary>
        private static string BuildUrl(string path, IQueryCollection query, int page, int perPage)
        {
            var parts = new List<string> { $"page={page}", $"perPage={perPage}" };

            foreach (var kv in query)
            {
                var key = kv.Key.ToLowerInvariant();
                if (key is "page" or "perpage") continue;
                foreach (var value in kv.Value)
                {
                    if (string.IsNullOrEmpty(value)) continue;
                    parts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(value)}");
                }
            }

            return $"{path}?{string.Join("&", parts)}";
        }
    }
}
