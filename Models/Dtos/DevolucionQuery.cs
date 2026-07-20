using System.ComponentModel.DataAnnotations;
using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Filtros del reporte "Devoluciones y Complementos" (<c>ReportesController.Devoluciones</c> /
    /// <c>ReportesDAO.ObtenerDevolucionesyComplementos</c> del legado). Hereda <see cref="PagedQuery"/>
    /// porque <c>SP_CONSULTA_DEVOLUCIONES_Y_COMPLEMENTOS</c> no pagina a nivel SQL (no soporta
    /// OFFSET/FETCH) y no se modifica (se reutiliza tal cual, sin <c>SP_V2_*</c>) — mismo patrón de
    /// paginación en memoria que <see cref="MermaQuery"/>/<see cref="VentaReporteQuery"/>. Se enlaza
    /// con <c>[FromQuery]</c>.
    /// </summary>
    public class DevolucionQuery : PagedQuery, IValidatableObject
    {
        public DevolucionQuery()
        {
            PerPage = 20;
        }

        /// <summary>Fecha inicio del rango (obligatoria).</summary>
        [Required(ErrorMessage = "fechaIni es requerida.")]
        public DateTime? FechaIni { get; set; }

        /// <summary>Fecha fin del rango (obligatoria).</summary>
        [Required(ErrorMessage = "fechaFin es requerida.")]
        public DateTime? FechaFin { get; set; }

        /// <summary>0/null = todos los almacenes.</summary>
        public int? IdAlmacen { get; set; }

        /// <summary>0/null = todos los usuarios.</summary>
        public int? IdUsuario { get; set; }

        /// <summary>
        /// Mapea a <c>@idTipoConsulta</c> del SP: <c>1</c> = Devoluciones, <c>2</c> = Complementos.
        /// A diferencia de <see cref="IdAlmacen"/>/<see cref="IdUsuario"/>, el SP NO admite "todos"
        /// para este filtro (las dos mitades del <c>UNION</c> están condicionadas a
        /// <c>@idTipoConsulta = 1</c> / <c>= 2</c> respectivamente; pasar <c>NULL</c> u otro valor
        /// no trae ninguna fila). 0/null en la request se resuelve a <c>1</c> (Devoluciones) en
        /// <see cref="Repositories.ReportesDevolucion.DevolucionReporteRepository"/>, igual que el
        /// default propio del SP (<c>@idTipoConsulta int = 1</c>).
        /// </summary>
        public int? TipoTicket { get; set; }

        /// <summary>Valida que el rango de fechas sea coherente (fechaIni ≤ fechaFin).</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaIni is not null && FechaFin is not null && FechaIni > FechaFin)
            {
                yield return new ValidationResult(
                    "fechaIni no puede ser mayor que fechaFin.",
                    new[] { nameof(FechaIni), nameof(FechaFin) });
            }
        }
    }
}
