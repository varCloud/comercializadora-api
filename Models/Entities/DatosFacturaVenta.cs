namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Datos de la venta/cliente necesarios para reenviar la factura por correo. Mapea
    /// <c>SP_FACTURACION_OBTENER_DATOS_FACTURA</c> (equivalente a <c>FacturaDAO.ObtenerDetalleFactura</c>
    /// del legado). No se expone tal cual como endpoint propio: lo consume internamente
    /// <c>FacturacionService.ReenviarAsync</c> para resolver el correo destino y el path del PDF/XML.
    /// </summary>
    public class DatosFacturaVenta
    {
        public string? Nombre { get; set; }
        public string? Rfc { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Domicilio { get; set; }
        public string? TipoCliente { get; set; }
        public string? UsoCfdi { get; set; }
        public string? DescripcionUsoCfdi { get; set; }
        public string? FormaPago { get; set; }
        public string? DescripcionFormaPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string? PathArchivoFactura { get; set; }
    }
}
