namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload para guardar los precios de un producto (PUT /productos/{id}/precios).
    /// Precios base + lista de rangos de mayoreo. Se envuelve en el SP legado
    /// SP_INSERTA_ACTUALIZA_RANGOS_PRECIOS (recibe los rangos como XML; la BD está en
    /// compatibility_level 120 y no soporta OPENJSON).
    /// </summary>
    public class GuardarPreciosRequest
    {
        public decimal PrecioIndividual { get; set; }
        public decimal PrecioMenudeo { get; set; }
        public decimal? UltimoCostoCompra { get; set; }
        public double? PorcUtilidadIndividual { get; set; }
        public double? PorcUtilidadMayoreo { get; set; }

        public List<RangoPrecioInput> Rangos { get; set; } = new();
    }

    /// <summary>Un rango de mayoreo capturado en el formulario (min/max enteros, costo decimal).</summary>
    public class RangoPrecioInput
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public decimal Costo { get; set; }
        public double PorcUtilidad { get; set; }
    }
}
