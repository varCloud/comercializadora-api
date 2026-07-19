using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repositories.Base;
using Dapper;

namespace comercializadora_api.Repositories.ReportesVentas
{
    /// <summary>
    /// Implementación de <see cref="IVentaReporteRepository"/> (Repository + Dapper + Stored
    /// Procedure). Migra <c>ReportesDAO.ObtenerVentas</c> del legado, reutilizando
    /// <c>SP_CONSULTA_VENTAS</c> sin cambios (decisión cerrada de la HU: no hay cambio de
    /// comportamiento que justifique un <c>SP_V2_*</c>).
    ///
    /// Nota de migración (corrección post-aprobación 2026-07-19): <c>SP_CONSULTA_VENTAS</c> NO
    /// pagina (no soporta OFFSET/FETCH) y un rango de fechas amplio puede devolver miles de
    /// filas. Como no se modifica el SP (decisión cerrada), se trae el resultset completo, se
    /// mapea y la página se resuelve en memoria (mismo último recurso ya usado en
    /// <c>LimitesInventarioRepository.ListarAsync</c>). El contrato paginado (links/meta) lo
    /// arma el controller con <c>IPaginationBuilder</c>.
    /// </summary>
    public sealed class VentaReporteRepository : BaseRepository, IVentaReporteRepository
    {
        private const string StoredProcedure = "SP_CONSULTA_VENTAS";

        public VentaReporteRepository(IDbConnectionFactory factory) : base(factory) { }

        public async Task<RawPage<VentaReporteItem>> ListarAsync(VentaReporteQuery filtros)
        {
            var todos = await ObtenerTodosAsync(filtros);
            if (!todos.EsExitoso)
                return RawPage<VentaReporteItem>.Empty();

            var lista = todos.Modelo ?? new List<VentaReporteItem>();
            int total = lista.Count;
            int page = filtros.Page < 1 ? 1 : filtros.Page;
            int perPage = filtros.PerPage < 1 ? 10 : filtros.PerPage;
            var items = lista.Skip((page - 1) * perPage).Take(perPage).ToList();

            return new RawPage<VentaReporteItem> { Items = items, Total = total };
        }

        /// <summary>
        /// Usado por <c>/exportar</c>: TODAS las filas que cumplen los filtros, sin paginar (a
        /// diferencia de <see cref="ListarAsync"/>, que sí pagina en memoria para el listado de
        /// pantalla). Comparte la consulta/mapeo vía <see cref="ObtenerTodosAsync"/> para no
        /// duplicar la llamada al SP; conserva <c>Estatus</c>/<c>Mensaje</c> del SP (incluido el
        /// caso "No se encontraron ventas con esos términos de búsqueda.") para que el controller
        /// siga devolviendo el mismo <c>400</c> explícito de antes de esta corrección.
        /// </summary>
        public async Task<Notificacion<IEnumerable<VentaReporteItem>>> ExportarAsync(VentaReporteQuery filtros)
        {
            var todos = await ObtenerTodosAsync(filtros);
            return new Notificacion<IEnumerable<VentaReporteItem>>
            {
                Estatus = todos.Estatus,
                Mensaje = todos.Mensaje,
                Modelo = todos.Modelo
            };
        }

        /// <summary>Llama al SP con los filtros de pantalla y mapea el resultset completo (sin paginar).</summary>
        private async Task<Notificacion<List<VentaReporteItem>>> ObtenerTodosAsync(VentaReporteQuery filtros)
        {
            var p = new DynamicParameters();

            // idProducto/descProducto: no expuestos en el Front (la pantalla legada tampoco los
            // exponía como filtro de "Reportes > Ventas"; 0/null son equivalentes para el SP).
            p.Add("@idProducto", (object?)null);
            p.Add("@descProducto", (object?)null);
            p.Add("@idLineaProducto", filtros.IdLineaProducto);
            p.Add("@idCliente", filtros.IdCliente);
            p.Add("@idUsuario", filtros.IdUsuario);

            // ReportesDAO.ObtenerVentas: fecha no provista (DateTime.MinValue en el legado) -> 1900-01-01.
            p.Add("@fechaIni", filtros.FechaIni ?? new DateTime(1900, 1, 1));
            p.Add("@fechaFin", filtros.FechaFin ?? new DateTime(1900, 1, 1));

            // tipoConsulta: 1 = "reportes" (trae precio/costo/ganancia/montoTotal calculados por
            // fila), 2 = "módulo de ventas" (resultset distinto, agrupado). El legado enviaba 0
            // por default (campo sin inicializar), que el SP también resuelve como "reportes"
            // (solo 2 tiene rama propia); se envía 1 explícito aquí por claridad de intención.
            p.Add("@tipoConsulta", 1);

            // idStatusVenta: 0 -> 1 en el legado (ventas activas); no se expone filtro en el Front.
            p.Add("@idStatusVenta", 1);

            // idFactFormaPago/idAlmacen: no expuestos en el Front, sin filtro (null = todos).
            p.Add("@idFactFormaPago", (object?)null);
            p.Add("@idAlmacen", (object?)null);

            var crudo = await ConsultarAsync<VentaReporteRow>(StoredProcedure, p);

            var notificacion = new Notificacion<List<VentaReporteItem>>
            {
                Estatus = crudo.Estatus,
                Mensaje = crudo.Mensaje,
            };

            if (crudo.EsExitoso)
                notificacion.Modelo = (crudo.Modelo ?? Enumerable.Empty<VentaReporteRow>())
                    .Select(MapearItem)
                    .ToList();

            return notificacion;
        }

        /// <summary>
        /// Utilidad/MargenBruto replican el cálculo que hacía <c>_Ventas.cshtml</c> en el legado
        /// (por fila, en la vista, no en el SP): gananciaTotal = ganancia unitaria * cantidad;
        /// margenBruto = 0 si gananciaTotal es 0, si no (gananciaTotal/precioTotal)*100 redondeado
        /// a 2 decimales. A diferencia del legado (float, sin guarda) también se protege
        /// precioTotal == 0 para no dividir entre cero (con decimal eso lanza excepción en vez de
        /// devolver Infinity/NaN como hacía el float del legado).
        /// </summary>
        private static VentaReporteItem MapearItem(VentaReporteRow r)
        {
            var precioTotal = r.Precio * (decimal)r.Cantidad;
            var gananciaTotal = r.Ganancia * (decimal)r.Cantidad;
            var margenBruto = (gananciaTotal == 0 || precioTotal == 0)
                ? 0
                : Math.Round((gananciaTotal / precioTotal) * 100, 2);

            return new VentaReporteItem
            {
                Fecha = r.FechaAlta,
                Sucursal = r.DescSucursal,
                Tienda = r.DescripcionAlmacen,
                Cajero = r.NombreUsuario,
                Folio = r.IdVenta,
                Cliente = r.NombreCliente,
                CodigoBarras = r.CodigoBarras,
                LineaProducto = r.DescripcionLineaProducto,
                Producto = r.DescripcionProducto,
                Cantidad = r.Cantidad,
                PrecioVenta = r.Precio,
                Iva = r.MontoIva,
                MontoTotal = r.MontoTotal,
                CostoCompra = r.Costo,
                Utilidad = gananciaTotal,
                MargenBruto = margenBruto,
                FormaPago = r.DescripcionFactFormaPago,
            };
        }

        /// <summary>
        /// Fila cruda del resultset de <c>SP_CONSULTA_VENTAS</c> en modo "reportes"
        /// (<c>@tipoConsulta = 1</c>): nombres de propiedad iguales a las columnas que devuelve el
        /// SP para que Dapper las mapee automáticamente (no se puede alterar el SP para alias-earlas
        /// al contrato del Front). Se proyecta a <see cref="VentaReporteItem"/> en
        /// <see cref="MapearItem"/>, que sí usa los nombres del contrato expuesto.
        /// </summary>
        private sealed class VentaReporteRow
        {
            public int IdVenta { get; set; }
            public string? NombreCliente { get; set; }
            public double Cantidad { get; set; }
            public DateTime FechaAlta { get; set; }
            public string? NombreUsuario { get; set; }
            public string? DescripcionProducto { get; set; }
            public string? DescripcionLineaProducto { get; set; }
            public decimal Precio { get; set; }
            public decimal Costo { get; set; }
            public decimal Ganancia { get; set; }
            public string? CodigoBarras { get; set; }
            public string? DescSucursal { get; set; }
            public string? DescripcionAlmacen { get; set; }
            public decimal MontoTotal { get; set; }
            public decimal MontoIva { get; set; }
            public string? DescripcionFactFormaPago { get; set; }
        }
    }
}
