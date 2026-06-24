namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Ítem genérico de catálogo (Roles, Sucursales, Almacenes) para poblar combos.
    /// Equivale al SelectListItem del legado, pero tipado para el contrato de la API.
    /// </summary>
    public class CatalogoItem
    {
        public int Id { get; set; }
        public string? Descripcion { get; set; }
    }
}
