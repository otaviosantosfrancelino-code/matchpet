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
        
        // SISTEMA DE VALIDAÇÃO DE DOCUMENTO (OCR SIMULADO)
        public string StatusValidacaoDocumento { get; set; } = "Pendente"; 
        public DateTime? DataValidacaoDocumento { get; set; }

        public string Classificacao { get; set; } = "Novo Usuário"; 
        
        // FASE 3 - PERFIL ONG E DOAÇÕES FINANCEIRAS
        public bool EhOng { get; set; } = false; 
        public string? ChavePix { get; set; }
        public string? DescricaoOng { get; set; }

        // SISTEMA DE CONFIRMAÇÃO DE SEGURANÇA
        public bool EmailConfirmado { get; set; } = false;
        public string? CodigoConfirmacao { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public DateTime? DataUltimaAlteracao { get; set; }

        public string IpAceite { get; set; } = string.Empty;
        public bool AceitouTermos { get; set; } = false;
        public DateTime? DataAceiteTermos { get; set; }
    }
}