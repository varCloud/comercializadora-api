namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Rango de precio por volumen ("super mayoreo") de un producto. Mapea ProductosPorPrecio
    /// (filas activas). El precio del rango vive en <see cref="Costo"/>.
    /// </summary>
    public class RangoPrecio
    {
        public int Contador { get; set; }
        public int IdProducto { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public decimal Costo { get; set; }
        public double PorcUtilidad { get; set; }
    }
}
