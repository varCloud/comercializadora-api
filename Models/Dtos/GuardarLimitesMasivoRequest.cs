using System.Collections.Generic;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de la carga masiva de límites (resultado del Excel parseado en el front).
    /// Cada ítem trae código de barras + almacén (por descripción) + mín/máx; el SP masivo
    /// resuelve producto/almacén. El idUsuario se toma del JWT en el controller.
    /// </summary>
    public class GuardarLimitesMasivoRequest
    {
        public List<LimiteMasivoItem> Limites { get; set; } = new();
    }

    /// <summary>Ítem de la carga masiva (una fila del Excel).</summary>
    public class LimiteMasivoItem
    {
        public string? CodigoBarras { get; set; }
        public string? DescripcionAlmacen { get; set; }
        public int Minimo { get; set; }
        public int Maximo { get; set; }
    }
}
