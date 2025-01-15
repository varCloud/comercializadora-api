using System.Numerics;

namespace comercializadora_api.Models.Models
{
    public class Paginador<T> where T : class
    {
        public int paginaActual { get; set; }
        public int totalPaginas { get; set; }
        public int paginaSiguiente { get; set; }
        public int itemsPorPagina { get; set; }
        public int ultimaPagina { get; set; }
        public int totalItems { get; set; }
        public IEnumerable<T> items { get; set; }


    }
}
