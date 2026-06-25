using System.Data;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.RelacionLiquidos
{
    /// <summary>
    /// Repositorio del módulo "Relación Liquidos" (Repository + Dapper + Stored Procedures).
    /// Migra ProductosAgranelAEnvasarDAO del legado. El listado usa SP_V2_CONSULTA_COMBINACION_LIQUIDOS
    /// (paginado); el guardado y la baja reutilizan los SP legados, cuya cabecera viene en la columna
    /// "estatus" (no "status"), por lo que se leen manualmente.
    /// </summary>
    public sealed class RelacionLiquidosRepository : BaseRepository, IRelacionLiquidosRepository
    {
        private const string SpConsulta   = "SP_V2_CONSULTA_COMBINACION_LIQUIDOS";
        private const string SpGuardar    = "SP_AGREGA_ACTUALIZA_COMBINACION_PRODUCTOS_ENSAVDOS_A_AGRANEL";
        private const string SpDesactivar = "SP_DESACTIVAR_COMBINACION_PRODUCTOS_ENVASADOS_A_AGRANEL";
        private const string SpUnidades   = "SP_OBTENER_UNIDADES_DE_MEDIDA_LIQUIDOS_AGRANEL";

        public RelacionLiquidosRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<RelacionLiquido>> ListarAsync(PagedQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idRelacionEnvasadoAgranel", 0);
            p.Add("@search", string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim());
            p.Add("@order", query.Order);
            p.Add("@sort", query.Sort);
            p.Add("@pageNumber", query.Page);
            p.Add("@pageSize", query.PerPage);
            return ConsultarPaginaAsync<RelacionLiquido>(SpConsulta, p);
        }

        public async Task<Notificacion<RelacionLiquido>> ObtenerPorIdAsync(int idRelacionEnvasadoAgranel)
        {
            var p = new DynamicParameters();
            p.Add("@idRelacionEnvasadoAgranel", idRelacionEnvasadoAgranel);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", 1);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpConsulta, p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<RelacionLiquido>
            {
                Estatus = (int)cabecera.status,
                Mensaje = (string?)cabecera.mensaje
            };

            if (notificacion.EsExitoso)
                notificacion.Modelo = await multi.ReadFirstOrDefaultAsync<RelacionLiquido>();

            return notificacion;
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarRelacionLiquidoRequest request)
        {
            var p = new DynamicParameters();
            // idRelacionEnvasadoAgranel = 0 => el SP hace INSERT; > 0 => UPDATE.
            p.Add("@idRelacionEnvasadoAgranel", request.IdRelacionEnvasadoAgranel);
            p.Add("@idProductoEnvasado", request.IdProductoEnvasado);
            p.Add("@idProductoAgranel", request.IdProductoAgranel);
            p.Add("@idProducoEnvase", request.IdProducoEnvase);
            // El SP deriva @unidadMedidad (clave SAT) desde @idUnidadMedida; se manda vacío.
            p.Add("@unidadMedidad", string.Empty);
            p.Add("@valorUnidadMedida", request.ValorUnidadMedida);
            p.Add("@idUnidadMedida", request.IdUnidadMedidad);
            return EjecutarLegacyAsync(SpGuardar, p);
        }

        public Task<Notificacion<string>> DesactivarAsync(int idRelacionEnvasadoAgranel)
        {
            var p = new DynamicParameters();
            p.Add("@idRelacionEnvasadoAgranel", idRelacionEnvasadoAgranel);
            return EjecutarLegacyAsync(SpDesactivar, p);
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ListarUnidadesMedidaAsync()
        {
            // SP legado: cabecera en columna "estatus" + un resultset de CatUnidadMedida (ids 2,4).
            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                SpUnidades, null, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            var notificacion = new Notificacion<IEnumerable<CatalogoItem>>
            {
                Estatus = (int)cabecera.estatus,
                Mensaje = (string?)cabecera.mensaje
            };

            if (!notificacion.EsExitoso)
                return notificacion;

            // El resultset es SELECT * de CatUnidadMedida (idUnidadMedida, descripcion, …);
            // se normaliza a un diccionario case-insensitive y se proyecta a CatalogoItem.
            var filas = (await multi.ReadAsync())
                .Select(f => new Dictionary<string, object>(
                    (IDictionary<string, object>)f, StringComparer.OrdinalIgnoreCase));

            notificacion.Modelo = filas
                .Select(f => new CatalogoItem
                {
                    Id = Convert.ToInt32(f["idUnidadMedida"]),
                    Descripcion = f.TryGetValue("descripcion", out var d) ? d?.ToString() : null
                })
                .ToList();

            return notificacion;
        }

        /// <summary>
        /// Ejecuta un SP legado de comando cuya cabecera viene en la columna "estatus" (no "status").
        /// Lee la primera fila del primer resultset (los SP de este módulo devuelven estatus/mensaje
        /// ahí); el acceso dinámico a columnas de Dapper es case-insensitive, así que cubre tanto
        /// "estatus" como "Estatus".
        /// </summary>
        private async Task<Notificacion<string>> EjecutarLegacyAsync(string storedProcedure, object parametros)
        {
            using IDbConnection db = CreateConnection();
            var fila = await db.QueryFirstAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            return new Notificacion<string>
            {
                Estatus = (int)fila.estatus,
                Mensaje = (string?)fila.mensaje
            };
        }
    }
}
