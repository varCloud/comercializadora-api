namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Renglón del proceso de producción a granel (conversión MPL → líquido a granel).
    /// Mapea el resultset de datos de <c>SP_V2_CONSULTA_PROCESO_PRODUCCION_AGRANEL</c>. Migra
    /// <c>CostoProduccionAgranel</c> del legado (pantalla "Producción Agranel") y expone además
    /// los ids (proceso/producto/ubicación/almacén/estatus) que necesita el flujo de aprobación.
    /// </summary>
    public class ProcesoProduccionAgranel
    {
        public long IdProcesoProduccionAgranel { get; set; }
        public int IdProducto { get; set; }
        public int IdUbicacion { get; set; }
        public int? IdAlmacen { get; set; }
        public int IdUsuario { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CantidadAceptada { get; set; }
        public decimal CantidadRestante { get; set; }
        public DateTime FechaAlta { get; set; }
        public string? CodigoBarras { get; set; }
        public string? DescripcionProducto { get; set; }
        public int IdLineaProducto { get; set; }
        public string? DescripcionLinea { get; set; }
        public int IdEstatusProduccionAgranel { get; set; }
        public string? DescripcionEstatus { get; set; }
        public decimal UltimoCostoCompra { get; set; }
        public string? NombreUsuario { get; set; }
    }
}
