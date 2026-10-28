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

        // IMPORTANTE (desempenho): nunca usar Include(UsuarioDoador) em listagens.
        // O usuário guarda a foto do documento (vários MB) e isso deixava o catálogo ~20s carregando.
        private static IQueryable<Pet> Leve(IQueryable<Pet> query) => query
            .AsNoTracking()
            .Select(p => new Pet
            {
                Id = p.Id,
                Nome = p.Nome,
                Especie = p.Especie,
                Raca = p.Raca,
                Sexo = p.Sexo,
                Porte = p.Porte,
                Idade = p.Idade,
                Descricao = p.Descricao,
                Castrado = p.Castrado,
                PrecisaDeQuintal = p.PrecisaDeQuintal,
                RecomendadoParaCriancas = p.RecomendadoParaCriancas,
                Status = p.Status,
                UsuarioDoadorId = p.UsuarioDoadorId,
                Imagens = p.Imagens.OrderBy(i => i.Id).Take(1).ToList(),
                UsuarioDoador = new Usuario
                {
                    Id = p.UsuarioDoador!.Id,
                    Nome = p.UsuarioDoador.Nome,
                    Cidade = p.UsuarioDoador.Cidade,
                    Estado = p.UsuarioDoador.Estado,
                    EhOng = p.UsuarioDoador.EhOng
                }
            });

        public async Task<IEnumerable<Pet>> GetAllAsync() =>
            await Leve(_context.Pets.OrderByDescending(p => p.Id)).ToListAsync();

        public async Task<IEnumerable<Pet>> GetPetsParaMatch(bool temCriancas, bool moraEmApartamento, string? especie, string? porte, string? sexo, string? cidade)
        {
            var query = _context.Pets.AsQueryable();

            if (temCriancas) query = query.Where(p => p.RecomendadoParaCriancas);
            if (moraEmApartamento) query = query.Where(p => !p.PrecisaDeQuintal);
            if (!string.IsNullOrEmpty(especie))
            {
                // "Outros" = qualquer espécie fora das principais
                query = especie == "Outros"
                    ? query.Where(p => p.Especie != "Cão" && p.Especie != "Gato" && p.Especie != "Pássaro" && p.Especie != "Peixe")
                    : query.Where(p => p.Especie == especie);
            }
            if (!string.IsNullOrEmpty(porte)) query = query.Where(p => p.Porte == porte);
            if (!string.IsNullOrEmpty(sexo)) query = query.Where(p => p.Sexo == sexo);
            if (!string.IsNullOrWhiteSpace(cidade))
            {
                var c = cidade.Trim().ToLower();
                query = query.Where(p => p.UsuarioDoador != null && p.UsuarioDoador.Cidade.ToLower().Contains(c));
            }

            return await Leve(query.OrderByDescending(p => p.Id)).ToListAsync();
        }

        public async Task<IEnumerable<Pet>> GetByUserIdAsync(int userId) =>
            await Leve(_context.Pets.Where(p => p.UsuarioDoadorId == userId).OrderByDescending(p => p.Id)).ToListAsync();

        public async Task<IEnumerable<Pet>> GetNgoPetsAsync(int? ongId = null)
        {
            var query = _context.Pets.Where(p => p.UsuarioDoador != null && p.UsuarioDoador.EhOng && p.Status == "Disponível");
            if (ongId != null) query = query.Where(p => p.UsuarioDoadorId == ongId);
            return await Leve(query.OrderByDescending(p => p.Id)).ToListAsync();
        }

        public async Task<IEnumerable<Pet>> GetAdoptedPetsAsync() =>
            await Leve(_context.Pets.Where(p => p.Status == "Adotado").OrderByDescending(p => p.Id)).ToListAsync();

        public async Task<IEnumerable<Pet>> GetRecentesAsync(int quantidade) =>
            await Leve(_context.Pets.Where(p => p.Status == "Disponível").OrderByDescending(p => p.Id).Take(quantidade)).ToListAsync();

        public async Task<(int disponiveis, int adotados)> GetEstatisticasAsync()
        {
            var porStatus = await _context.Pets.AsNoTracking()
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Total = g.Count() })
                .ToListAsync();
            return (porStatus.FirstOrDefault(s => s.Status == "Disponível")?.Total ?? 0,
                    porStatus.FirstOrDefault(s => s.Status == "Adotado")?.Total ?? 0);
        }

        public async Task<Pet?> GetByIdAsync(int id) => await _context.Pets
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new Pet
            {
                Id = p.Id,
                Nome = p.Nome,
                Especie = p.Especie,
                Raca = p.Raca,
                Sexo = p.Sexo,
                Porte = p.Porte,
                Idade = p.Idade,
                Descricao = p.Descricao,
                Castrado = p.Castrado,
                PrecisaDeQuintal = p.PrecisaDeQuintal,
                RecomendadoParaCriancas = p.RecomendadoParaCriancas,
                Status = p.Status,
                UsuarioDoadorId = p.UsuarioDoadorId,
                Imagens = p.Imagens.OrderBy(i => i.Id).ToList(),
                UsuarioDoador = new Usuario
                {
                    Id = p.UsuarioDoador!.Id,
                    Nome = p.UsuarioDoador.Nome,
                    Documento = p.UsuarioDoador.Documento,   // usado no contrato
                    Telefone = p.UsuarioDoador.Telefone,     // liberado após assinar o termo
                    Cidade = p.UsuarioDoador.Cidade,
                    Estado = p.UsuarioDoador.Estado,
                    EhOng = p.UsuarioDoador.EhOng,
                    FotoPerfilBase64 = p.UsuarioDoador.FotoPerfilBase64
                }
            })
            .FirstOrDefaultAsync();

        public async Task<Pet?> GetParaEdicaoAsync(int id) =>
            await _context.Pets.Include(p => p.Imagens).FirstOrDefaultAsync(p => p.Id == id);

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

        // Muda só o status direto no banco (sem regravar fotos nem o doador)
        public async Task AtualizarStatusAsync(int id, string status) =>
            await _context.Pets.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, status));

        public async Task DeleteAsync(int id)
        {
            // Remove os registros que dependem do pet e depois o pet
            await _context.Interesses.Where(i => i.PetId == id).ExecuteDeleteAsync();
            await _context.PetImagens.Where(i => i.PetId == id).ExecuteDeleteAsync();
            await _context.Pets.Where(p => p.Id == id).ExecuteDeleteAsync();
        }
    }
}
