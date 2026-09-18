using System.ComponentModel.DataAnnotations;

namespace crudcomdb.Models
{
    public class PetImagem
    {
        [Key]
        public int Id { get; set; }
        public string Base64Data { get; set; } = string.Empty;
        public int PetId { get; set; } // Chave Estrangeira para Pet
    }
}