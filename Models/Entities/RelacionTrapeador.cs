namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Relación del proceso de producción de trapeadores: por cada combinación de materia prima
    /// (Matra) + bastón se indica en qué trapeador se convierte, con la cantidad por unidad de
    /// medida. Mapea el resultset de SP_V2_CONSULTA_COMBINACION_PRODUCCION_PRODUCTOS (tabla
    /// ProduccionProductos). Migra ProduccionProductosModel del legado (menú "Relación Trapeadores").
    /// </summary>
    public class RelacionTrapeador
    {
        public int Id { get; set; }

        /// <summary>Id de la materia prima / Matra.</summary>
        public int IdProductoMateria1 { get; set; }
        public string? ProductoMateria1Descripcion { get; set; }

        /// <summary>Id del bastón.</summary>
        public int IdProductoMateria2 { get; set; }
        public string? ProductoMateria2Descripcion { get; set; }

        /// <summary>Id del trapeador producido.</summary>
        public int IdProductoProduccion { get; set; }
        public string? ProductoProduccionDescripcion { get; set; }

        /// <summary>Id de la unidad de medida (FK CatUnidadMedida). Typo "Medidad" del esquema legado.</summary>
        public int IdUnidadMedidad { get; set; }

        /// <summary>Descripción/clave SAT de la unidad de medida (derivada por el SP de guardado).</summary>
        public string? UnidadMedidad { get; set; }

        /// <summary>Cantidad por unidad, ya convertida (valor / 1000) tal como se almacena.</summary>
        public decimal ValorUnidadMedida { get; set; }

        public bool Activo { get; set; }
    }
}
