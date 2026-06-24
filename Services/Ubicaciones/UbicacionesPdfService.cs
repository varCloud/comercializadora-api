using System.Drawing;
using System.Text.Json;
using comercializadora_api.Models.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZXing;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

// El servicio usa System.Drawing (binding de ZXing) y la API se despliega en Windows/IIS
// (supuesto documentado en la HU). Se silencia CA1416 (API solo soportada en Windows).
#pragma warning disable CA1416

namespace comercializadora_api.Services.Ubicaciones
{
    /// <summary>
    /// Implementación del generador de PDF de etiquetas QR de ubicaciones.
    /// QR con ZXing.Net (reutiliza el enfoque del legado, GenerarQR) y maquetado del PDF con QuestPDF.
    /// El QR codifica el mismo JSON que el legado para compatibilidad con los lectores existentes.
    /// </summary>
    public sealed class UbicacionesPdfService : IUbicacionesPdfService
    {
        private const int QrLado = 240;          // px del QR generado
        private const float ColumnasPorFila = 4; // 4 QR por fila, como el legado

        public byte[] GenerarPdf(IReadOnlyList<UbicacionImprimir> ubicaciones)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(t => t.FontSize(7).FontColor(Colors.Black));

                    page.Header().PaddingBottom(8).Text("Ubicaciones")
                        .FontSize(14).SemiBold();

                    page.Content().Column(col =>
                    {
                        foreach (var fila in ubicaciones.Chunk((int)ColumnasPorFila))
                        {
                            col.Item().PaddingVertical(6).Row(row =>
                            {
                                foreach (var u in fila)
                                {
                                    row.RelativeItem().Padding(4).Column(celda =>
                                    {
                                        celda.Item().AlignCenter().Width(110)
                                            .Image(GenerarQr(ConstruirContenidoQr(u)));
                                        celda.Item().PaddingTop(2).AlignCenter()
                                            .Text(ConstruirPie(u));
                                    });
                                }

                                // Rellena columnas vacías para mantener la rejilla de 4 alineada.
                                for (int k = fila.Length; k < ColumnasPorFila; k++)
                                    row.RelativeItem();
                            });
                        }
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// JSON que codifica el QR. Conserva las claves exactas del legado
        /// (idAlmacen, idPasillo, Pasillo, idRack, Rack, idPiso, Piso) para no romper lectores.
        /// </summary>
        private static string ConstruirContenidoQr(UbicacionImprimir u) => JsonSerializer.Serialize(new
        {
            idAlmacen = u.IdAlmacen.ToString(),
            idPasillo = u.IdPasillo.ToString(),
            Pasillo = (u.DescripcionPasillo ?? string.Empty).Trim(),
            idRack = u.IdRaq.ToString(),
            Rack = u.DescripcionRaq ?? string.Empty,
            idPiso = u.IdPiso.ToString(),
            Piso = u.DescripcionPiso ?? string.Empty
        });

        /// <summary>Texto bajo el QR, equivalente al pie del legado.</summary>
        private static string ConstruirPie(UbicacionImprimir u) =>
            $"Almacén: {u.DescripcionAlmacen} Pasillo: {u.DescripcionPasillo}, Rack: {u.IdRaq}, Piso: {u.IdPiso}";

        /// <summary>
        /// Genera el QR como PNG en memoria con ZXing.Net (binding de System.Drawing, Windows).
        /// Equivale a Utilerias.Utils.GenerarQR del legado.
        /// </summary>
        private static byte[] GenerarQr(string contenido)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = QrLado,
                    Height = QrLado,
                    Margin = 1
                }
            };

            using Bitmap bmp = writer.Write(contenido);
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
#pragma warning restore CA1416
