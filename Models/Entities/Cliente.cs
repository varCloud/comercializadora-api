namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Cliente de la comercializadora (persona física o moral). Mapea el resultset de datos
    /// de SP_V2_CONSULTA_CLIENTES, que aplana el tipo de cliente (IdTipoCliente /
    /// TipoClienteDescripcion / Descuento) y el régimen fiscal (IdRegimenFiscal /
    /// RegimenFiscal) como columnas del cliente. Para persona moral la razón social se
    /// almacena en la columna nombres del legado; el SP V2 la separa en RazonSocial.
    /// </summary>
    public class Cliente
    {
        public int IdCliente { get; set; }

        /// <summary>true = persona moral (razón social); false = persona física.</summary>
        public bool EsPersonaMoral { get; set; }

        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }

        /// <summary>Razón social (solo persona moral).</summary>
        public string? RazonSocial { get; set; }

        /// <summary>Tipo de sociedad mercantil (S.A. de C.V., etc.; solo persona moral).</summary>
        public string? SociedadMercantil { get; set; }

        /// <summary>Nombre completo en mayúsculas (calculado por el SP, paridad con el legado).</summary>
        public string? NombreCompleto { get; set; }

        public string? Rfc { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }

        // ------- Domicilio -------
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public string? Colonia { get; set; }
        public string? Localidad { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string? Cp { get; set; }

        // ------- Contacto -------
        public string? NombreContacto { get; set; }

        // ------- Pedidos especiales -------
        public string? Latitud { get; set; }
        public string? Longitud { get; set; }
        public string? NombreContactoPE { get; set; }
        public string? TelefonoContactoPE { get; set; }
        public string? CorreoContactoPE { get; set; }

        /// <summary>Usar los datos generales del cliente como contacto de pedidos especiales.</summary>
        public bool UsarDatosCliente { get; set; }

        // ------- Crédito -------
        public int DiasCredito { get; set; }
        public double MontoMaximoCredito { get; set; }

        // ------- Datos fiscales -------
        public int IdRegimenFiscal { get; set; }

        /// <summary>Descripción del régimen fiscal (FactCatRegimenFiscal).</summary>
        public string? RegimenFiscal { get; set; }

        // ------- Tipo de cliente (aplanado) -------
        public int IdTipoCliente { get; set; }
        public string? TipoClienteDescripcion { get; set; }

        /// <summary>% de descuento del tipo de cliente.</summary>
        public decimal Descuento { get; set; }

        public bool Activo { get; set; }
        public DateTime? FechaAlta { get; set; }
    }
}
