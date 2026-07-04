using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de una relación de trapeadores (POST/PUT). Envuelve el SP legado
    /// SP_AGREGA_ACTUALIZA_COMBINACION_PRODUCCION_PRODUCTOS, que deriva la descripción de la
    /// unidad de medida (unidadMedidad) a partir de <see cref="IdUnidadMedidad"/>.
    /// <para>
    /// <see cref="ValorUnidadMedida"/> llega ya convertido (capturado y dividido /1000 en el
    /// front, igual que el legado y que Relación Líquidos); el SP lo almacena tal cual.
    /// </para>
    /// </summary>
    public class GuardarRelacionTrapeadorRequest
    {
        /// <summary>0 = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona la materia prima / Matra.")]
        public int IdProductoMateria1 { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona el bastón.")]
        public int IdProductoMateria2 { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona el trapeador producido.")]
        public int IdProductoProduccion { get; set; }

        /// <summary>Id de la unidad de medida (FK CatUnidadMedida). Typo "Medidad" del esquema legado.</summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona la unidad de medida.")]
        public int IdUnidadMedidad { get; set; }

        /// <summary>Cantidad, ya convertida (valor / 1000) lista para almacenar.</summary>
        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Captura una cantidad válida.")]
        public decimal ValorUnidadMedida { get; set; }
    }
}
