namespace comercializadora_api.Models.Entities
{
    /// <summary>
    /// Estación (caja/punto de venta) del sistema. Mapea el resultset de SP_CONSULTA_ESTACIONES.
    /// Migra el modelo Estacion del backend legado (solo los campos del CRUD; los montos de
    /// ventas del dashboard viven en <see cref="EstacionVenta"/>).
    /// </summary>
    public class Estacion
    {
        public int IdEstacion { get; set; }
        public int IdAlmacen { get; set; }
        public string? NombreAlmacen { get; set; }
        public string? MacAdress { get; set; }
        public string? Nombre { get; set; }
        public int Numero { get; set; }
        public bool Configurado { get; set; }
        public int IdUsuario { get; set; }
        public int IdStatus { get; set; }
        public int IdSucursal { get; set; }
    }
}
