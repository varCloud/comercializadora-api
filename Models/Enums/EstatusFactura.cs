namespace comercializadora_api.Models.Enums
{
    /// <summary>Estatus de factura (tabla <c>FacCatEstatusFactura</c>). Migra <c>EnumEstatusFactura</c> del legado.</summary>
    public enum EstatusFactura
    {
        Facturada = 1,
        Cancelada = 2,
        Error = 3,
        PendienteDeCancelacion = 4
    }
}
