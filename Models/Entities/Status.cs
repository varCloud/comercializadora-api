namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Estatus de un nivel de inventario (1 = dentro de límites, 2 = sobre el máximo,
    /// 3 = bajo el mínimo). Equivale al modelo Status del legado; se mapea anidado en
    /// <see cref="LimiteInventario.EstatusInventario"/> por multi-mapping de Dapper.
    /// </summary>
    public class Status
    {
        public int IdStatus { get; set; }
        public string? Descripcion { get; set; }
    }
}
