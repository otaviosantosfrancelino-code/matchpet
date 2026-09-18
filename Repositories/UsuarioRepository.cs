using crudcomdb.Data;
using crudcomdb.Interfaces;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;
        public UsuarioRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Usuario>> GetAllAsync() => await _context.Usuarios.ToListAsync();

        public async Task<Usuario?> GetByEmailAsync(string email) => 
            await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<Usuario?> GetByDocumentoAsync(string documento) => 
            await _context.Usuarios.FirstOrDefaultAsync(u => u.Documento == documento); // NOVO

        public async Task<Usuario?> GetByIdAsync(int id) => 
            await _context.Usuarios.FindAsync(id);

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}