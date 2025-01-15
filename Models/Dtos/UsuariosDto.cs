using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    public class UsuariosDto
    {
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
    }
}
