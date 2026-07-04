using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de un tipo de cliente (POST/PUT). Envuelve
    /// SP_INSERTA_ACTUALIZA_TIPOS_CLIENTES (legado, sin modificar).
    /// </summary>
    public class GuardarTipoClienteRequest
    {
        /// <summary>0 = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdTipoCliente { get; set; }

        /// <summary>Descripción del tipo (columna varchar(50) en BD).</summary>
        [Required]
        [MaxLength(50)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>% de descuento que aplica a los clientes del tipo.</summary>
        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100.")]
        public decimal Descuento { get; set; }

        /// <summary>Estatus activo/inactivo.</summary>
        public bool Activo { get; set; } = true;
    }
}
