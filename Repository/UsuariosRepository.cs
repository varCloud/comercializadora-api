using comercializadora_api.Models.Entities;
using comercializadora_api.UnitofWork;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace comercializadora_api.Repository
{
    public class UsuariosRepository : RepositoryBase<Usuarios>
    {
        protected DbSet<Usuarios> dbSet;
        
        public UsuariosRepository(IUnitofWork unitOfwork) : base(unitOfwork)
        {
            //EN CASO DE QUE SE NECESIRA UNA QUERY  QUE NO SE TIENE EL REPOSITORIO BASE
            //PODRIAMOS USAR ESTE DEBE SET.
            dbSet = unitOfwork.Context.Set<Usuarios>();
        }
    }
}
