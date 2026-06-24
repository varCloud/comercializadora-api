using System.Text.Json.Serialization;

namespace comercializadora_api.Models.Common
{
    /// <summary>
    /// Envoltorio estándar de respuesta de los stored procedures.
    /// Equivale al patrón Notificacion del backend legado (status / mensaje / modelo).
    /// En listados paginados agrega <see cref="Links"/> y <see cref="Meta"/> junto a
    /// estatus/mensaje (los datos van en <see cref="Modelo"/>).
    /// </summary>
    public class Notificacion<T>
    {
        /// <summary>Código de estado del SP (200 = éxito).</summary>
        public int Estatus { get; set; }

        /// <summary>Mensaje descriptivo devuelto por el SP.</summary>
        public string? Mensaje { get; set; }

        /// <summary>Datos devueltos por el SP cuando la operación fue exitosa.</summary>
        public T? Modelo { get; set; }

        /// <summary>Links de paginación (first/last/prev/next). Solo en listados paginados.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PageLinks? Links { get; set; }

        /// <summary>Metadatos de paginación (currentPage, total, …). Solo en listados paginados.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PageMeta? Meta { get; set; }

        /// <summary>True cuando <see cref="Estatus"/> es 200.</summary>
        public bool EsExitoso => Estatus == 200;
    }
}
