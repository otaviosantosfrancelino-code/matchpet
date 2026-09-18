using crudcomdb.Models;
using System.Net;
using System.Net.Mail;

namespace crudcomdb.Services
{
    public class AuthService
    {
        public Usuario? UsuarioLogado { get; private set; }
        public bool EstaLogado => UsuarioLogado != null;
        public event Action? OnChage;

        public void Login(Usuario usuario)
        {
            AtualizarClassificacao(usuario);
            UsuarioLogado = usuario;
            NotifyStateChanged();
        }

        public void Logout()
        {
            UsuarioLogado = null;
            NotifyStateChanged();
        }

        public void AtualizarClassificacao(Usuario user)
        {
            if (user == null) return;
            bool temDocumento = !string.IsNullOrEmpty(user.FotoDocumentoBase64);
            var tempoDeConta = DateTime.UtcNow - user.DataCadastro;

            if (tempoDeConta.TotalDays <= 21) user.Classificacao = "Novo Usuário";
            else if (temDocumento) user.Classificacao = "Usuário Bronze";
        }

        public void NotifyStateChanged() => OnChage?.Invoke();

        // ENVIO REAL DE E-MAIL DE SEGURANÇA VIA SMTP
        public async Task EnviarEmailConfirmacaoAsync(string emailDestino, string codigo)
        {
            try
            {
                // Substitua com um e-mail real e a respectiva senha de aplicativo (App Password)
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("seuemailaqui@gmail.com", "suasenhadegmailapp"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("seuemailaqui@gmail.com", "Match-Pet SP"),
                    Subject = "Código de Segurança - Cadastro Match-Pet SP",
                    Body = $"<div style='font-family:Arial,sans-serif;padding:20px;background:#f8f9fa;border-radius:10px;'>" +
                           $"<h2 style='color:#1a73e8;'>Match-Pet SP</h2>" +
                           $"<p>Olá! Obrigado por se cadastrar em nossa plataforma de adoção.</p>" +
                           $"<p>Seu código de confirmação de segurança é:</p>" +
                           $"<h1 style='background:#1a73e8;color:#fff;padding:10px 20px;display:inline-block;border-radius:5px;'>{codigo}</h1>" +
                           $"<p style='color:#6c757d;font-size:12px;margin-top:20px;'>Se você não solicitou este cadastro, ignore este e-mail.</p>" +
                           $"</div>",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(emailDestino);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro crítico ao enviar e-mail real: {ex.Message}");
                throw new Exception("Não foi possível enviar o e-mail de confirmação. Verifique o endereço informado.");
            }
        }
    }
}