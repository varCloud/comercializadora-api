namespace comercializadora_api.Services.Exportacion
{
    /// <summary>Genera un archivo CSV en memoria a partir de datos + columnas.</summary>
    public interface ICsvGeneratorService
    {
        /// <summary>Devuelve el contenido del .csv (UTF-8, en memoria, sin archivos temporales en disco).</summary>
        byte[] Generar<T>(IEnumerable<T> datos, IReadOnlyList<ColumnaExportable<T>> columnas);
    }
}
