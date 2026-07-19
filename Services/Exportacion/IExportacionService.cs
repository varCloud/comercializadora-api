namespace comercializadora_api.Services.Exportacion
{
    /// <summary>
    /// Servicio transversal de exportación a Excel. Encapsula la regla de negocio: si el volumen
    /// de <paramref name="datos"/> es manejable, genera el archivo y lo entrega para descarga
    /// inmediata; si no, lo procesa en segundo plano y lo envía por correo. Cualquier reporte del
    /// sistema (inventario, consumo MPL, producción, compras, …) consume este mismo contrato en
    /// vez de reimplementar la decisión descarga-vs-correo.
    /// </summary>
    public interface IExportacionService
    {
        Task<ResultadoExportacion> ExportarAsync<T>(
            IReadOnlyCollection<T> datos,
            IReadOnlyList<ColumnaExportable<T>> columnas,
            string nombreReporte,
            DestinatarioExportacion destinatario,
            CancellationToken cancellationToken = default);
    }
}
