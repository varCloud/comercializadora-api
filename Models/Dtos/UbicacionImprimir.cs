using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Una ubicación de almacén a imprimir como etiqueta QR. Migra el modelo legado
    /// <c>Ubicacion</c> (solo los campos que necesita la generación del PDF). El front arma una
    /// lista de estas y la API devuelve un PDF con un QR por cada una (4 por fila).
    /// </summary>
    public sealed class UbicacionImprimir
    {
        [Required]
        public int IdAlmacen { get; set; }

        public string? DescripcionAlmacen { get; set; }

        [Required]
        public int IdPiso { get; set; }

        public string? DescripcionPiso { get; set; }

        [Required]
        public int IdPasillo { get; set; }

        public string? DescripcionPasillo { get; set; }

        [Required]
        public int IdRaq { get; set; }

        public string? DescripcionRaq { get; set; }
    }
}
