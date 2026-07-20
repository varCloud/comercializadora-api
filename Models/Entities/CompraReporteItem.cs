namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del listado de "Reportes &gt; Compras" (migra
    /// <c>ReportesController.BuscarCompras</c> + <c>ComprasDAO.ObtenerCompras(detalleCompra: true)</c>
    /// del legado). A diferencia del legado (una fila por producto de la compra), aquí cada fila
    /// resume una compra completa: <see cref="TotalCantProductos"/> reemplaza el detalle línea a
    /// línea con un conteo, igual que pide la HU ("Productos: resumen count"). Mapea el resultset
    /// de <c>SP_V2_CONSULTA_COMPRAS_REPORTE</c>.
    /// </summary>
    public class CompraReporteItem
    {
        public int IdCompra { get; set; }
        public DateTime FechaAlta { get; set; }

        public int IdProveedor { get; set; }
        public string? ProveedorNombre { get; set; }

        public int IdStatus { get; set; }
        public string? EstatusDescripcion { get; set; }

        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }

        public decimal MontoTotal { get; set; }
        public double TotalCantProductos { get; set; }
    }
}
