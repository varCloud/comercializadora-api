using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Dapper;
using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;

namespace comercializadora_api.Repositories.LimitesInventario
{
    /// <summary>
    /// Repositorio del módulo Límites de Inventario (Repository + Dapper + Stored Procedures).
    /// Migra LimiteInventarioDAO del legado.
    ///
    /// Nota de migración: el SP legado <c>SP_OBTENER_LIMITES_INVENTARIO</c> NO pagina ni recibe
    /// búsqueda. Como no se dispone del esquema para crear un <c>SP_V2</c> paginado, se reúsa el
    /// SP tal cual (con su multi-mapping de estatus, como el legado) y la página/búsqueda se
    /// resuelven en memoria (último recurso permitido por la regla 10 del front). El contrato
    /// paginado (links/meta) se conserva: el controller arma la respuesta con IPaginationBuilder.
    /// TODO: cuando se tenga el esquema, sustituir por SP_V2_CONSULTA_LIMITES_INVENTARIO (OFFSET/FETCH).
    /// </summary>
    public sealed class LimitesInventarioRepository : BaseRepository, ILimitesInventarioRepository
    {
        public LimitesInventarioRepository(IDbConnectionFactory factory) : base(factory) { }

        public async Task<RawPage<LimiteInventario>> ListarAsync(LimitesInventarioQuery query)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", null);
            p.Add("@idAlmacen", query.IdAlmacen == 0 ? (int?)null : query.IdAlmacen);
            p.Add("@idEstatusLimiteInv", query.IdEstatusLimiteInv == 0 ? (int?)null : query.IdEstatusLimiteInv);
            p.Add("@idLineaProducto", query.IdLineaProducto == 0 ? (int?)null : query.IdLineaProducto);

            using IDbConnection db = CreateConnection();
            using var multi = await db.QueryMultipleAsync(
                "SP_OBTENER_LIMITES_INVENTARIO", p, commandType: CommandType.StoredProcedure);

            var cabecera = await multi.ReadFirstAsync();
            if ((int)cabecera.status != 200)
                return RawPage<LimiteInventario>.Empty();

            // Multi-mapping estatus anidado, igual que el DAO legado (splitOn idEstatusLimiteInventario).
            // GridReader solo expone el multi-mapping en su versión síncrona; los datos ya están leídos.
            var todos = multi.Read<LimiteInventario, Status, LimiteInventario>(
                (limite, status) =>
                {
                    limite.EstatusInventario = status;
                    return limite;
                },
                splitOn: "idEstatusLimiteInventario").ToList();

            foreach (var l in todos)
                l.CantidadSugerida = l.Maximo - l.CantidadInventario;

            // Búsqueda libre en memoria (el SP legado no recibe @search).
            var search = query.Q?.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                todos = todos.Where(l =>
                    Contiene(l.Descripcion, search) ||
                    Contiene(l.CodigoBarras, search) ||
                    Contiene(l.DescripcionAlmacen, search) ||
                    Contiene(l.DescripcionLineaProducto, search)).ToList();
            }

            int total = todos.Count;
            int page = query.Page < 1 ? 1 : query.Page;
            int perPage = query.PerPage < 1 ? 10 : query.PerPage;
            var items = todos.Skip((page - 1) * perPage).Take(perPage).ToList();

            return new RawPage<LimiteInventario> { Items = items, Total = total };
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
        {
            // El SP de estatus devuelve UN resultset (idStatus, descripcion) sin cabecera, igual
            // que el legado (db.Query<Status>). Se proyecta a CatalogoItem para el combo.
            using IDbConnection db = CreateConnection();
            var filas = (await db.QueryAsync<Status>(
                "SP_OBTENER_ESTATUS_LIMITES_INVENTARIO",
                commandType: CommandType.StoredProcedure)).ToList();

            return new Notificacion<IEnumerable<CatalogoItem>>
            {
                Estatus = 200,
                Mensaje = "OK",
                Modelo = filas.Select(s => new CatalogoItem { Id = s.IdStatus, Descripcion = s.Descripcion }).ToList()
            };
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenesAsync(int? idSucursal, int? idTipoAlmacen)
        {
            var p = new DynamicParameters();
            p.Add("@idSucursal", idSucursal);
            p.Add("@idTipoAlmacen", idTipoAlmacen);
            return ConsultarCatalogoAsync("SP_CONSULTA_ALMACENES", p, "idAlmacen");
        }

        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync()
        {
            var p = new DynamicParameters();
            p.Add("@tipo", "lineas");
            return ConsultarAsync<CatalogoItem>("SP_V2_CONSULTA_CATALOGOS_PRODUCTO", p);
        }

        public Task<Notificacion<string>> GuardarAsync(GuardarLimiteRequest request, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@idProducto", request.IdProducto);
            p.Add("@idAlmacen", request.IdAlmacen);
            p.Add("@idUsuario", idUsuario);
            p.Add("@minimo", request.Minimo);
            p.Add("@maximo", request.Maximo);
            return EjecutarLegadoAsync("SP_INSERTA_ACTUALIZA_LIMITE_INVENTARIO", p);
        }

        public Task<Notificacion<string>> GuardarMasivoAsync(IEnumerable<LimiteMasivoItem> limites, int idUsuario)
        {
            var p = new DynamicParameters();
            p.Add("@xmlLimitesInventario", SerializarXml(limites));
            p.Add("@idUsuario", idUsuario);
            return EjecutarLegadoAsync("SP_INSERTA_ACTUALIZA_LIMITES_INVENTARIO_MASIVO", p);
        }

        // --- helpers -----------------------------------------------------------------

        private static bool Contiene(string? valor, string termino)
            => valor?.Contains(termino, StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// Ejecuta un SP de upsert legado que devuelve una fila con columnas Estatus/Mensaje
        /// (PascalCase, distinto de los SP de listado que usan status/mensaje). Tolera ambas
        /// grafías leyendo la fila como diccionario case-insensitive.
        /// </summary>
        private async Task<Notificacion<string>> EjecutarLegadoAsync(string storedProcedure, DynamicParameters parametros)
        {
            using IDbConnection db = CreateConnection();
            var fila = await db.QueryFirstOrDefaultAsync(
                storedProcedure, parametros, commandType: CommandType.StoredProcedure);

            if (fila is null)
                return new Notificacion<string> { Estatus = 500, Mensaje = "El SP no devolvió respuesta." };

            var row = new Dictionary<string, object>(
                (IDictionary<string, object>)fila, StringComparer.OrdinalIgnoreCase);

            int estatus = row.TryGetValue("estatus", out var e) && e is not null
                ? Convert.ToInt32(e)
                : row.TryGetValue("status", out var s) && s is not null ? Convert.ToInt32(s) : 0;

            string? mensaje = row.TryGetValue("mensaje", out var m) ? m?.ToString() : null;

            return new Notificacion<string> { Estatus = estatus, Mensaje = mensaje };
        }

        /// <summary>
        /// Serializa la lista a XML con la misma forma que el DAO legado
        /// (root ArrayOfLimiteInvetario, ítems LimiteInvetario, elementos camelCase) para que el
        /// SP masivo (que parsea esos nodos) lo entienda sin cambios.
        /// </summary>
        private static string SerializarXml(IEnumerable<LimiteMasivoItem> limites)
        {
            var lista = limites.Select(i => new LimiteInvetarioXml
            {
                CodigoBarras = i.CodigoBarras ?? string.Empty,
                DescripcionAlmacen = i.DescripcionAlmacen ?? string.Empty,
                Minimo = i.Minimo,
                Maximo = i.Maximo
            }).ToList();

            var serializer = new XmlSerializer(typeof(List<LimiteInvetarioXml>),
                new XmlRootAttribute("ArrayOfLimiteInvetario"));

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
            {
                serializer.Serialize(writer, lista);
            }
            return sb.ToString();
        }

        /// <summary>
        /// DTO de serialización del masivo. Reproduce los nombres del modelo legado
        /// (incluida la errata "Invetario") para que el XML sea idéntico al que esperaba el SP.
        /// </summary>
        [XmlType("LimiteInvetario")]
        public sealed class LimiteInvetarioXml
        {
            [XmlElement("idProducto")] public int IdProducto { get; set; }
            [XmlElement("idAlmacen")] public int IdAlmacen { get; set; }
            [XmlElement("idLimiteInventario")] public int IdLimiteInventario { get; set; }
            [XmlElement("idLineaProducto")] public int IdLineaProducto { get; set; }
            [XmlElement("minimo")] public int Minimo { get; set; }
            [XmlElement("maximo")] public int Maximo { get; set; }
            [XmlElement("descripcionAlmacen")] public string DescripcionAlmacen { get; set; } = string.Empty;
            [XmlElement("codigoBarras")] public string CodigoBarras { get; set; } = string.Empty;
            [XmlElement("cantidadInventario")] public int CantidadInventario { get; set; }
        }
    }
}
