using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del reporte "Merma": paginación (heredada de
    /// <see cref="PagedQuery"/>, en memoria desde el día uno porque <c>SP_CONSULTA_MERMA</c> no
    /// soporta OFFSET/FETCH y no se modifica) + los 4 filtros de la vista legada (año/mes de
    /// cálculo, almacén, línea de producto). Todos opcionales; 0/null = TODOS (misma semántica
    /// que el legado, que los enviaba como <c>null</c> al SP cuando el combo estaba en
    /// "--TODOS--"; año/mes ausentes hacen que el propio SP calcule con el mes/año actual).
    /// Equivale a <c>Merma</c> del legado (reusado también como filtro de "Costo de Producción",
    /// sub-reporte separado, fuera de alcance).
    /// </summary>
    public class MermaQuery : PagedQuery
    {
        public int? AnioCalculo { get; set; }
        public int? MesCalculo { get; set; }
        public int? IdAlmacen { get; set; }
        public int? IdLineaProducto { get; set; }
    }
}
