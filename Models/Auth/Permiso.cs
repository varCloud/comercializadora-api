namespace comercializadora_api.Models.Auth
{
    /// <summary>
    /// Permiso/módulo al que tiene acceso el rol del usuario.
    /// Mapea el resultset de permisos de SP_VALIDA_CONTRASENA (PermisosRolPorModulo + módulo).
    /// </summary>
    public class Permiso
    {
        public int IdPermiso { get; set; }
        public int IdModulo { get; set; }
        public string? Modulo { get; set; }
        public string? Descripcion { get; set; }
        public bool TienePermiso { get; set; }
    }
}
