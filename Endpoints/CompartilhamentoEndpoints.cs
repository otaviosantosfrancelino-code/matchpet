using System.Net;
using System.Text;
using crudcomdb.Data;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Endpoints
{
    // Endereços para quem não é um navegador comum:
    //  /p/{id}       link curto para compartilhar um pet no WhatsApp/redes, com a prévia (foto, nome, descrição)
    //  /sitemap.xml  lista de páginas para o Google encontrar o site
    public static class CompartilhamentoEndpoints
    {
        public static void MapCompartilhamento(this WebApplication app)
        {
            app.MapGet("/p/{id:int}", async (int id, IDbContextFactory<AppDbContext> fabrica, HttpContext http) =>
            {
                await using var db = await fabrica.CreateDbContextAsync();
                var pet = await db.Pets.AsNoTracking().Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Nome, p.Especie, p.Raca, p.Idade, p.Porte, p.Status, p.Descricao,
                        Cidade = p.UsuarioDoador != null ? p.UsuarioDoador.Cidade : "Franca",
                        Foto = p.Imagens.OrderBy(i => i.Id).Select(i => new { i.Id, Externa = i.Base64Data.StartsWith("http") ? i.Base64Data : null }).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                var site = $"{http.Request.Scheme}://{http.Request.Host}";
                var destino = $"/pets/detalhes/{id}";
                if (pet == null || pet.Status == ModeracaoService.StatusRemovido) return Results.Redirect("/pets");

                var titulo = pet.Status == "Adotado" ? $"{pet.Nome} já encontrou um lar! 💛" : $"Adote {pet.Nome}! 🐾";
                var descricao = $"{pet.Especie} · {pet.Raca} · {PetIdade(pet.Idade)} · {pet.Porte} · {pet.Cidade}/SP. " +
                                (pet.Descricao.Length > 140 ? pet.Descricao[..140] + "…" : pet.Descricao);
                var imagem = pet.Foto == null ? $"{site}/home-match-pet.png" : pet.Foto.Externa ?? $"{site}/imagens/pet/{pet.Foto.Id}";

                string H(string s) => WebUtility.HtmlEncode(s);
                var html = $"""
                    <!DOCTYPE html>
                    <html lang="pt-BR"><head>
                    <meta charset="utf-8" />
                    <title>{H(titulo)} | Match-Pet SP</title>
                    <meta name="description" content="{H(descricao)}" />
                    <meta property="og:type" content="website" />
                    <meta property="og:site_name" content="Match-Pet SP" />
                    <meta property="og:title" content="{H(titulo)}" />
                    <meta property="og:description" content="{H(descricao)}" />
                    <meta property="og:image" content="{H(imagem)}" />
                    <meta property="og:url" content="{H(site + "/p/" + id)}" />
                    <meta name="twitter:card" content="summary_large_image" />
                    <meta http-equiv="refresh" content="0; url={destino}" />
                    <link rel="canonical" href="{H(site + destino)}" />
                    </head><body style="font-family:system-ui;text-align:center;padding:3rem">
                    <p>Abrindo o anúncio de {H(pet.Nome)}…</p>
                    <p><a href="{destino}">Clique aqui se a página não abrir</a></p>
                    <script>location.replace("{destino}");</script>
                    </body></html>
                    """;
                http.Response.Headers.CacheControl = "public, max-age=600";
                return Results.Content(html, "text/html; charset=utf-8");
            });

            app.MapGet("/sitemap.xml", async (IDbContextFactory<AppDbContext> fabrica, HttpContext http) =>
            {
                await using var db = await fabrica.CreateDbContextAsync();
                var site = $"{http.Request.Scheme}://{http.Request.Host}";
                var pets = await db.Pets.AsNoTracking().Where(p => p.Status == "Disponível").OrderByDescending(p => p.Id).Select(p => p.Id).Take(2000).ToListAsync();
                string[] paginas = { "/", "/pets", "/bazar", "/ongs", "/comunidade", "/final-feliz", "/estatisticas", "/sobre", "/avaliar", "/verificar-termo", "/privacidade", "/cadastro" };

                var xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
                foreach (var p in paginas) xml.Append($"  <url><loc>{site}{p}</loc><changefreq>daily</changefreq></url>\n");
                foreach (var id in pets) xml.Append($"  <url><loc>{site}/pets/detalhes/{id}</loc><changefreq>weekly</changefreq></url>\n");
                xml.Append("</urlset>");
                http.Response.Headers.CacheControl = "public, max-age=3600";
                return Results.Content(xml.ToString(), "application/xml; charset=utf-8");
            });
        }

        private static string PetIdade(int anos) => anos switch { <= 0 => "Filhote", 1 => "1 ano", _ => $"{anos} anos" };
    }
}
