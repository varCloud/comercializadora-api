namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Sucursal (CatSucursales). Equivale al modelo Sucursal del legado (solo los campos que
    /// consumen las pantallas migradas); se mapea anidada por multi-mapping de Dapper
    /// (p. ej. <see cref="InventarioFisico.Sucursal"/>).
    /// </summary>
    public class Sucursal
    {
        public int IdSucursal { get; set; }
        public string? Descripcion { get; set; }
    }
}
