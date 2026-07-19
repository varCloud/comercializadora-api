namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Filtros del diálogo "Ajuste Inventario Físico" (GET /api/inventario-fisico/{id}/ajustes).
    /// Lista completa sin paginar (paridad con el modal legado, que paginaba client-side);
    /// null/0 = TODOS. El id del inventario viaja en la ruta, no aquí.
    /// </summary>
    public class AjustesInventarioQuery
    {
        public int? IdAlmacen { get; set; }
        public int? IdLineaProducto { get; set; }
    }
}
