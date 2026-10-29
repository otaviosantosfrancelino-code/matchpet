using crudcomdb.Models;

namespace crudcomdb.Interfaces
{
    public interface IPetRepository
    {
        // Listagens "leves": só a 1ª foto e dados públicos do doador (carregam rápido)
        Task<IEnumerable<Pet>> GetAllAsync();
        Task<IEnumerable<Pet>> GetPetsParaMatch(bool temCriancas, bool moraEmApartamento, string? especie, string? porte, string? sexo, string? cidade);
        Task<IEnumerable<Pet>> BuscarAsync(FiltroPets filtro);
        Task<List<string>> CidadesComPetsAsync();
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

namespace crudcomdb.Interfaces
{
    // Filtros do Catálogo
    public class FiltroPets
    {
        public string? Especie { get; set; }
        public string? Porte { get; set; }
        public string? Sexo { get; set; }
        public string? Cidade { get; set; }
        public string? Raca { get; set; }
        public string? FaixaIdade { get; set; }   // filhote, jovem, adulto, idoso
        public bool TemCriancas { get; set; }
        public bool MoraEmApto { get; set; }
        public bool SoCastrados { get; set; }
        public bool SoOngs { get; set; }
        public string Ordem { get; set; } = "recentes"; // recentes, antigos, nome, novos
    }
}
