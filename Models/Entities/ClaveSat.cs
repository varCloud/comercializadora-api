namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Clave de Producto/Servicio del SAT (FactCatClaveProdServicio). El catálogo tiene
    /// ~52 511 filas, por eso se consume con búsqueda servidor (no se carga completo).
    /// El producto guarda la cadena <see cref="ClaveProdServ"/> (no el id numérico).
    /// </summary>
    public class ClaveSat
    {
        public int Id { get; set; }
        public string? ClaveProdServ { get; set; }
        public string? Descripcion { get; set; }
    }
}
