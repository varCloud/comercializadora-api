namespace comercializadora_api.Services.Exportacion
{
    /// <summary>Parámetros de la regla de umbral. Se enlazan desde la sección "Exportacion".</summary>
    public class ExportacionOptions
    {
        public const string SectionName = "Exportacion";

        /// <summary>Registros hasta los cuales se descarga inmediato; por encima, se envía por correo.</summary>
        public int UmbralDescargaInmediata { get; set; } = 1000;
    }
}
