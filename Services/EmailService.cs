using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;

namespace crudcomdb.Services
{
    // Envio de e-mails do site (ex.: recuperação de senha).
    //
    // Configuração (appsettings.json, seção "Email", ou variáveis de ambiente Email__Provedor etc.):
    //   Provedor: "Brevo" (recomendado: envia pela internet, funciona no Render)  ou  "Smtp"
    //   RemetenteEmail / RemetenteNome: quem aparece como remetente
    //   Brevo:  BrevoApiKey
    //   Smtp:   SmtpHost, SmtpPorta, SmtpUsuario, SmtpSenha  (ex.: Gmail com "senha de app")
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _http;
        private readonly ILogger<EmailService> _log;

        public EmailService(IConfiguration config, IHttpClientFactory http, ILogger<EmailService> log)
        {
            _config = config;
            _http = http;
            _log = log;
        }

        // Se só a chave do Brevo for informada, o provedor é escolhido sozinho
        private string Provedor => !string.IsNullOrWhiteSpace(_config["Email:Provedor"]) ? _config["Email:Provedor"]!.Trim()
            : !string.IsNullOrWhiteSpace(_config["Email:BrevoApiKey"]) ? "Brevo"
            : !string.IsNullOrWhiteSpace(_config["Email:SmtpHost"]) ? "Smtp" : "";
        private string RemetenteEmail => _config["Email:RemetenteEmail"] ?? "";
        private string RemetenteNome => _config["Email:RemetenteNome"] ?? "Match-Pet SP";

        public bool Configurado => Provedor.Equals("Brevo", StringComparison.OrdinalIgnoreCase)
            ? !string.IsNullOrWhiteSpace(_config["Email:BrevoApiKey"]) && !string.IsNullOrWhiteSpace(RemetenteEmail)
            : Provedor.Equals("Smtp", StringComparison.OrdinalIgnoreCase)
              && !string.IsNullOrWhiteSpace(_config["Email:SmtpHost"]) && !string.IsNullOrWhiteSpace(RemetenteEmail);

        public async Task<bool> EnviarAsync(string para, string nomePara, string assunto, string html)
        {
            if (!Configurado)
            {
                _log.LogWarning("E-mail não configurado. Mensagem para {Para} não enviada: {Assunto}", para, assunto);
                return false;
            }

            try
            {
                if (Provedor.Equals("Brevo", StringComparison.OrdinalIgnoreCase))
                {
                    var cliente = _http.CreateClient();
                    using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
                    req.Headers.Add("api-key", _config["Email:BrevoApiKey"]);
                    req.Content = JsonContent.Create(new
                    {
                        sender = new { name = RemetenteNome, email = RemetenteEmail },
                        to = new[] { new { email = para, name = string.IsNullOrWhiteSpace(nomePara) ? para : nomePara } },
                        subject = assunto,
                        htmlContent = html
                    });
                    var resp = await cliente.SendAsync(req);
                    if (!resp.IsSuccessStatusCode)
                    {
                        _log.LogError("Brevo recusou o e-mail ({Status}): {Corpo}", (int)resp.StatusCode, await resp.Content.ReadAsStringAsync());
                        return false;
                    }
                    return true;
                }

                using var smtp = new SmtpClient(_config["Email:SmtpHost"], int.TryParse(_config["Email:SmtpPorta"], out var porta) ? porta : 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_config["Email:SmtpUsuario"], _config["Email:SmtpSenha"])
                };
                using var msg = new MailMessage { From = new MailAddress(RemetenteEmail, RemetenteNome), Subject = assunto, Body = html, IsBodyHtml = true };
                msg.To.Add(new MailAddress(para, nomePara));
                await smtp.SendMailAsync(msg);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Falha ao enviar e-mail para {Para}", para);
                return false;
            }
        }

        // Modelo visual dos e-mails do Match-Pet
        public static string Modelo(string titulo, string corpoHtml, string? textoBotao = null, string? link = null) => $@"
<div style=""font-family:Arial,Helvetica,sans-serif;background:#f1f5f9;padding:32px 12px"">
  <div style=""max-width:520px;margin:0 auto;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e2e8f0"">
    <div style=""background:linear-gradient(135deg,#2563eb,#1e40af);padding:24px;color:#fff"">
      <div style=""font-size:20px;font-weight:bold"">🐾 Match-Pet SP</div>
    </div>
    <div style=""padding:28px;color:#0f172a;font-size:15px;line-height:1.6"">
      <h2 style=""margin:0 0 12px;font-size:20px"">{WebUtility.HtmlEncode(titulo)}</h2>
      {corpoHtml}
      {(textoBotao != null && link != null ? $@"<p style=""text-align:center;margin:28px 0""><a href=""{link}"" style=""background:#2563eb;color:#fff;text-decoration:none;padding:14px 28px;border-radius:12px;font-weight:bold;display:inline-block"">{WebUtility.HtmlEncode(textoBotao)}</a></p>
      <p style=""font-size:12px;color:#64748b"">Se o botão não funcionar, copie e cole no navegador:<br><span style=""word-break:break-all"">{link}</span></p>" : "")}
    </div>
    <div style=""padding:16px 28px;background:#f8fafc;color:#64748b;font-size:12px"">Você recebeu este e-mail porque há uma conta no Match-Pet SP com este endereço.</div>
  </div>
</div>";
    }
}
