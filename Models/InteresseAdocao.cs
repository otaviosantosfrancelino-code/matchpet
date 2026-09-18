using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crudcomdb.Models
{
    public class InteresseAdocao
    {
        [Key]
        public int Id { get; set; }
        
        public int PetId { get; set; }
        [ForeignKey("PetId")]
        public Pet? Pet { get; set; }

        public int UsuarioInteressadoId { get; set; }
        [ForeignKey("UsuarioInteressadoId")]
        public Usuario? Usuario { get; set; }

        public DateTime DataInteresse { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pendente"; 

        public bool TermoAceito { get; set; }
        public string IpUsuario { get; set; } = string.Empty;

        // FASE 4 - REGISTRO JURÍDICO
        public string ContratoGerado { get; set; } = string.Empty;
    }
}