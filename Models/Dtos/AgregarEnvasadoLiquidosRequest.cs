using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload del registro de envasado de líquidos (POST /api/produccion-agranel/envasado).
    /// Envuelve <c>SP_APP_AGREGAR_PRODUCTO_INVENTARIO_LIQUIDOS_ENVASADO</c>, que valida la
    /// relación envasado↔granel (<c>ProductosEnvasadosXAgranel</c>) y el stock de granel y de
    /// envases antes de incrementar el inventario del producto envasado. Migra
    /// <c>RequestAgregarProductoInventarioLiquidos</c> (WS <c>AdminLiquidosController</c>) del
    /// legado; el idUsuario NO viaja en el body: la API lo toma del claim del JWT.
    /// </summary>
    public class AgregarEnvasadoLiquidosRequest
    {
        /// <summary>Producto envasado (debe tener relación en ProductosEnvasadosXAgranel).</summary>
        [Required]
        public int IdProducto { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public double Cantidad { get; set; }

        [Required]
        public int IdAlmacen { get; set; }
    }
}
