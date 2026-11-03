using System.Collections.Concurrent;
using System.Security.Claims;
using System.Security.Cryptography;
using crudcomdb.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace crudcomdb.Endpoints
{
    // Entrar com Google ou Facebook.
    // 1) /entrar/externo/Google manda a pessoa para o Google;
    // 2) o Google devolve para /signin-google, que grava um cookie temporário;
    // 3) /entrar/externo/retorno lê o e-mail, apaga o cookie e gera um código de uso único (5 min);
    // 4) a página /login (ou /cadastro, se ainda não houver conta) troca o código pela sessão normal do site.
    public static class LoginExternoEndpoints
    {
        public const string EsquemaTemporario = "Externo";
        public static readonly string[] Provedores = { "Google", "Facebook" };

        public record DadosExternos(string Provedor, string Email, string Nome, int? UsuarioId, DateTime ExpiraEm);

        private static readonly ConcurrentDictionary<string, DadosExternos> Codigos = new();

        // Configura só os provedores que têm chaves (variáveis de ambiente no Render, nunca no código)
        public static void AddLoginExterno(this WebApplicationBuilder builder)
        {
            var auth = builder.Services.AddAuthentication()
                .AddCookie(EsquemaTemporario, o =>
                {
                    o.Cookie.Name = "matchpet_externo";
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                });

            var cfg = builder.Configuration;
            if (!string.IsNullOrWhiteSpace(cfg["Authentication:Google:ClientId"]) && !string.IsNullOrWhiteSpace(cfg["Authentication:Google:ClientSecret"]))
                auth.AddGoogle(o =>
                {
                    o.SignInScheme = EsquemaTemporario;
                    o.ClientId = cfg["Authentication:Google:ClientId"]!;
                    o.ClientSecret = cfg["Authentication:Google:ClientSecret"]!;
                });

            if (!string.IsNullOrWhiteSpace(cfg["Authentication:Facebook:AppId"]) && !string.IsNullOrWhiteSpace(cfg["Authentication:Facebook:AppSecret"]))
                auth.AddFacebook(o =>
                {
                    o.SignInScheme = EsquemaTemporario;
                    o.AppId = cfg["Authentication:Facebook:AppId"]!;
                    o.AppSecret = cfg["Authentication:Facebook:AppSecret"]!;
                    o.Scope.Add("email");
                    o.Fields.Add("name");
                    o.Fields.Add("email");
                });
        }

        public static void MapLoginExterno(this WebApplication app)
        {
            var grupo = app.MapGroup("/entrar/externo").RequireRateLimiting("login");
            grupo.MapGet("/{provedor}", async (string provedor, string? voltar, IAuthenticationSchemeProvider esquemas) =>
            {
                var nome = Provedores.FirstOrDefault(p => p.Equals(provedor, StringComparison.OrdinalIgnoreCase));
                if (nome == null || await esquemas.GetSchemeAsync(nome) == null)
                    return Results.Redirect("/login?erro=indisponivel");

                var destino = "/entrar/externo/retorno" + (EhLocal(voltar) ? "?voltar=" + Uri.EscapeDataString(voltar!) : "");
                return Results.Challenge(new AuthenticationProperties { RedirectUri = destino }, new[] { nome });
            });

            grupo.MapGet("/retorno", async (HttpContext ctx, string? voltar, IUsuarioRepository usuarios) =>
            {
                var resultado = await ctx.AuthenticateAsync(EsquemaTemporario);
                await ctx.SignOutAsync(EsquemaTemporario);
                if (!resultado.Succeeded || resultado.Principal == null) return Results.Redirect("/login?erro=cancelado");

                var email = resultado.Principal.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant();
                var nome = resultado.Principal.FindFirstValue(ClaimTypes.Name) ?? "";
                var provedor = resultado.Principal.FindFirst(ClaimTypes.Email)?.Issuer ?? "Google";
                if (string.IsNullOrEmpty(email)) return Results.Redirect("/login?erro=sem-email");

                LimparVencidos();
                var usuario = await usuarios.GetByEmailAsync(email);
                var codigo = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
                Codigos[codigo] = new DadosExternos(provedor, email, nome, usuario?.Id, DateTime.UtcNow.AddMinutes(5));

                if (usuario != null)
                    return Results.Redirect($"/login?codigo={codigo}" + (EhLocal(voltar) ? "&voltar=" + Uri.EscapeDataString(voltar!) : ""));
                return Results.Redirect($"/cadastro?externo={codigo}");
            });
        }

        // Usa o código uma única vez
        public static DadosExternos? Consumir(string? codigo)
        {
            if (string.IsNullOrEmpty(codigo) || !Codigos.TryRemove(codigo, out var dados)) return null;
            return dados.ExpiraEm > DateTime.UtcNow ? dados : null;
        }

        // Só lê (o cadastro precisa do código até a conta ser criada)
        public static DadosExternos? Ler(string? codigo) =>
            !string.IsNullOrEmpty(codigo) && Codigos.TryGetValue(codigo, out var d) && d.ExpiraEm > DateTime.UtcNow ? d : null;

        private static bool EhLocal(string? url) => !string.IsNullOrEmpty(url) && url.StartsWith('/') && !url.StartsWith("//");

        private static void LimparVencidos()
        {
            foreach (var (chave, dados) in Codigos)
                if (dados.ExpiraEm < DateTime.UtcNow) Codigos.TryRemove(chave, out _);
        }
    }
}
