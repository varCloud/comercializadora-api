using comercializadora_api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace comercializadora_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options ) : base(options)
        {            
        }

        //AQUI SE DEBE DE AGREGAR TODAS LAS  CLASES/TABLAS PARA QUE PUEDAN SER ACCEDIDAS DESDE EL DB CONTEXT
        public DbSet<Usuarios> Usuarios { get; set; }         
    }
}
