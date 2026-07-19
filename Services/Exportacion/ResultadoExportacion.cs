namespace comercializadora_api.Services.Exportacion
{
    /// <summary>
    /// Resultado de <see cref="IExportacionService.ExportarAsync{T}"/>: o trae el archivo listo
    /// para descargar, o trae el mensaje de "te llega por correo". El controller decide la
    /// respuesta HTTP según <see cref="EsDescargaInmediata"/>.
    /// </summary>
    public sealed class ResultadoExportacion
    {
        public bool EsDescargaInmediata { get; private init; }
        public byte[]? Archivo { get; private init; }
        public string? NombreArchivo { get; private init; }
        public string Mensaje { get; private init; } = string.Empty;

        public static ResultadoExportacion Descarga(byte[] archivo, string nombreArchivo) => new()
        {
            EsDescargaInmediata = true,
            Archivo = archivo,
            NombreArchivo = nombreArchivo,
            Mensaje = "Archivo generado correctamente.",
        };

        public static ResultadoExportacion Diferido(string correo) => new()
        {
            EsDescargaInmediata = false,
            Mensaje = $"El reporte supera el límite de descarga inmediata; se enviará por correo a {correo} en unos minutos.",
        };
    }
}
