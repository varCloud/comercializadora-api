using System.Drawing;
using System.Globalization;
using comercializadora_api.Models.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

// El servicio usa System.Drawing (binding de ZXing) y la API se despliega en Windows/IIS
// (mismo supuesto que UbicacionesPdfService). Se silencia CA1416 (solo soportado en Windows).
#pragma warning disable CA1416

namespace comercializadora_api.Services.CodigosBarras
{
    /// <summary>
    /// Implementación del generador de PDF de etiquetas de código de barras.
    /// Barra CODE_128 con ZXing.Net (equivale a Utils.GenerarCodigoBarras) y maquetado A4 con
    /// QuestPDF (2 etiquetas por fila, igual que el legado). El PDF se devuelve en memoria,
    /// sin archivos temporales en disco (mejora sobre el legado, que escribía/borraba JPEGs + PDF).
    /// </summary>
    public sealed class CodigosBarrasPdfService : ICodigosBarrasPdfService
    {
        private const int BarrasAncho = 280;     // px de la imagen de barras (CODE_128)
        private const int BarrasAlto = 90;
        private const int ColumnasPorFila = 2;   // 2 etiquetas por fila, como el legado
        private static readonly CultureInfo Mx = new("es-MX");

        public byte[] GenerarPdf(IReadOnlyList<ProductoCodigoBarra> productos)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Black));

                    page.Header().PaddingBottom(8).Text("Códigos de Barras").FontSize(14).SemiBold();

                    page.Content().Column(col =>
                    {
                        foreach (var fila in productos.Chunk(ColumnasPorFila))
                        {
                            col.Item().PaddingVertical(6).Row(row =>
                            {
                                foreach (var prod in fila)
                                {
                                    row.RelativeItem().Border(1).Padding(8).Column(celda =>
                                    {
                                        var nombre = prod.Descripcion ?? string.Empty;
                                        celda.Item().AlignCenter().Text(nombre)
                                            .FontSize(nombre.Length > 35 ? 8 : 10).SemiBold();

                                        byte[]? barras = GenerarBarras(prod.CodigoBarras);
                                        if (barras is not null)
                                            celda.Item().PaddingTop(4).AlignCenter().Width(190).Image(barras);
                                        else
                                            celda.Item().PaddingTop(4).AlignCenter()
                                                .Text("(sin código de barras)").Italic().FontColor(Colors.Grey.Medium);

                                        celda.Item().PaddingTop(4).Row(precios =>
                                        {
                                            precios.RelativeItem().AlignCenter()
                                                .Text($"Menudeo: {FormatoMoneda(prod.PrecioIndividual)}").FontSize(11);
                                            precios.RelativeItem().AlignCenter()
                                                .Text($"Mayoreo: {FormatoMoneda(prod.PrecioMenudeo)}").FontSize(11);
                                        });
                                    });
                                }

                                // Rellena la columna faltante para mantener la rejilla de 2 alineada.
                                for (int k = fila.Length; k < ColumnasPorFila; k++)
                                    row.RelativeItem();
                            });
                        }
                    });
                });
            }).GeneratePdf();
        }

        private static string FormatoMoneda(decimal? valor) => (valor ?? 0m).ToString("C2", Mx);

        /// <summary>
        /// Genera la barra CODE_128 como PNG en memoria con ZXing.Net (System.Drawing, Windows).
        /// Devuelve null si el producto no tiene código de barras. PureBarcode=false para que la
        /// imagen incluya el texto legible debajo, igual que el legado.
        /// </summary>
        private static byte[]? GenerarBarras(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = BarrasAncho,
                    Height = BarrasAlto,
                    Margin = 2,
                    PureBarcode = false
                }
            };

            using Bitmap bmp = writer.Write(codigo);
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
#pragma warning restore CA1416
