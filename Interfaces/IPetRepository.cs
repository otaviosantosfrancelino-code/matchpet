using crudcomdb.Models;

namespace crudcomdb.Interfaces
{
    public interface IPetRepository
    {
        // Listagens "leves": só a 1ª foto e dados públicos do doador (carregam rápido)
        Task<IEnumerable<Pet>> GetAllAsync();
        Task<IEnumerable<Pet>> GetPetsParaMatch(bool temCriancas, bool moraEmApartamento, string? especie, string? porte, string? sexo, string? cidade);
        Task<IEnumerable<Pet>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Pet>> GetNgoPetsAsync(int? ongId = null);
        Task<IEnumerable<Pet>> GetAdoptedPetsAsync();
        Task<IEnumerable<Pet>> GetRecentesAsync(int quantidade);
        Task<(int disponiveis, int adotados)> GetEstatisticasAsync();

        // Página de detalhes: todas as fotos + dados do doador necessários ao contrato
        Task<Pet?> GetByIdAsync(int id);

        // Tela de edição: entidade rastreada pelo EF para salvar alterações
        Task<Pet?> GetParaEdicaoAsync(int id);

        Task AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task AtualizarStatusAsync(int id, string status);
        Task DeleteAsync(int id);
    }
}
