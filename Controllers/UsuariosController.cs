using comercializadora_api.Data;
using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Repository;
using comercializadora_api.Services;
using comercializadora_api.UnitofWork;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace comercializadora_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosService usuariosService;
        public UsuariosController(UsuariosService usuariosService)
        {
           this.usuariosService = usuariosService;
           
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuarios>>> usuarios([FromQuery] PaginadorRequestDto queryParams)
        {
            var _usuarios = await this.usuariosService.ObtenerUsuariosPaginados(queryParams);
            return Ok(_usuarios);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Usuarios>> usuarios(int id)
        {
            var _usuario = await this.usuariosService.ObtenerUsuario(id);
            if (_usuario == null || _usuario.Value.activo == false)
            {
                return NotFound(id);
            }
            return Ok(_usuario.Value);
        }

        //[HttpPost]
        //public IActionResult usuarios(UsuariosDto usuarioDto)
        //{
        //    var usuario = new Usuarios()
        //    {
        //        nombre = usuarioDto.nombre,
        //        apellidoMaterno = usuarioDto.apellidoMaterno

        //    };

        //    dbContext.Usuarios.Add(usuario);
        //    dbContext.SaveChanges();
        //    return Ok(usuario);
        //}

        //[HttpPut]
        //[Route("{id:int}")]
        //public IActionResult usuarios(int id, UsuariosDto usuarioDto)
        //{
        //    var _usuario = dbContext.Usuarios.Find(id);
        //    if (_usuario == null)
        //    {
        //        return NotFound(id);
        //    }
        //    _usuario.nombre = usuarioDto.nombre;
        //    dbContext.SaveChanges();
        //    return Ok(_usuario);
        //}

        //[HttpDelete]
        //[Route("{id:int}")]
        //public IActionResult EliminarUsuarios(int id)
        //{
        //    var _usuario = dbContext.Usuarios.Find(id);
        //    if (_usuario == null)
        //    {
        //        return NotFound(id);
        //    }
        //    _usuario.activo = false;
        //    dbContext.SaveChanges();
        //    return Ok(_usuario);
        //}
    }
}
