using crudcomdb.Models;

namespace crudcomdb.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByEmailAsync(string email);           // ignora maiúsculas/minúsculas
        Task<Usuario?> GetByDocumentoAsync(string documento);   // ignora pontos, traços e barras
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByResetTokenHashAsync(string tokenHash);

        // ONGs com dados públicos (sem documento) para a página de ONGs
        Task<List<Usuario>> GetOngsAsync();
        Task<int> ContarOngsAsync();

        // Antifraude: outra conta já usou a mesma foto de documento?
        Task<Usuario?> GetOutroComMesmoDocumentoHashAsync(string hash, int idAtual);

        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
    }
}
