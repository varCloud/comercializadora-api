using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de una compra (POST/PUT). Envuelve SP_REGISTRA_COMPRA: el
    /// repositorio serializa <see cref="Productos"/> al XML &lt;ArrayOfProducto&gt; que espera el SP.
    /// El idUsuario NO viaja en el body: la API lo toma del claim del JWT.
    /// </summary>
    public class GuardarCompraRequest
    {
        /// <summary>0 = alta; &gt; 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdCompra { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        [Required]
        public int IdStatusCompra { get; set; }

        public string? Observaciones { get; set; }

        [Required]
        public int IdAlmacen { get; set; }

        /// <summary>Líneas de detalle de la compra (producto + cantidad + costo).</summary>
        public List<CompraProductoRequest> Productos { get; set; } = new();
    }

    /// <summary>Una línea del detalle de la compra en el payload de guardado.</summary>
    public class CompraProductoRequest
    {
        public long IdProducto { get; set; }
        public double Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}
