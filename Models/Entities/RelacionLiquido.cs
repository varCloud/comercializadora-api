namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Relación del proceso de producción de líquidos: por cada materia prima a granel se indica
    /// en qué producto envasado se convierte, con qué envase y la cantidad por unidad de medida.
    /// Mapea el resultset de SP_V2_CONSULTA_COMBINACION_LIQUIDOS (tabla ProductosEnvasadosXAgranel).
    /// Migra ProductosAgranelAEnvasarModel del legado (menú "Relación Liquidos").
    /// </summary>
    public class RelacionLiquido
    {
        public int IdRelacionEnvasadoAgranel { get; set; }

        public int IdProductoAgranel { get; set; }
        public string? AgranelDescripcion { get; set; }

        public int IdProductoEnvasado { get; set; }
        public string? EnvasadoDescripcion { get; set; }

        /// <summary>Id del producto envase (se conserva el typo "Produco" del esquema legado).</summary>
        public int IdProducoEnvase { get; set; }
        public string? EnvaseDescripcion { get; set; }

        /// <summary>Id de la unidad de medida (FK CatUnidadMedida). Typo "Medidad" del esquema legado.</summary>
        public int IdUnidadMedidad { get; set; }

        /// <summary>Descripción/clave SAT de la unidad de medida (derivada por el SP de guardado).</summary>
        public string? UnidadMedidad { get; set; }

        /// <summary>Cantidad por envase, ya convertida (valor / 1000) tal como se almacena.</summary>
        public decimal ValorUnidadMedida { get; set; }

        public bool Activo { get; set; }
    }
}
