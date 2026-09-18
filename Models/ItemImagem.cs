using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crudcomdb.Models
{
    public class ItemImagem
    {
        [Key]
        public int Id { get; set; }

        public int ItemDoacaoId { get; set; }

        [Required]
        public string Base64Data { get; set; } = string.Empty;
    }
}