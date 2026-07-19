using System.Globalization;
using System.Text;
using CsvHelper;

namespace comercializadora_api.Services.Exportacion
{
    /// <summary>
    /// Implementación de <see cref="ICsvGeneratorService"/> con CsvHelper. Migra la lógica de
    /// <c>generaCSVInventario</c> del legado (que armaba el CSV a mano con <c>StringBuilder</c> y
    /// escapaba comillas manualmente): aquí el quoting/escaping de campos con comas, comillas o
    /// saltos de línea lo resuelve la librería, no código propio.
    /// </summary>
    public sealed class CsvGeneratorService : ICsvGeneratorService
    {
        public byte[] Generar<T>(IEnumerable<T> datos, IReadOnlyList<ColumnaExportable<T>> columnas)
        {
            using var memoria = new MemoryStream();
            using (var escritorTexto = new StreamWriter(memoria, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
            using (var csv = new CsvWriter(escritorTexto, CultureInfo.InvariantCulture))
            {
                foreach (var columna in columnas)
                    csv.WriteField(columna.Encabezado);
                csv.NextRecord();

                foreach (var item in datos)
                {
                    foreach (var columna in columnas)
                        csv.WriteField(FormatearValor(columna.Valor(item)));
                    csv.NextRecord();
                }
            }

            return memoria.ToArray();
        }

        /// <summary>
        /// Formatea a texto invariant-culture (evita que un separador decimal de coma choque con
        /// el delimitador de columnas del CSV). El quoting del campo resultante lo hace CsvWriter.
        /// </summary>
        private static string? FormatearValor(object? valor) => valor switch
        {
            null => null,
            string texto => texto,
            bool booleano => booleano ? "true" : "false",
            DateTime fecha => fecha.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateOnly fecha => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            IFormattable formateable => formateable.ToString(null, CultureInfo.InvariantCulture),
            _ => valor.ToString(),
        };
    }
}
