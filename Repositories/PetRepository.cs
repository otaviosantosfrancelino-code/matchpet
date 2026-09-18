using crudcomdb.Data;
using crudcomdb.Interfaces;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _context;
        public PetRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Pet>> GetAllAsync() => 
            await _context.Pets.Include(p => p.Imagens).Include(p => p.UsuarioDoador).ToListAsync();

        public async Task<Pet?> GetByIdAsync(int id) => 
            await _context.Pets.Include(p => p.Imagens).Include(p => p.UsuarioDoador).FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(Pet pet)
        {
            await _context.Pets.AddAsync(pet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var pet = await GetByIdAsync(id);
            if (pet != null)
            {
                _context.Pets.Remove(pet);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Pet>> GetPetsParaMatch(bool temCriancas, bool moraEmApartamento, string? especie, string? porte, string? sexo, string? cidade)
        {
            var query = _context.Pets.Include(p => p.Imagens).Include(p => p.UsuarioDoador).AsQueryable();

            if (temCriancas) query = query.Where(p => p.RecomendadoParaCriancas);
            if (moraEmApartamento) query = query.Where(p => !p.PrecisaDeQuintal);
            if (!string.IsNullOrEmpty(especie)) query = query.Where(p => p.Especie == especie);
            if (!string.IsNullOrEmpty(porte)) query = query.Where(p => p.Porte == porte);
            if (!string.IsNullOrEmpty(sexo)) query = query.Where(p => p.Sexo == sexo);
            if (!string.IsNullOrEmpty(cidade)) 
                query = query.Where(p => p.UsuarioDoador != null && p.UsuarioDoador.Cidade.ToLower().Contains(cidade.ToLower()));

            return await query.ToListAsync();
        }

        // --- MÉTODOS FASE 3 ---
        public async Task<IEnumerable<Pet>> GetByUserIdAsync(int userId) =>
            await _context.Pets.Include(p => p.Imagens).Where(p => p.UsuarioDoadorId == userId).ToListAsync();

        public async Task<IEnumerable<Pet>> GetNgoPetsAsync() =>
            await _context.Pets.Include(p => p.Imagens).Include(p => p.UsuarioDoador)
                .Where(p => p.UsuarioDoador != null && p.UsuarioDoador.EhOng && p.Status == "Disponível").ToListAsync();

        public async Task<IEnumerable<Pet>> GetAdoptedPetsAsync() =>
            await _context.Pets.Include(p => p.Imagens).Where(p => p.Status == "Adotado").ToListAsync();
    }
}