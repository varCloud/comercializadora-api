using comercializadora_api.Data;
using Microsoft.EntityFrameworkCore;

namespace comercializadora_api.UnitofWork
{
    public interface IUnitofWork : IDisposable
    {
         DbContext Context { get; }
        public Task SaveChangesAsync();
    }
}
