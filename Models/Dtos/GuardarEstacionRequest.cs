using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de estación (POST/PUT). Envuelve SP_INSERTA_ACTUALIZA_ESTACIONES.
    /// El <c>idUsuario</c> no viaja en el body: lo toma el controller del JWT (usuario que da
    /// de alta). <c>macAdress</c>/<c>configurado</c> los gestiona la propia estación al
    /// configurarse; desde el panel se conservan/inicializan en su valor por defecto.
    /// </summary>
    public class GuardarEstacionRequest
    {
        /// <summary>0 o null = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdEstacion { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Captura el número de estación.")]
        public int Numero { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un almacén.")]
        public int IdAlmacen { get; set; }

        public string? MacAdress { get; set; }

        public bool Configurado { get; set; }
    }
}
