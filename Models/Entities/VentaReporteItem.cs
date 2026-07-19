namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del listado de "Reportes &gt; Ventas" (paridad de columnas con la vista legada
    /// <c>Views/Reportes/_Ventas.cshtml</c>, 17 campos). <see cref="VentaReporteRepository"/> la
    /// arma a partir del resultset "modo reportes" (<c>@tipoConsulta = 1</c>) de
    /// <c>SP_CONSULTA_VENTAS</c>; <see cref="Utilidad"/>/<see cref="MargenBruto"/> replican el
    /// cálculo que hacía la vista legada por fila (no lo devuelve el SP): ganancia unitaria y
    /// precio unitario, multiplicados por <see cref="Cantidad"/>.
    /// </summary>
    public class VentaReporteItem
    {
        public DateTime Fecha { get; set; }
        public string? Sucursal { get; set; }
        public string? Tienda { get; set; }
        public string? Cajero { get; set; }
        public int Folio { get; set; }
        public string? Cliente { get; set; }
        public string? CodigoBarras { get; set; }
        public string? LineaProducto { get; set; }
        public string? Producto { get; set; }
        public double Cantidad { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Iva { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal CostoCompra { get; set; }
        public decimal Utilidad { get; set; }
        public decimal MargenBruto { get; set; }
        public string? FormaPago { get; set; }
    }
}
