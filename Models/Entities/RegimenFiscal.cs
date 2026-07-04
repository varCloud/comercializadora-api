namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Régimen fiscal del SAT (catálogo FactCatRegimenFiscal). Catálogo read-only para el
    /// form de Clientes; mapea el resultset de SP_FACTURACION_OBTENER_REGIMEN_FISCAL (que
    /// además de Text/Value legados trae las columnas reales idRegimenFiscal/descripcion).
    /// </summary>
    public class RegimenFiscal
    {
        public int IdRegimenFiscal { get; set; }

        public string? Descripcion { get; set; }
    }
}
