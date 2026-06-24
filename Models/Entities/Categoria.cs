using System.Text.Json.Serialization;

namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Resultset común de varios SP del dashboard (ventas por fecha, top ten, información
    /// global, IVA acumulado). Migra el modelo Categoria del legado.
    /// </summary>
    /// <remarks>
    /// La columna del SP se llama <c>categoria</c>, pero una propiedad no puede llamarse igual
    /// que su tipo (CS0542), así que se usa <see cref="NombreCategoria"/> y se conserva el
    /// contrato JSON <c>categoria</c> con <see cref="JsonPropertyNameAttribute"/> (mismo patrón
    /// que <c>Usuario.NombreUsuario</c>). Como no se modifican los SP, el repositorio proyecta
    /// la columna a mano (ver <c>DashboardRepository</c>); el resto de columnas
    /// (id/total/totalPE/fechaIni/fechaFin) mapean directo en Dapper (case-insensitive, sin
    /// guiones bajos).
    /// </remarks>
    public class Categoria
    {
        public int Id { get; set; }

        [JsonPropertyName("categoria")]
        public string? NombreCategoria { get; set; }

        public float Total { get; set; }
        public float TotalPE { get; set; }
        public DateTime FechaIni { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
