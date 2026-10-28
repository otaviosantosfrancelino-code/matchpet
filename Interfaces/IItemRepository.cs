using crudcomdb.Models;

namespace crudcomdb.Interfaces
{
    public interface IItemRepository
    {
        Task<List<ItemDoacao>> GetDisponiveisAsync();           // vitrine do Bazar (leve)
        Task<List<ItemDoacao>> GetByUserIdAsync(int userId);    // "Meus anúncios" (leve)
        Task<ItemDoacao?> GetParaEdicaoAsync(int id);           // rastreado para salvar
        Task<int> ContarDisponiveisAsync();
        Task AddAsync(ItemDoacao item);
        Task UpdateAsync(ItemDoacao item);
        Task AtualizarStatusAsync(int id, string status);
        Task DeleteAsync(int id);
    }
}
