using comercializadora_api.Models.Common;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Parámetros de consulta del listado de Usuarios: paginación/búsqueda/orden (heredados de
    /// <see cref="PagedQuery"/>) + filtros propios por rol y almacén. Se enlaza con [FromQuery].
    /// </summary>
    public class UsuariosQuery : PagedQuery
    {
        public int IdRol { get; set; }
        public int IdAlmacen { get; set; }
    }
}
