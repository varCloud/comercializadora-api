using comercializadora_api.Models.Dtos;

namespace comercializadora_api.Services.Ubicaciones
{
    /// <summary>
    /// Genera el PDF imprimible de etiquetas QR de ubicaciones (un QR por ubicación, 4 por fila).
    /// Migra Utilerias.Utils.GenerarUbicaciones del legado, pero devuelve el PDF en memoria
    /// (sin escribir archivos temporales en disco).
    /// </summary>
    public interface IUbicacionesPdfService
    {
        /// <summary>Arma el PDF (A4) con un QR por cada ubicación y devuelve los bytes del archivo.</summary>
        byte[] GenerarPdf(IReadOnlyList<UbicacionImprimir> ubicaciones);
    }
}
