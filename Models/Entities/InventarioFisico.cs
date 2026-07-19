namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Inventario físico (conteo de existencias por sucursal). Mapea el resultset de datos de
    /// <c>SP_V2_CONSULTA_INVENTARIO_FISICO</c> con multi-mapping de Dapper
    /// (splitOn "idSucursal,idStatus" → <see cref="Sucursal"/> y <see cref="Estatus"/> anidados).
    /// Migra el modelo <c>InventarioFisico</c> del legado; el Usuario que mapeaba el DAO viejo
    /// no se expone porque la pantalla migrada no lo muestra.
    /// </summary>
    public class InventarioFisico
    {
        public int IdInventarioFisico { get; set; }
        public string? Nombre { get; set; }
        public string? Observaciones { get; set; }

        /// <summary>La fija el SP de estatus al iniciar (estatus 2); null mientras está pendiente.</summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>La fija el SP de estatus al finalizar/cancelar (3/4); null antes.</summary>
        public DateTime? FechaFin { get; set; }

        public DateTime? FechaAlta { get; set; }

        /// <summary>Tipo de inventario: 1 = General, 2 = Individual (enum fijo, sin catálogo en BD).</summary>
        public int? IdTipoInventario { get; set; }

        /// <summary>Texto del tipo ("General"/"Individual"), calculado por el SP.</summary>
        public string? TipoInventario { get; set; }

        public Sucursal? Sucursal { get; set; }

        /// <summary>Estatus: 1 Pendiente, 2 Iniciado, 3 Finalizado, 4 Cancelado (CatEstatusInventarioFisico).</summary>
        public Status? Estatus { get; set; }
    }
}
