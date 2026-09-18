using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crudcomdb.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        public string Especie { get; set; } = string.Empty;
        public string Raca { get; set; } = "SRD";
        public string Sexo { get; set; } = string.Empty;
        public string Porte { get; set; } = string.Empty;
        public int Idade { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        
        public bool Castrado { get; set; }
        public bool PrecisaDeQuintal { get; set; }
        public bool RecomendadoParaCriancas { get; set; }

        // FASE 4 - STATUS EVOLUÍDO
        public string Status { get; set; } = "Disponível"; // Disponível, Em Processo ou Adotado

        [Required]
        public int UsuarioDoadorId { get; set; } 

        [ForeignKey("UsuarioDoadorId")]
        public Usuario? UsuarioDoador { get; set; } 

        public List<PetImagem> Imagens { get; set; } = new List<PetImagem>();
    }
}