namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Renglón del reporte "Devoluciones y Complementos" (menú legado "Reportes &gt; Devoluciones",
    /// <c>ReportesDAO.ObtenerDevolucionesyComplementos</c>). Mapea el resultset único de
    /// <c>SP_CONSULTA_DEVOLUCIONES_Y_COMPLEMENTOS</c> (verificado contra el SP real en BD, no el
    /// modelo <c>Ventas</c> genérico del legado, que trae ~60 campos ajenos a este reporte): el SP
    /// arma dos <c>SELECT</c> (Devoluciones/Complementos) unidos con <c>UNION</c> según
    /// <c>@idTipoConsulta</c>, cada uno con las mismas 16 columnas. Nombres de propiedad iguales
    /// (case-insensitive) a las columnas del resultset para que Dapper las mapee automáticamente,
    /// sin clase de fila intermedia.
    /// </summary>
    public class DevolucionItem
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public int IdCliente { get; set; }
        public string? NombreCliente { get; set; }
        public int IdSucursal { get; set; }
        public int IdAlmacen { get; set; }
        public int IdProducto { get; set; }
        public string? DescripcionProducto { get; set; }

        /// <summary>Cantidad del renglón (columna <c>DevolucionesDetalle.cantidad</c>/<c>ComplementosDetalle.cantidad</c>, tipo <c>float</c> en BD).</summary>
        public double Cantidad { get; set; }

        /// <summary>Monto total del renglón (<c>monto</c> + comisión bancaria correspondiente, ya sumados por el SP).</summary>
        public decimal MontoTotal { get; set; }

        public decimal PrecioVenta { get; set; }
        public DateTime FechaAlta { get; set; }
        public string? DescAlmacen { get; set; }
        public string? CodigoBarras { get; set; }

        /// <summary>
        /// Tipo de movimiento tal cual lo etiqueta el propio SP: <c>"Devolución"</c> o
        /// <c>"Complemento"</c> (columna literal <c>descripcion</c> del <c>SELECT</c>, no la
        /// descripción del producto).
        /// </summary>
        public string? Descripcion { get; set; }
    }
}
