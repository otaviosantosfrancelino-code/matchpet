using crudcomdb.Data;
using crudcomdb.Interfaces;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;
        public ItemRepository(AppDbContext context) => _context = context;

        // Só a 1ª foto e dados públicos do doador (o telefone é usado no botão do WhatsApp)
        private static IQueryable<ItemDoacao> Leve(IQueryable<ItemDoacao> query) => query
            .AsNoTracking()
            .Select(i => new ItemDoacao
            {
                Id = i.Id,
                Nome = i.Nome,
                EspecieDestino = i.EspecieDestino,
                Categoria = i.Categoria,
                Descricao = i.Descricao,
                Status = i.Status,
                UsuarioDoadorId = i.UsuarioDoadorId,
                DataCadastro = i.DataCadastro,
                Imagens = i.Imagens.OrderBy(img => img.Id).Take(1).ToList(),
                UsuarioDoador = new Usuario
                {
                    Id = i.UsuarioDoador!.Id,
                    Nome = i.UsuarioDoador.Nome,
                    Cidade = i.UsuarioDoador.Cidade,
                    Estado = i.UsuarioDoador.Estado,
                    Telefone = i.UsuarioDoador.Telefone,
                    EhOng = i.UsuarioDoador.EhOng
                }
            });

        public async Task<List<ItemDoacao>> GetDisponiveisAsync() =>
            await Leve(_context.ItensDoacao.Where(i => i.Status == "Disponível").OrderByDescending(i => i.Id)).ToListAsync();

        public async Task<List<ItemDoacao>> GetByUserIdAsync(int userId) =>
            await Leve(_context.ItensDoacao.Where(i => i.UsuarioDoadorId == userId).OrderByDescending(i => i.Id)).ToListAsync();

        public async Task<ItemDoacao?> GetParaEdicaoAsync(int id) =>
            await _context.ItensDoacao.Include(i => i.Imagens).FirstOrDefaultAsync(i => i.Id == id);

        public async Task<int> ContarDisponiveisAsync() =>
            await _context.ItensDoacao.CountAsync(i => i.Status == "Disponível");

        public async Task AddAsync(ItemDoacao item)
        {
            await _context.ItensDoacao.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemDoacao item)
        {
            _context.ItensDoacao.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task AtualizarStatusAsync(int id, string status) =>
            await _context.ItensDoacao.Where(i => i.Id == id).ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, status));

        public async Task DeleteAsync(int id)
        {
            await _context.ItemImagens.Where(i => i.ItemDoacaoId == id).ExecuteDeleteAsync();
            await _context.ItensDoacao.Where(i => i.Id == id).ExecuteDeleteAsync();
        }
    }
}
