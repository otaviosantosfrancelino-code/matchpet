using crudcomdb.Models;

namespace crudcomdb.Interfaces
{
    public interface IPetRepository
    {
        Task<IEnumerable<Pet>> GetAllAsync();
        Task<Pet?> GetByIdAsync(int id);
        Task AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(int id);
        
        // Match com 6 parâmetros (incluindo cidade)
        Task<IEnumerable<Pet>> GetPetsParaMatch(bool temCriancas, bool moraEmApartamento, string? especie, string? porte, string? sexo, string? cidade);
        
        // Funções da Fase 3
        Task<IEnumerable<Pet>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Pet>> GetNgoPetsAsync();
        Task<IEnumerable<Pet>> GetAdoptedPetsAsync();
    }
}