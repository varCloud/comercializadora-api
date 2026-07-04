namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Tipo de cliente (catálogo CatTipoCliente): clasifica a cada cliente y define el % de
    /// descuento que le aplica. Era la pantalla "Descuentos" del legado. Mapea el resultset
    /// de SP_V2_CONSULTA_TIPOS_CLIENTES y el del legado SP_CONSULTA_TIPOS_CLIENTES (catálogo).
    /// </summary>
    public class TipoCliente
    {
        public int IdTipoCliente { get; set; }

        public string? Descripcion { get; set; }

        /// <summary>% de descuento (0–100; columna money en BD).</summary>
        public decimal Descuento { get; set; }

        public bool Activo { get; set; }
    }
}
