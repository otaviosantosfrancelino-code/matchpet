using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace crudcomdb.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Documento é obrigatório")]
        public string Documento { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório")]
        public string Telefone { get; set; } = string.Empty;

        public string Senha { get; set; } = string.Empty;

        [NotMapped]
        public string ConfirmarSenha { get; set; } = string.Empty;

        public string CEP { get; set; } = string.Empty;
        public string Logradouro { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = "Franca";
        public string Estado { get; set; } = "SP";

        // FASE 1 - IDENTIDADE
        public string? FotoPerfilBase64 { get; set; } 
        public string? FotoDocumentoBase64 { get; set; } 
        
        // SISTEMA DE VALIDAÇÃO DE DOCUMENTO
        // Status: Pendente, Em Análise, Aprovado, Reprovado
        public string StatusValidacaoDocumento { get; set; } = "Pendente";
        public DateTime? DataValidacaoDocumento { get; set; }

        // Impressão digital (SHA-256) da foto do documento: detecta a MESMA foto usada em outra conta
        public string? DocumentoHash { get; set; }

        // Explicação do resultado da validação (mostrada ao usuário no Perfil)
        public string? DocumentoObservacao { get; set; }

        // RECUPERAÇÃO DE SENHA: guardamos só o hash do código enviado por e-mail, com validade
        public string? ResetSenhaTokenHash { get; set; }
        public DateTime? ResetSenhaExpiraEm { get; set; }

        public string Classificacao { get; set; } = "Novo Usuário"; 
        
        // Nas listagens a foto não é carregada, só este marcador avisando que ela existe
        public const string FotoPorUrl = "@url";

        // Endereço da foto de perfil para usar no <img> (o navegador baixa e guarda em cache)
        [NotMapped]
        public string? UrlFoto => string.IsNullOrEmpty(FotoPerfilBase64) ? null
            : FotoPerfilBase64.StartsWith("http") ? FotoPerfilBase64
            : Id > 0 ? $"/imagens/usuario/{Id}?v={(DataUltimaAlteracao ?? DataCadastro).Ticks}"
            : FotoPerfilBase64;

        // Texto "sobre mim" exibido no perfil público
        [MaxLength(600)]
        public string? Bio { get; set; }

        // FASE 3 - PERFIL ONG E DOAÇÕES FINANCEIRAS
        public bool EhOng { get; set; } = false; 
        public string? ChavePix { get; set; }
        public string? DescricaoOng { get; set; }

        // SISTEMA DE CONFIRMAÇÃO DE SEGURANÇA
        public bool EmailConfirmado { get; set; } = false;
        public string? CodigoConfirmacao { get; set; }

        // MODERAÇÃO
        public bool EhAdmin { get; set; }               // acessa o Painel do administrador (/admin)
        public bool Bloqueado { get; set; }             // conta bloqueada não consegue entrar
        public bool ContaDemonstracao { get; set; }     // conta do QR Code da banca: vê tudo, mas dados pessoais aparecem mascarados e ações de moderação são simuladas
        [MaxLength(300)] public string? MotivoBloqueio { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public DateTime? DataUltimaAlteracao { get; set; }

        public string IpAceite { get; set; } = string.Empty;
        public bool AceitouTermos { get; set; } = false;
        public DateTime? DataAceiteTermos { get; set; }
    }
}