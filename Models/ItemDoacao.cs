using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crudcomdb.Models
{
    public class ItemDoacao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do item é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecione para qual animal o item se destina")]
        public string EspecieDestino { get; set; } = "Cão"; // Cão, Gato, Pássaro, Peixe, Todos, Outros

        public string Categoria { get; set; } = "Acessório"; // Ração/Alimento, Casinha/Cama, Gaiola/Aquário, Remédio/Higiene, Brinquedo, Acessório

        [Required(ErrorMessage = "A descrição do item é obrigatória")]
        public string Descricao { get; set; } = string.Empty;

        public string Status { get; set; } = "Disponível"; // Disponível, Doado

        public string FotoUrl { get; set; } = string.Empty;

        [Required]
        public int UsuarioDoadorId { get; set; }

        [ForeignKey("UsuarioDoadorId")]
        public Usuario? UsuarioDoador { get; set; }

        public List<ItemImagem> Imagens { get; set; } = new List<ItemImagem>();

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}