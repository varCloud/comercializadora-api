namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Catálogo de estatus de compra (CatStatusCompra). Mapea el segundo resultset de
    /// SP_CONSULTA_ESTATUS_COMPRA. 1 Pendiente · 2 Realizada · 3 Finalizada · 4 Cancelada ·
    /// 5 Devolución.
    /// </summary>
    public class EstatusCompra
    {
        public int IdStatus { get; set; }
        public string? Descripcion { get; set; }
    }
}
