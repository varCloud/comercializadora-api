using System.Data;
using System.Globalization;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.Clientes
{
    /// <summary>
    /// Repositorio del módulo de Clientes (Repository + Dapper + Stored Procedures).
    /// Migra ClienteDAO del legado. El listado usa SP_V2_CONSULTA_CLIENTES (paginado,
    /// resultset aplanado); guardar/estatus/catálogos reutilizan los SP legados sin
    /// modificarlos. Nota: SP_INSERTA_ACTUALIZA_CLIENTES y SP_ACTUALIZA_STATUS_CLIENTES
    /// devuelven la cabecera como Estatus/Mensaje (PascalCase), por eso se leen con mapeo
    /// tipado a Notificacion&lt;string&gt; en vez de EjecutarAsync (que espera "status").
    /// </summary>
    public sealed class ClientesRepository : BaseRepository, IClientesRepository
    {
        /// <summary>Paridad con el DAO legado: nombres/apellidos a Title Case al guardar.</summary>
        private static readonly TextInfo TitleCase = CultureInfo.GetCultureInfo("es-MX").TextInfo;

        public ClientesRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<Cliente>> ListarAsync(PagedQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idCliente", 0);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<Cliente>("SP_V2_CONSULTA_CLIENTES", p);
        }

        public async Task<Notificacion<Cliente>> ObtenerPorIdAsync(int idCliente)
        {
            var p = new DynamicParameters();
            p.Add("@idCliente", idCliente);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_V2_CONSULTA_CLIENTES", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<Cliente>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<Cliente>();

            return notificacion;
        }

        public async Task<Notificacion<string>> GuardarAsync(GuardarClienteRequest c)
        {
            var esMoral = c.EsPersonaMoral;

            var p = new DynamicParameters();
            p.Add("@idCliente", c.IdCliente);
            // Persona moral: la razón social viaja en @nombres y los apellidos van vacíos
            // (contrato del SP legado). Persona física: Title Case como el DAO legado.
            p.Add("@nombres", esMoral ? (c.RazonSocial ?? string.Empty).Trim() : ATitleCase(c.Nombres));
            p.Add("@apellidoPaterno", esMoral ? string.Empty : ATitleCase(c.ApellidoPaterno));
            p.Add("@apellidoMaterno", esMoral ? string.Empty : ATitleCase(c.ApellidoMaterno));
            p.Add("@telefono", c.Telefono);
            p.Add("@correo", c.Correo);
            p.Add("@rfc", c.Rfc);
            p.Add("@calle", c.Calle);
            p.Add("@numeroExterior", c.NumeroExterior);
            p.Add("@colonia", c.Colonia);
            p.Add("@municipio", c.Municipio);
            p.Add("@cp", c.Cp);
            p.Add("@estado", c.Estado);
            p.Add("@numeroInterior", c.NumeroInterior);
            p.Add("@localidad", c.Localidad);
            p.Add("@idTipoCliente", c.IdTipoCliente);
            p.Add("@esPersonaMoral", esMoral ? 1 : 0);       // columna/param int en el legado
            p.Add("@nombreContacto", c.NombreContacto);
            p.Add("@sociedadMercantil", esMoral ? c.SociedadMercantil : null);
            // ------- Pedidos especiales -------
            p.Add("@latitud", c.Latitud);
            p.Add("@longitud", c.Longitud);
            p.Add("@nombreContactoPE", c.NombreContactoPE);
            p.Add("@telefonoContactoPE", c.TelefonoContactoPE);
            p.Add("@correoContactoPE", c.CorreoContactoPE);
            p.Add("@diasCredito", c.DiasCredito);
            p.Add("@montoMaximoCredito", c.MontoMaximoCredito);
            p.Add("@usarDatosCliente", c.UsarDatosCliente ? 1 : 0);
            p.Add("@idRegimenFiscal", c.IdRegimenFiscal);

            return await EjecutarLegadoPascalAsync("SP_INSERTA_ACTUALIZA_CLIENTES", p);
        }

        public async Task<Notificacion<string>> CambiarEstatusAsync(int idCliente, bool activo)
        {
            var p = new DynamicParameters();
            p.Add("@idCliente", idCliente);
            // Bug del legado corregido: ClienteDAO.EliminarCliente mandaba c.nombres en @activo;
            // el SP declara @activo bit (verificado en BD) y aquí se manda el booleano real.
            p.Add("@activo", activo);
            return await EjecutarLegadoPascalAsync("SP_ACTUALIZA_STATUS_CLIENTES", p);
        }

        public Task<Notificacion<IEnumerable<TipoCliente>>> ObtenerTiposActivosAsync()
        {
            // SP legado sin modificar: con @idTipoCliente = 0 lista todos los ACTIVOS
            // (cabecera status/mensaje + resultset), justo lo que necesita el dropdown.
            var p = new DynamicParameters();
            p.Add("@idTipoCliente", 0);
            return ConsultarAsync<TipoCliente>("SP_CONSULTA_TIPOS_CLIENTES", p);
        }

        public async Task<Notificacion<IEnumerable<RegimenFiscal>>> ObtenerRegimenesFiscalesAsync()
        {
            // SP legado sin modificar. Su cabecera trae SOLO la columna status (sin mensaje),
            // por eso se lee a mano en vez de usar ConsultarAsync (que espera cabecera.mensaje).
            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_FACTURACION_OBTENER_REGIMEN_FISCAL", commandType: CommandType.StoredProcedure);

            var cabecera = (IDictionary<string, object>)await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<RegimenFiscal>>
            {
                Estatus = Convert.ToInt32(cabecera["status"]),
                Mensaje = cabecera.TryGetValue("mensaje", out var mensaje) ? mensaje?.ToString() : "OK"
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = (await multi.ReadAsync<RegimenFiscal>()).ToList();

            return notificacion;
        }

        /// <summary>
        /// Ejecuta un SP legado de escritura cuya cabecera viene como Estatus/Mensaje
        /// (PascalCase). El mapeo tipado de Dapper es case-insensitive por propiedad,
        /// a diferencia del acceso dinámico de EjecutarAsync (fila.status).
        /// </summary>
        private async Task<Notificacion<string>> EjecutarLegadoPascalAsync(
            string storedProcedure, DynamicParameters parametros)
        {
            using IDbConnection db = CreateConnection();
            return await db.QueryFirstAsync<Notificacion<string>>(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);
        }

        private static string ATitleCase(string? valor)
            => string.IsNullOrWhiteSpace(valor) ? string.Empty : TitleCase.ToTitleCase(valor.Trim());
    }
}
