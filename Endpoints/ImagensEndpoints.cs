using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Endpoints
{
    // Fotos e vídeos servidos por endereço próprio (ex.: /imagens/pet/15).
    //
    // Antes, as fotos (em Base64) iam dentro da página pela conexão do Blazor: cada lista
    // carregava megabytes e o celular sofria. Agora a página leva só o endereço e o navegador
    // baixa a foto separadamente, em paralelo, e guarda em cache.
    public static class ImagensEndpoints
    {
        private const int UmDia = 86400;
        private const int UmAno = 31536000;

        public static void MapImagens(this WebApplication app)
        {
            app.MapGet("/imagens/pet/{id:int}", async (int id, IDbContextFactory<AppDbContext> fabrica, HttpContext http) =>
            {
                await using var db = await fabrica.CreateDbContextAsync();
                var dado = await db.PetImagens.AsNoTracking().Where(i => i.Id == id).Select(i => i.Base64Data).FirstOrDefaultAsync();
                return Responder(dado, http, UmAno);
            });

            app.MapGet("/imagens/item/{id:int}", async (int id, IDbContextFactory<AppDbContext> fabrica, HttpContext http) =>
            {
                await using var db = await fabrica.CreateDbContextAsync();
                var dado = await db.ItemImagens.AsNoTracking().Where(i => i.Id == id).Select(i => i.Base64Data).FirstOrDefaultAsync();
                return Responder(dado, http, UmAno);
            });

            // A foto de perfil pode mudar: o endereço leva ?v=<data da alteração> para atualizar o cache
            app.MapGet("/imagens/usuario/{id:int}", async (int id, IDbContextFactory<AppDbContext> fabrica, HttpContext http) =>
            {
                await using var db = await fabrica.CreateDbContextAsync();
                var dado = await db.Usuarios.AsNoTracking().Where(u => u.Id == id).Select(u => u.FotoPerfilBase64).FirstOrDefaultAsync();
                return Responder(dado, http, http.Request.Query.ContainsKey("v") ? UmAno : UmDia);
            });

            // Arquivos enviados (fotos e vídeos de postagens, fotos do chat). Suporta "pular" no vídeo.
            app.MapGet("/midia/{id:int}", async (int id, IDbContextFactory<AppDbContext> fabrica, HttpContext http) =>
            {
                await using var db = await fabrica.CreateDbContextAsync();
                var midia = await db.Midias.AsNoTracking().Where(m => m.Id == id)
                    .Select(m => new { m.Dados, m.ContentType }).FirstOrDefaultAsync();
                if (midia == null) return Results.NotFound();
                http.Response.Headers.CacheControl = $"public, max-age={UmAno}, immutable";
                return Results.File(midia.Dados, midia.ContentType, enableRangeProcessing: true);
            });
        }

        // O banco guarda "data:image/jpeg;base64,..." (enviado no site) ou um link externo (cadastros de demonstração)
        private static IResult Responder(string? dado, HttpContext http, int segundosCache)
        {
            if (string.IsNullOrEmpty(dado)) return Results.NotFound();
            http.Response.Headers.CacheControl = $"public, max-age={segundosCache}";

            if (dado.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return Results.Redirect(dado);

            var virgula = dado.IndexOf(',');
            if (!dado.StartsWith("data:") || virgula < 0) return Results.NotFound();

            var cabecalho = dado[5..virgula];                     // image/jpeg;base64  ou  image/svg+xml;charset=utf-8
            var tipo = cabecalho.Split(';')[0];
            var conteudo = dado[(virgula + 1)..];

            byte[] bytes = cabecalho.Contains("base64")
                ? Convert.FromBase64String(conteudo)
                : System.Text.Encoding.UTF8.GetBytes(Uri.UnescapeDataString(conteudo));

            return Results.File(bytes, tipo);
        }
    }

    // Endereços prontos para usar nas páginas
    public static class Url
    {
        public static string Pet(PetImagem img) => img.Id > 0 ? $"/imagens/pet/{img.Id}" : img.Base64Data;
        public static string Item(ItemImagem img) => img.Id > 0 ? $"/imagens/item/{img.Id}" : img.Base64Data;
    }
}
