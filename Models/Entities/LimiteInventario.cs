namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Fila del listado de Límites de Inventario: límites mínimo/máximo de un producto en un
    /// almacén, su cantidad actual, la cantidad sugerida de compra y su estatus de nivel.
    /// Migra el modelo LimiteInvetario del legado (con su estatus anidado por multi-mapping).
    /// </summary>
    public class LimiteInventario
    {
        public int IdProducto { get; set; }
        public int IdAlmacen { get; set; }
        public int IdLimiteInventario { get; set; }
        public int IdLineaProducto { get; set; }

        public int Minimo { get; set; }
        public int Maximo { get; set; }

        public string? Descripcion { get; set; }
        public string? DescripcionAlmacen { get; set; }
        public string? DescripcionLineaProducto { get; set; }
        public string? CodigoBarras { get; set; }

        public int CantidadInventario { get; set; }

        /// <summary>Cantidad sugerida de compra = máximo − cantidad actual (se calcula en el repo).</summary>
        public int CantidadSugerida { get; set; }

        /// <summary>Estatus del nivel de inventario (anidado por multi-mapping de Dapper).</summary>
        public Status? EstatusInventario { get; set; }
    }
}
