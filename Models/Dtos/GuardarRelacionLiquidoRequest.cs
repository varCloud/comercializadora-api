using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de una relación de líquidos (POST/PUT). Envuelve el SP legado
    /// SP_AGREGA_ACTUALIZA_COMBINACION_PRODUCTOS_ENSAVDOS_A_AGRANEL, que deriva la descripción de
    /// la unidad de medida (unidadMedidad) a partir de <see cref="IdUnidadMedidad"/>.
    /// <para>
    /// <see cref="ValorUnidadMedida"/> llega ya convertido (capturado en ml/g y dividido /1000 en
    /// el front, igual que el legado); el SP lo almacena tal cual.
    /// </para>
    /// </summary>
    public class GuardarRelacionLiquidoRequest
    {
        /// <summary>0 = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdRelacionEnvasadoAgranel { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona la materia prima / granel.")]
        public int IdProductoAgranel { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona el producto a envasar.")]
        public int IdProductoEnvasado { get; set; }

        /// <summary>Id del producto envase (se conserva el typo "Produco" del esquema legado).</summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona el envase.")]
        public int IdProducoEnvase { get; set; }

        /// <summary>Id de la unidad de medida (FK CatUnidadMedida). Typo "Medidad" del esquema legado.</summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona la unidad de medida.")]
        public int IdUnidadMedidad { get; set; }

        /// <summary>Cantidad por envase, ya convertida (valor / 1000) lista para almacenar.</summary>
        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Captura una cantidad válida.")]
        public decimal ValorUnidadMedida { get; set; }
    }
}
