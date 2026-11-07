using System.ComponentModel.DataAnnotations;

namespace crudcomdb.Models
{
    // =====================================================================
    // MODERAÇÃO, FAVORITOS, PERFIL DE ADOTANTE E AVALIAÇÃO DE USO
    // =====================================================================

    // Denúncia feita por um usuário sobre um pet, item, publicação, comentário ou perfil
    public class Denuncia
    {
        [Key] public int Id { get; set; }
        public int DenuncianteId { get; set; }
        public Usuario? Denunciante { get; set; }

        public string Tipo { get; set; } = "";      // pet, item, post, comentario, usuario
        public int AlvoId { get; set; }

        [MaxLength(60)] public string Motivo { get; set; } = "";
        [MaxLength(1000)] public string? Detalhes { get; set; }

        public string Status { get; set; } = "Aberta"; // Aberta, Resolvida, Descartada
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public int? ResolvidaPorId { get; set; }
        public DateTime? DataResolucao { get; set; }
        [MaxLength(300)] public string? Resolucao { get; set; }
    }

    // Pet salvo nos favoritos (coração)
    public class Favorito
    {
        [Key] public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int PetId { get; set; }
        public Pet? Pet { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    // Questionário de adoção responsável: respondido uma vez pelo adotante e mostrado ao doador no chat
    public class PerfilAdotante
    {
        [Key] public int Id { get; set; }
        public int UsuarioId { get; set; }

        public string Moradia { get; set; } = "Casa";        // Casa, Apartamento, Chácara/Sítio
        public bool MoradiaPropria { get; set; } = true;
        public bool ProprietarioPermite { get; set; } = true; // se for alugada
        public bool TemQuintal { get; set; }
        public bool TelasDeProtecao { get; set; }
        public bool TemCriancas { get; set; }
        public bool TemOutrosAnimais { get; set; }
        [MaxLength(200)] public string? OutrosAnimais { get; set; }
        public int HorasSozinho { get; set; } = 4;            // horas por dia que o pet ficaria sozinho
        public bool JaTevePets { get; set; }
        public bool TodosConcordam { get; set; }
        public bool CienteDosCustos { get; set; }
        public bool AceitaAcompanhamento { get; set; }
        [MaxLength(600)] public string? Motivacao { get; set; }

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }

    // Avaliação de usabilidade com o questionário padronizado SUS (System Usability Scale)
    public class AvaliacaoUso
    {
        [Key] public int Id { get; set; }
        public int? UsuarioId { get; set; }
        public string Perfil { get; set; } = "Visitante";   // Adotante, Doador, ONG, Visitante
        public string Respostas { get; set; } = "";         // 10 notas de 1 a 5 separadas por vírgula
        public double Pontuacao { get; set; }               // 0 a 100
        [MaxLength(800)] public string? Comentario { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
