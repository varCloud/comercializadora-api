using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload del alta de producto MPL a producción a granel (POST /api/produccion-agranel).
    /// Envuelve <c>SP_APP_INVENTARIO_AGREGAR_PRODUCTO_PRODUCCION_AGRANEL</c>, que valida que el
    /// producto sea de línea MPL (12), que exista en el almacén y que haya inventario suficiente.
    /// Migra <c>RequestAgregarProductoProduccionAgranel</c> del legado; el idUsuario NO viaja en
    /// el body: la API lo toma del claim del JWT.
    /// </summary>
    public class AgregarProduccionAgranelRequest
    {
        [Required]
        public int IdProducto { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public double Cantidad { get; set; }

        [Required]
        public int IdAlmacen { get; set; }
    }
}
