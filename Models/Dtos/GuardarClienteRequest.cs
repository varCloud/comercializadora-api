using System.ComponentModel.DataAnnotations;

namespace comercializadora_api.Models.Dtos
{
    /// <summary>
    /// Payload de alta/edición de cliente (POST/PUT). Envuelve SP_INSERTA_ACTUALIZA_CLIENTES
    /// (legado, 28 parámetros, sin modificar). DTO único física/moral: desaparece la dupla
    /// rfc/rfcPM del legado; el repositorio resuelve el mapeo (persona moral → razón social
    /// en @nombres y apellidos vacíos) y aplica Title Case, como el ClienteDAO legado.
    /// </summary>
    public class GuardarClienteRequest : IValidatableObject
    {
        /// <summary>0 = alta; > 0 = edición. En PUT lo fija la ruta.</summary>
        public int IdCliente { get; set; }

        /// <summary>true = persona moral; activa la validación condicional.</summary>
        public bool EsPersonaMoral { get; set; }

        /// <summary>Requerido para persona física (validación condicional).</summary>
        [MaxLength(250)]
        public string? Nombres { get; set; }

        /// <summary>Requerido para persona física (validación condicional).</summary>
        [MaxLength(50)]
        public string? ApellidoPaterno { get; set; }

        [MaxLength(50)]
        public string? ApellidoMaterno { get; set; }

        /// <summary>Requerida para persona moral (validación condicional).</summary>
        [MaxLength(250)]
        public string? RazonSocial { get; set; }

        [MaxLength(50)]
        public string? SociedadMercantil { get; set; }

        /// <summary>Requerido para persona moral (validación condicional).</summary>
        [MaxLength(50)]
        public string? Rfc { get; set; }

        /// <summary>Opcional; si se captura debe ser exactamente 10 dígitos.</summary>
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe ser de 10 dígitos.")]
        public string? Telefono { get; set; }

        [MaxLength(50)]
        [EmailAddress]
        public string? Correo { get; set; }

        // ------- Domicilio -------
        [MaxLength(50)]
        public string? Calle { get; set; }

        [MaxLength(50)]
        public string? NumeroExterior { get; set; }

        [MaxLength(20)]
        public string? NumeroInterior { get; set; }

        [MaxLength(50)]
        public string? Colonia { get; set; }

        [MaxLength(250)]
        public string? Localidad { get; set; }

        [MaxLength(50)]
        public string? Municipio { get; set; }

        [MaxLength(50)]
        public string? Estado { get; set; }

        /// <summary>Código postal (obligatorio para facturar).</summary>
        [Required(ErrorMessage = "El código postal es requerido (obligatorio para facturar).")]
        [MaxLength(50)]
        public string? Cp { get; set; }

        // ------- Contacto -------
        [MaxLength(250)]
        public string? NombreContacto { get; set; }

        // ------- Pedidos especiales -------
        [MaxLength(250)]
        public string? Latitud { get; set; }

        [MaxLength(250)]
        public string? Longitud { get; set; }

        [MaxLength(250)]
        public string? NombreContactoPE { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono de contacto PE debe ser de 10 dígitos.")]
        public string? TelefonoContactoPE { get; set; }

        [MaxLength(50)]
        [EmailAddress]
        public string? CorreoContactoPE { get; set; }

        /// <summary>Usar los datos generales del cliente como contacto de pedidos especiales.</summary>
        public bool UsarDatosCliente { get; set; }

        // ------- Crédito -------
        [Range(0, int.MaxValue)]
        public int DiasCredito { get; set; }

        [Range(0, double.MaxValue)]
        public double MontoMaximoCredito { get; set; }

        // ------- Clasificación / fiscales -------
        [Range(1, 90, ErrorMessage = "El tipo de cliente es requerido.")]
        public int IdTipoCliente { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El régimen fiscal es requerido (obligatorio para facturar).")]
        public int IdRegimenFiscal { get; set; }

        /// <summary>Validación condicional persona física / moral (paridad con el legado).</summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EsPersonaMoral)
            {
                if (string.IsNullOrWhiteSpace(RazonSocial))
                    yield return new ValidationResult(
                        "La razón social es requerida para persona moral.", new[] { nameof(RazonSocial) });

                if (string.IsNullOrWhiteSpace(Rfc))
                    yield return new ValidationResult(
                        "El RFC es requerido para persona moral.", new[] { nameof(Rfc) });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Nombres))
                    yield return new ValidationResult(
                        "Los nombres son requeridos para persona física.", new[] { nameof(Nombres) });

                if (string.IsNullOrWhiteSpace(ApellidoPaterno))
                    yield return new ValidationResult(
                        "El apellido paterno es requerido para persona física.", new[] { nameof(ApellidoPaterno) });
            }
        }
    }
}
