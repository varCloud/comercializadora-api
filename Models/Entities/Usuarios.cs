using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace comercializadora_api.Models.Entities
{
    public class Usuarios
    {
        [Key] 
        public int idUsuario { get; set; }
        [NotMapped]
        public int id { get; set; }
        public Boolean activo { get; set; }
        public string usuario { get; set; }
        public string contrasena { get; set; }
        public int idRol { get; set; }            
        public string nombre { get; set; }
        public string apellidoPaterno { get; set; }          
        public string apellidoMaterno { get; set; }
        public string telefono { get; set; }
        public int idAlmacen { get; set; }
        public int idSucursal { get; set; }
        [Column("fecha_alta")]
        public DateTime fechaAlta { get; set; } 

        [NotMapped]
        public Boolean usuarioValido { get; set; }
        [NotMapped]
        public string nombreCompleto { get; set; }
        [NotMapped]
        public DateTime fechaIni { get; set; }
        [NotMapped]
        public DateTime fechaFin { get; set; }

    }
}
