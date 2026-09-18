using crudcomdb.Models;

namespace crudcomdb.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByDocumentoAsync(string documento); // NOVO
        Task<Usuario?> GetByIdAsync(int id);
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
    }
}