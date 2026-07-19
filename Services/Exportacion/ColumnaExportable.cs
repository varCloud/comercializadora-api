namespace comercializadora_api.Services.Exportacion
{
    /// <summary>
    /// Define una columna del Excel exportado: su encabezado y cómo extraer el valor de cada
    /// fila de <typeparamref name="T"/>. El llamador arma la lista una vez por reporte (ver
    /// ejemplo en <c>InventarioFisicoController</c>); no vive en el dominio de negocio.
    /// </summary>
    public sealed class ColumnaExportable<T>
    {
        public string Encabezado { get; }
        public Func<T, object?> Valor { get; }

        public ColumnaExportable(string encabezado, Func<T, object?> valor)
        {
            Encabezado = encabezado;
            Valor = valor;
        }
    }
}
