using comercializadora_api.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace comercializadora_api.UnitofWork
{
    public class UnitofWork : IUnitofWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public UnitofWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public DbContext Context => _context;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
