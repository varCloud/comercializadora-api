namespace comercializadora_api.Services.Email
{
    /// <summary>Archivo adjunto a enviar por correo (en memoria, sin temporales en disco).</summary>
    public sealed record EmailAdjunto(string NombreArchivo, byte[] Contenido, string TipoContenido);
}
