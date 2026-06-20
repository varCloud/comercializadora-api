namespace comercializadora_api.Models.Common
{
    /// <summary>
    /// Envoltorio estándar de respuesta de los stored procedures.
    /// Equivale al patrón Notificacion del backend legado (status / mensaje / modelo).
    /// </summary>
    public class Notificacion<T>
    {
        /// <summary>Código de estado del SP (200 = éxito).</summary>
        public int Estatus { get; set; }

        /// <summary>Mensaje descriptivo devuelto por el SP.</summary>
        public string? Mensaje { get; set; }

        /// <summary>Datos devueltos por el SP cuando la operación fue exitosa.</summary>
        public T? Modelo { get; set; }

        /// <summary>True cuando <see cref="Estatus"/> es 200.</summary>
        public bool EsExitoso => Estatus == 200;
    }
}
