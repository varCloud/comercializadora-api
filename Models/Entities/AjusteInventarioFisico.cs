namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Renglón de ajuste de un inventario físico (producto contado en una ubicación). Mapea el
    /// resultset de datos de <c>SP_CONSULTA_AJUSTE_INVENTARIO</c> (SP legado reusado) con
    /// multi-mapping de Dapper (splitOn "idProducto" → <see cref="Producto"/> anidado). Migra el
    /// modelo <c>AjusteInventarioFisico</c> del legado; el Usuario que mapeaba el DAO viejo no
    /// se expone porque el diálogo migrado no lo muestra.
    /// </summary>
    public class AjusteInventarioFisico
    {
        public long IdAjusteInventarioFisico { get; set; }

        /// <summary>Existencia según sistema al iniciar el inventario.</summary>
        public double? CantidadActual { get; set; }

        /// <summary>Cantidad contada físicamente (float en BD).</summary>
        public double? CantidadEnFisico { get; set; }

        /// <summary>Diferencia a ajustar; sobrante/faltante se deriva del signo contra la actual.</summary>
        public double? CantidadAAjustar { get; set; }

        public DateTime? FechaAlta { get; set; }

        /// <summary>false = "Sin Ajustar" (aún no contado); el SP hace coalesce(ajustado, 0).</summary>
        public bool Ajustado { get; set; }

        /// <summary>Marca de error de proceso/humano capturada desde la app móvil.</summary>
        public int? ErrorHumano { get; set; }

        public int IdInventarioFisico { get; set; }

        /// <summary>Estatus del inventario padre (1 Pendiente, 2 Iniciado, 3 Finalizado, 4 Cancelado).</summary>
        public int? IdEstatusInventarioFisico { get; set; }

        public ProductoAjusteInventario? Producto { get; set; }
    }
}
