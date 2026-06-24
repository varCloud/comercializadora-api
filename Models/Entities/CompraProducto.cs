namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Línea de detalle de una compra (un producto). Mapea el tercer resultset de
    /// SP_V2_CONSULTA_COMPRA_DETALLE. `IdEstatusProducto` &gt; 0 indica que el producto ya fue
    /// recibido/devuelto: en el panel su cantidad no es editable y la fila no se puede eliminar.
    /// </summary>
    public class CompraProducto
    {
        public int IdProducto { get; set; }
        public string? Descripcion { get; set; }

        /// <summary>0 = pendiente; &gt; 0 = ya tiene estatus (recibido/devuelto).</summary>
        public int IdEstatusProducto { get; set; }
        public string? EstatusProducto { get; set; }

        public string? Observaciones { get; set; }

        public double CantidadRecibida { get; set; }
        public double CantidadDevuelta { get; set; }

        /// <summary>Cantidad solicitada (comprada).</summary>
        public double Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }

        /// <summary>Si la línea del producto permite fraccionar (dbo.LineaProductoFraccion).</summary>
        public bool Fraccion { get; set; }
    }
}
