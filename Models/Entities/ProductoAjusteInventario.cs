namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Producto + su ubicación física tal como lo devuelve <c>SP_CONSULTA_AJUSTE_INVENTARIO</c>
    /// (segundo segmento del multi-mapping, splitOn "idProducto"). No se reutiliza la entidad
    /// <see cref="Producto"/> del catálogo porque este resultset trae campos de ubicación
    /// (almacén/piso/pasillo/rack, con "SIN ACOMODAR" resuelto por el SP) que no pertenecen
    /// al producto maestro.
    /// </summary>
    public class ProductoAjusteInventario
    {
        public int IdProducto { get; set; }
        public string? Descripcion { get; set; }

        /// <summary>Último precio de compra (función dbo.obtenerPrecioCompra; money en BD).</summary>
        public decimal? UltimoCostoCompra { get; set; }

        public int IdLineaProducto { get; set; }
        public string? DescripcionLinea { get; set; }

        public int IdAlmacen { get; set; }
        public string? Almacen { get; set; }

        public int IdPiso { get; set; }
        public string? Piso { get; set; }

        public int IdPasillo { get; set; }
        public string? Pasillo { get; set; }

        /// <summary>Rack ("Raq" es el nombre real de la columna/tabla en BD).</summary>
        public int IdRaq { get; set; }
        public string? Raq { get; set; }
    }
}
