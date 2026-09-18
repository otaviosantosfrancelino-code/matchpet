using crudcomdb.Data;
using crudcomdb.Interfaces;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Repositories
{
    public class InteresseRepository : IInteresseRepository
    {
        private readonly AppDbContext _context;
        public InteresseRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(InteresseAdocao interesse)
        {
            await _context.Interesses.AddAsync(interesse);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<InteresseAdocao>> GetAllAsync()
        {
            return await _context.Interesses.ToListAsync();
        }
    }
}