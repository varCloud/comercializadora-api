namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Proveedor del sistema. Mapea el resultset de datos de SP_V2_CONSULTA_PROVEEDORES.
    /// Incluye métricas de surtido de pedidos (Compras) heredadas del legado.
    /// </summary>
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public bool Activo { get; set; }

        public int TotalPedidosIncompletos { get; set; }
        public int TotalPedidosTotales { get; set; }
        public int TotalPedidosCompletos { get; set; }

        /// <summary>Porcentaje de pedidos atendidos (0–100).</summary>
        public float PorcAtendido { get; set; }
    }
}
