using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta (POST, id = 0) y renombrado (PUT /{id}) de un inventario físico.
    /// Envuelve SP_INSERTA_ACTUALIZA_INVENTARIO_FISICO, que valida unicidad del nombre y, en
    /// alta, toma la sucursal del usuario y crea el inventario en estatus 1 (Pendiente).
    /// El idUsuario se toma del claim JWT, no del body.
    /// </summary>
    public class GuardarInventarioFisicoRequest
    {
        /// <summary>Nombre del inventario (único; columna varchar(300) en BD).</summary>
        [Required]
        [MaxLength(300)]
        public string Nombre { get; set; } = string.Empty;
    }
}
