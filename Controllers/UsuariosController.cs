using comercializadora_api.Models.Common;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Pagination;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    /// <summary>
    /// Administración de usuarios del sistema. Migra UsuariosController + UsuarioDAO del legado,
    /// corrigiendo los verbos HTTP y agregando paginación server-side. Requiere JWT válido.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;
        private readonly IPaginationBuilder _pagination;

        public UsuariosController(IUsuariosService usuariosService, IPaginationBuilder pagination)
        {
            _usuariosService = usuariosService;
            _pagination = pagination;
        }

        /// <summary>Listado paginado de usuarios. Query: page, perPage, q, order, sort, idRol, idAlmacen.</summary>
        [HttpGet]
        public async Task<Notificacion<IEnumerable<Usuario>>> Listar([FromQuery] UsuariosQuery query)
        {
            var page = await _usuariosService.ListarAsync(query);
            return _pagination.Build(page, query, Request);
        }

        /// <summary>Obtiene un usuario por id (para precargar el formulario de edición).</summary>
        [HttpGet("{id:int}")]
        public Task<Notificacion<Usuario>> ObtenerPorId(int id)
            => _usuariosService.ObtenerPorIdAsync(id);

        /// <summary>Alta de usuario.</summary>
        [HttpPost]
        public Task<Notificacion<string>> Crear([FromBody] GuardarUsuarioRequest request)
        {
            request.IdUsuario = 0;
            return _usuariosService.GuardarAsync(request);
        }

        /// <summary>Edición de usuario.</summary>
        [HttpPut("{id:int}")]
        public Task<Notificacion<string>> Actualizar(int id, [FromBody] GuardarUsuarioRequest request)
        {
            request.IdUsuario = id;
            return _usuariosService.GuardarAsync(request);
        }

        /// <summary>Activa/desactiva un usuario (borrado lógico).</summary>
        [HttpPatch("{id:int}/estatus")]
        public Task<Notificacion<string>> CambiarEstatus(int id, [FromBody] CambiarEstatusRequest request)
            => _usuariosService.CambiarEstatusAsync(id, request.Activo);

        /// <summary>Catálogo de roles (para el combo del formulario).</summary>
        [HttpGet("catalogos/roles")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerRoles()
            => _usuariosService.ObtenerRolesAsync();

        /// <summary>Catálogo de sucursales.</summary>
        [HttpGet("catalogos/sucursales")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerSucursales()
            => _usuariosService.ObtenerSucursalesAsync();

        /// <summary>Catálogo de almacenes, opcionalmente filtrado por sucursal/tipo.</summary>
        [HttpGet("catalogos/almacenes")]
        public Task<Notificacion<IEnumerable<CatalogoItem>>> ObtenerAlmacenes(
            [FromQuery] int? idSucursal = null,
            [FromQuery] int? idTipoAlmacen = null)
            => _usuariosService.ObtenerAlmacenesAsync(idSucursal, idTipoAlmacen);
    }
}
