using System.ComponentModel.DataAnnotations;

namespace crudcomdb.Models
{
    // =====================================================================
    // REDE SOCIAL, CHAT E TERMOS DIGITAIS
    // =====================================================================

    // Arquivo enviado pelos usuários (foto ou vídeo de postagem, foto no chat).
    // Fica guardado no banco e é servido pelo endereço /midia/{Id}.
    public class Midia
    {
        [Key] public int Id { get; set; }
        public byte[] Dados { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "image/jpeg";
        public long Tamanho { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    // Publicação no feed da Comunidade
    public class Postagem
    {
        [Key] public int Id { get; set; }
        public int AutorId { get; set; }
        public Usuario? Autor { get; set; }

        [MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;

        // "/midia/12" (enviado no site) ou um link externo
        public string? MidiaUrl { get; set; }
        public string? TipoMidia { get; set; } // "imagem" ou "video"

        // Pet marcado na publicação (opcional)
        public int? PetId { get; set; }
        public Pet? Pet { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public List<Curtida> Curtidas { get; set; } = new();
        public List<Comentario> Comentarios { get; set; } = new();
    }

    public class Curtida
    {
        [Key] public int Id { get; set; }
        public int PostagemId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    public class Comentario
    {
        [Key] public int Id { get; set; }
        public int PostagemId { get; set; }
        public int AutorId { get; set; }
        public Usuario? Autor { get; set; }

        [MaxLength(1000)]
        public string Texto { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

    // Pedido de amizade. Vira amizade quando Status = "Aceita".
    public class Amizade
    {
        [Key] public int Id { get; set; }
        public int SolicitanteId { get; set; }
        public Usuario? Solicitante { get; set; }
        public int DestinatarioId { get; set; }
        public Usuario? Destinatario { get; set; }
        public string Status { get; set; } = "Pendente"; // Pendente, Aceita
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataResposta { get; set; }
    }

    // Conversa entre duas pessoas (sobre um pet, um item do bazar, ou direta)
    public class Conversa
    {
        [Key] public int Id { get; set; }
        public int IniciadorId { get; set; }
        public Usuario? Iniciador { get; set; }
        public int DestinatarioId { get; set; }
        public Usuario? Destinatario { get; set; }

        public int? PetId { get; set; }
        public Pet? Pet { get; set; }
        public int? ItemDoacaoId { get; set; }
        public ItemDoacao? ItemDoacao { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime UltimaMensagemEm { get; set; } = DateTime.UtcNow;

        public List<Mensagem> Mensagens { get; set; } = new();
    }

    public class Mensagem
    {
        [Key] public int Id { get; set; }
        public int ConversaId { get; set; }
        public int RemetenteId { get; set; }

        [MaxLength(2000)]
        public string Texto { get; set; } = string.Empty;

        public string Tipo { get; set; } = "texto"; // texto, imagem, termo, sistema
        public string? MidiaUrl { get; set; }
        public int? TermoAdocaoId { get; set; }
        public TermoAdocao? TermoAdocao { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public bool Lida { get; set; }
    }

    // Termo de adoção enviado pelo doador dentro do chat e assinado pelo adotante
    public class TermoAdocao
    {
        [Key] public int Id { get; set; }
        public int PetId { get; set; }
        public Pet? Pet { get; set; }
        public int DoadorId { get; set; }
        public Usuario? Doador { get; set; }
        public int AdotanteId { get; set; }
        public Usuario? Adotante { get; set; }
        public int ConversaId { get; set; }

        public string Texto { get; set; } = string.Empty;
        public string Status { get; set; } = "Aguardando assinatura"; // Aguardando assinatura, Assinado, Cancelado

        // Código curto para conferir a autenticidade do documento impresso
        public string Codigo { get; set; } = string.Empty;

        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
        public string IpDoador { get; set; } = string.Empty;
        public DateTime? DataAssinatura { get; set; }
        public string? IpAdotante { get; set; }
    }

    // Histórias publicadas na galeria Final Feliz
    public class Depoimento
    {
        [Key] public int Id { get; set; }
        public int AutorId { get; set; }
        public Usuario? Autor { get; set; }
        public int? PetId { get; set; }
        public Pet? Pet { get; set; }

        [MaxLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string Texto { get; set; } = string.Empty;

        public string? FotoUrl { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}

namespace crudcomdb.Models
{
    // Notificação do site (sininho): curtida, comentário, amizade, termo, ranking...
    public class Notificacao
    {
        [System.ComponentModel.DataAnnotations.Key] public int Id { get; set; }
        public int UsuarioId { get; set; }            // quem recebe
        public int? AtorId { get; set; }              // quem causou (ex.: quem curtiu)
        public string Tipo { get; set; } = "";        // curtida, comentario, amizade, amizade_aceita, termo, termo_assinado, ranking, adocao
        public string Texto { get; set; } = "";
        public string? Link { get; set; }
        public string? Chave { get; set; }            // evita notificação repetida (ex.: a mesma curtida)
        public bool Lida { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
