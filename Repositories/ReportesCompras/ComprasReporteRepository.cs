using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;
using Dapper;

namespace comercializadora_api.Repositories.ReportesCompras
{
    /// <summary>
    /// Implementación de <see cref="IComprasReporteRepository"/> (Repository + Dapper + Stored
    /// Procedures). El listado/exportación usan <c>SP_V2_CONSULTA_COMPRAS_REPORTE</c> (paginado
    /// server-side; ver el .sql para por qué es un SP propio y no una extensión de
    /// <c>SP_V2_CONSULTA_COMPRAS</c>, ya usado por el módulo CRUD de Compras). Los catálogos
    /// reutilizan los SP paginados ya migrados de Proveedores/Líneas de producto/Usuarios (se
    /// pide una página "grande" para traerlos completos, no hay SP de catálogo dedicado para
    /// esas 3 entidades) y el SP de estatus ya usado por <c>ComprasRepository.ObtenerEstatusAsync</c>.
    /// </summary>
    public sealed class ComprasReporteRepository : BaseRepository, IComprasReporteRepository
    {
        private const string StoredProcedure = "SP_V2_CONSULTA_COMPRAS_REPORTE";

        // Tamaño de página usado para traer catálogos completos (Proveedores/Líneas/Usuarios) a
        // partir de sus SP paginados: no existe un SP de catálogo dedicado para esas 3 entidades,
        // así que se pide "una sola página" lo bastante grande para cubrir el universo real de
        // filas (equivalente a los combos ViewBag sin paginar del legado).
        private const int CatalogoPageSize = 10_000;

        public ComprasReporteRepository(IDbConnectionFactory factory) : base(factory) { }

        public Task<RawPage<CompraReporteItem>> ListarAsync(ComprasReporteQuery filtros)
            => ConsultarPaginaAsync<CompraReporteItem>(StoredProcedure, ArmarParametros(filtros, paraExportar: false));

        public async Task<Notificacion<IEnumerable<CompraReporteItem>>> ExportarAsync(ComprasReporteQuery filtros)
        {
            var pagina = await ConsultarPaginaAsync<CompraReporteItem>(
                StoredProcedure, ArmarParametros(filtros, paraExportar: true));

            return new Notificacion<IEnumerable<CompraReporteItem>>
            {
                Estatus = 200,
                Mensaje = "OK",
                Modelo = pagina.Items
            };
        }

        private static DynamicParameters ArmarParametros(ComprasReporteQuery filtros, bool paraExportar)
        {
            var p = new DynamicParameters();
            p.Add("@search", string.IsNullOrWhiteSpace(filtros.Q) ? null : filtros.Q.Trim());
            p.Add("@idProveedor", filtros.IdProveedor is null or 0 ? null : filtros.IdProveedor);
            p.Add("@idStatusCompra", filtros.IdStatusCompra is null or 0 ? null : filtros.IdStatusCompra);
            p.Add("@idUsuario", filtros.IdUsuario is null or 0 ? null : filtros.IdUsuario);
            p.Add("@idLineaProducto", filtros.IdLineaProducto is null or 0 ? null : filtros.IdLineaProducto);
            p.Add("@fechaInicio", filtros.FechaIni);
            p.Add("@fechaFin", filtros.FechaFin);
            p.Add("@order", filtros.Order);
            p.Add("@sort", filtros.Sort);

            // /exportar ignora la paginación de pantalla y trae el universo completo que cumple
            // los filtros (IExportacionService decide descarga inmediata vs. correo según el total).
            p.Add("@pageNumber", paraExportar ? 1 : filtros.Page);
            p.Add("@pageSize", paraExportar ? CatalogoPageSize : filtros.PerPage);
            return p;
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerProveedoresAsync()
        {
            var p = new DynamicParameters();
            p.Add("@search", (string?)null);
            p.Add("@order", (string?)null);
            p.Add("@sort", (string?)null);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", CatalogoPageSize);

            var pagina = await ConsultarPaginaAsync<Proveedor>("SP_V2_CONSULTA_PROVEEDORES", p);
            return EnvolverCatalogo(pagina.Items.Select(x => new CatalogoItem
            {
                Id = x.IdProveedor,
                Descripcion = x.Nombre
            }));
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerLineasAsync()
        {
            var p = new DynamicParameters();
            p.Add("@idLineaProducto", 0);
            p.Add("@search", (string?)null);
            p.Add("@order", (string?)null);
            p.Add("@sort", (string?)null);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", CatalogoPageSize);

            var pagina = await ConsultarPaginaAsync<LineaProducto>("SP_V2_CONSULTA_LINEAS_PRODUCTO", p);
            return EnvolverCatalogo(pagina.Items.Select(x => new CatalogoItem
            {
                Id = x.IdLineaProducto,
                Descripcion = x.Descripcion
            }));
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerCompradoresAsync()
        {
            var p = new DynamicParameters();
            p.Add("@idUsuario", 0);
            p.Add("@idAlmacen", (int?)null);
            p.Add("@idRol", (int?)null);
            p.Add("@search", (string?)null);
            p.Add("@pageNumber", 1);
            p.Add("@pageSize", CatalogoPageSize);

            var pagina = await ConsultarPaginaAsync<Usuario>("SP_V2_CONSULTA_USUARIOS", p);
            return EnvolverCatalogo(pagina.Items.Select(x => new CatalogoItem
            {
                Id = x.IdUsuario,
                Descripcion = x.NombreCompleto ?? x.NombreUsuario
            }));
        }

        public async Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerEstatusAsync()
        {
            // Mismo SP que ComprasRepository.ObtenerEstatusAsync (patrón cabecera + resultset,
            // no el patrón "catálogo legado en un solo resultset" de ConsultarCatalogoAsync).
            var estatus = await ConsultarAsync<EstatusCompra>("SP_CONSULTA_ESTATUS_COMPRA");
            if (!estatus.EsExitoso)
                return new Notificacion<IEnumerable<CatalogoItem>> { Estatus = estatus.Estatus, Mensaje = estatus.Mensaje };

            return EnvolverCatalogo((estatus.Modelo ?? Enumerable.Empty<EstatusCompra>())
                .Select(x => new CatalogoItem { Id = x.IdStatus, Descripcion = x.Descripcion }));
        }

        private static Notificacion<IEnumerable<CatalogoItem>> EnvolverCatalogo(IEnumerable<CatalogoItem> items)
            => new() { Estatus = 200, Mensaje = "OK", Modelo = items.ToList() };
    }
}
