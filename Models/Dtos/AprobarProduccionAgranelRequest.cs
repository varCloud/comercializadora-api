using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de la aprobación de productos en producción a granel
    /// (PATCH /api/produccion-agranel/aprobar). Envuelve
    /// <c>SP_APP_APROBAR_PRODUCTOS_PRODCUCCION_AGRANEL</c> (typo "PRODCUCCION" es el nombre real
    /// en BD): el repositorio serializa <see cref="Productos"/> al XML
    /// &lt;ArrayOfProductosProduccionAgranel&gt; que espera el SP. El estatus final (3 procesado /
    /// 4 rechazo total / 5 rechazo parcial) lo calcula el SP a partir de la cantidad atendida.
    /// Migra <c>RequestAprobarProductosProduccionAgranel</c> del legado; el idUsuario NO viaja en
    /// el body: la API lo toma del claim del JWT.
    /// </summary>
    public class AprobarProduccionAgranelRequest
    {
        [Required]
        public int IdAlmacen { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto a aprobar.")]
        public List<AprobarProduccionAgranelItem> Productos { get; set; } = new();
    }

    /// <summary>Un renglón del proceso a aprobar/rechazar en el payload de aprobación.</summary>
    public class AprobarProduccionAgranelItem
    {
        public long IdProcesoProduccionAgranel { get; set; }
        public int IdProducto { get; set; }
        public int IdUbicacion { get; set; }

        /// <summary>0 = rechazo total; = solicitada → procesado; &lt; solicitada → rechazo parcial.</summary>
        [Range(0, double.MaxValue, ErrorMessage = "La cantidad atendida no puede ser negativa.")]
        public double CantidadAtendida { get; set; }

        public string? Observaciones { get; set; }
    }
}
