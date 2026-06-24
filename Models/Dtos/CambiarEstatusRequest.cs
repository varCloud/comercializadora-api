namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload para activar/desactivar un usuario (PATCH). El "eliminar" del legado es un
    /// borrado lógico: activo = false. Envuelve SP_ACTUALIZA_STATUS_USUARIO.
    /// </summary>
    public class CambiarEstatusRequest
    {
        public bool Activo { get; set; }
    }
}
