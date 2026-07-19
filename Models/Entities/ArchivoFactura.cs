namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Resultado de <c>SP_FACTURAS_OBTENER_PATH_ARCHIVO</c>: path relativo del PDF + UUID de la
    /// factura o pedido especial consultado. El UUID se usa para construir la "expresión
    /// impresa" de consulta de estatus ante el SAT.
    /// </summary>
    public class ArchivoFactura
    {
        public string? PathArchivoFactura { get; set; }
        public string? Uuid { get; set; }
    }
}
