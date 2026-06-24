using comercializadora_api.Models.Dtos;

namespace comercializadora_api.Services.CodigosBarras
{
    /// <summary>
    /// Generador del PDF de etiquetas de código de barras (CODE_128) de productos.
    /// Migra Utilerias.Utils.GenerarCodigosBarras del legado (iTextSharp → QuestPDF, ZXing).
    /// </summary>
    public interface ICodigosBarrasPdfService
    {
        /// <summary>Devuelve el PDF (en memoria) con una etiqueta por producto, 2 por fila.</summary>
        byte[] GenerarPdf(IReadOnlyList<ProductoCodigoBarra> productos);
    }
}
