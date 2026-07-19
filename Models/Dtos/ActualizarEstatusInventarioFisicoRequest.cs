using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload del PATCH /api/inventario-fisico/{id}/estatus. Envuelve
    /// SP_ACTUALIZA_ESTATUS_INVENTARIO_FISICO, que valida las transiciones (2 = iniciar solo
    /// desde 1 y sin otro iniciado en la sucursal; 3 = finalizar y afectar inventario solo
    /// desde 2 y con ajustes; 4 = cancelar solo desde 2) y devuelve el error de negocio en
    /// la Notificacion. El idUsuario se toma del claim JWT, no del body.
    /// </summary>
    public class ActualizarEstatusInventarioFisicoRequest
    {
        /// <summary>Estatus destino: 2 Iniciado, 3 Finalizado, 4 Cancelado.</summary>
        [Required]
        [Range(2, 4)]
        public int IdEstatus { get; set; }

        /// <summary>Observaciones capturadas en el diálogo (columna varchar(500) en BD).</summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}
