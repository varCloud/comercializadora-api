using comercializadora_api.Models.Dtos;
using comercializadora_api.Models.Entities;
using comercializadora_api.Models.Models;
using comercializadora_api.Repository;
using comercializadora_api.UnitofWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace comercializadora_api.Services
{
    public class UsuariosService
    {

        private readonly IUnitofWork _unitOfWork;
        UsuariosRepository usuariosRepository;

        public UsuariosService(UsuariosRepository usuariosRepository)
        {
            this.usuariosRepository = usuariosRepository;
        }

        public async Task<ActionResult<IEnumerable<Usuarios>>> ObtenerUsuariosActivos()
        {
            var usuarios = await this.usuariosRepository.GetAsync(us=> us.activo);
            return usuarios;
        }

        public async Task<Paginador<Usuarios>> ObtenerUsuariosPaginados(PaginadorRequestDto request)
        {
            var usuarios = await this.usuariosRepository.GetFieldsFiltered(request);
            return usuarios;
        }

        public async Task<ActionResult<Usuarios>> ObtenerUsuario(int id)
        {
            var usuarios = await this.usuariosRepository.GetAsync(us=> us.idUsuario == id);
            return usuarios.Value.First();
            
        }

    }
}
