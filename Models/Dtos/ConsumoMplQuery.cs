using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del reporte "Consumo de MPL" (Costo de Producción Agranel):
    /// paginación/búsqueda/orden (heredados de <see cref="PagedQuery"/>; <c>Q</c> es nuevo, el
    /// legado no tenía buscador aquí) + los 4 filtros de la vista legada (año/mes de cálculo,
    /// almacén, línea de producto). Todos opcionales; 0/null = TODOS (misma semántica que el
    /// legado, que los enviaba como <c>null</c> al SP cuando el combo estaba en "--TODOS--").
    /// Equivale a <c>Merma</c> (reusado como filtro por el legado) / <c>FiltroCostoProduccionAgranel</c>.
    /// </summary>
    public class ConsumoMplQuery : PagedQuery
    {
        public int? AnioCalculo { get; set; }
        public int? MesCalculo { get; set; }
        public int? IdAlmacen { get; set; }
        public int? IdLineaProducto { get; set; }
    }
}
