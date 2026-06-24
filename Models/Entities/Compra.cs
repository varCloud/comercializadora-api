namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Compra a proveedor (cabecera). Mapea el resultset de SP_V2_CONSULTA_COMPRAS (listado) y
    /// la cabecera de SP_V2_CONSULTA_COMPRA_DETALLE. Las métricas (montoTotal, cantidades,
    /// estadoCompra) las calcula el SP a partir de ComprasDetalle. En la lectura por id se
    /// rellena <see cref="ListProductos"/> con el detalle de la compra.
    /// </summary>
    public class Compra
    {
        public int IdCompra { get; set; }
        public DateTime FechaAlta { get; set; }
        public string? Observaciones { get; set; }

        public int IdAlmacen { get; set; }
        public string? Almacen { get; set; }

        public int IdProveedor { get; set; }
        public string? ProveedorNombre { get; set; }

        /// <summary>Estatus de la compra (1 Pendiente, 2 Realizada, 3 Finalizada, 4 Cancelada, 5 Devolución).</summary>
        public int IdStatus { get; set; }
        public string? EstatusDescripcion { get; set; }

        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }

        public decimal MontoTotal { get; set; }
        public double TotalCantProductos { get; set; }
        public double TotalCantProductosRecibidos { get; set; }
        public double TotalCantProductosDevueltos { get; set; }
        public decimal MontoTotalRecibido { get; set; }

        /// <summary>Estado derivado: 0 = pendiente, 1 = correcta, 2 = incorrecta (productos con estatus 3/4/5).</summary>
        public int EstadoCompra { get; set; }

        /// <summary>Detalle de productos (solo en la lectura por id).</summary>
        public List<CompraProducto> ListProductos { get; set; } = new();
    }
}
