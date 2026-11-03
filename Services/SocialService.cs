using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Comunidade: perfis públicos, amizades, publicações, curtidas, comentários e depoimentos
    public class SocialService
    {
        private readonly IDbContextFactory<AppDbContext> _fabrica;
        private readonly NotificacoesService _avisos;
        private readonly NotificacaoService _notificacoes;

        public SocialService(IDbContextFactory<AppDbContext> fabrica, NotificacoesService avisos, NotificacaoService notificacoes)
        {
            _fabrica = fabrica;
            _avisos = avisos;
            _notificacoes = notificacoes;
        }

        public const int PorPagina = 12;

        public record PostagemView(int Id, Usuario Autor, string Texto, string? MidiaUrl, string? TipoMidia,
            int? PetId, string? PetNome, DateTime Data, int Curtidas, int Comentarios, bool Curti);

        public record ComentarioView(int Id, Usuario Autor, string Texto, DateTime Data);

        public record Perfil(Usuario Usuario, int Amigos, int Postagens, int PetsAnunciados, int PetsDoados, int PetsAdotados);

        public enum Relacao { Nenhuma, PedidoEnviado, PedidoRecebido, Amigos, EuMesmo }

        // ================= PUBLICAÇÕES =================

        // aba: "todos" | "amigos" | "videos"
        public async Task<List<PostagemView>> FeedAsync(int? eu, string aba, int pagina, int? autorId = null)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var q = db.Postagens.AsNoTracking();

            if (autorId != null) q = q.Where(p => p.AutorId == autorId);
            if (aba == "videos") q = q.Where(p => p.TipoMidia == "video");
            if (aba == "amigos" && eu != null)
            {
                var amigos = IdsAmigos(db, eu.Value);
                q = q.Where(p => amigos.Contains(p.AutorId) || p.AutorId == eu);
            }

            var linhas = await q.OrderByDescending(p => p.DataCriacao)
                .Skip(pagina * PorPagina).Take(PorPagina)
                .Select(p => new
                {
                    p.Id, p.AutorId, p.Texto, p.MidiaUrl, p.TipoMidia, p.PetId, p.DataCriacao,
                    PetNome = p.Pet != null ? p.Pet.Nome : null,
                    Curtidas = p.Curtidas.Count,
                    Comentarios = p.Comentarios.Count,
                    Curti = eu != null && p.Curtidas.Any(c => c.UsuarioId == eu)
                })
                .ToListAsync();

            var autores = await Consultas.UsuariosPublicosAsync(db, linhas.Select(l => l.AutorId));
            return linhas.Where(l => autores.ContainsKey(l.AutorId))
                .Select(l => new PostagemView(l.Id, autores[l.AutorId], l.Texto, l.MidiaUrl, l.TipoMidia, l.PetId, l.PetNome,
                    l.DataCriacao, l.Curtidas, l.Comentarios, l.Curti))
                .ToList();
        }

        public async Task PublicarAsync(int autorId, string texto, string? midiaUrl, string? tipoMidia, int? petId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            db.Postagens.Add(new Postagem { AutorId = autorId, Texto = texto.Trim(), MidiaUrl = midiaUrl, TipoMidia = tipoMidia, PetId = petId });
            await db.SaveChangesAsync();
        }

        public async Task ExcluirPostagemAsync(int postagemId, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await db.Postagens.Where(p => p.Id == postagemId && p.AutorId == usuarioId).ExecuteDeleteAsync();
        }

        public async Task<string> SalvarMidiaAsync(byte[] dados, string contentType, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await Consultas.SalvarMidiaAsync(db, dados, contentType, usuarioId);
        }

        // Curtir / descurtir. Devolve (curtiu?, total)
        public async Task<(bool, int)> AlternarCurtidaAsync(int postagemId, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var existente = await db.Curtidas.FirstOrDefaultAsync(c => c.PostagemId == postagemId && c.UsuarioId == usuarioId);
            if (existente != null) db.Curtidas.Remove(existente);
            else db.Curtidas.Add(new Curtida { PostagemId = postagemId, UsuarioId = usuarioId });
            await db.SaveChangesAsync();
            if (existente == null)
            {
                // Avisa o autor (só uma vez por pessoa, mesmo que curta e descurta)
                var autorId = await db.Postagens.Where(p => p.Id == postagemId).Select(p => p.AutorId).FirstOrDefaultAsync();
                var nome = await NomeAsync(db, usuarioId);
                await _notificacoes.CriarAsync(autorId, usuarioId, "curtida", $"{nome} deu uma patinha na sua publicação.", $"/comunidade?post={postagemId}", $"curtida:{postagemId}:{usuarioId}");
            }
            return (existente == null, await db.Curtidas.CountAsync(c => c.PostagemId == postagemId));
        }

        public async Task<List<ComentarioView>> ComentariosAsync(int postagemId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var linhas = await db.Comentarios.AsNoTracking().Where(c => c.PostagemId == postagemId)
                .OrderBy(c => c.DataCriacao).Select(c => new { c.Id, c.AutorId, c.Texto, c.DataCriacao }).ToListAsync();
            var autores = await Consultas.UsuariosPublicosAsync(db, linhas.Select(l => l.AutorId));
            return linhas.Where(l => autores.ContainsKey(l.AutorId))
                .Select(l => new ComentarioView(l.Id, autores[l.AutorId], l.Texto, l.DataCriacao)).ToList();
        }

        public async Task ComentarAsync(int postagemId, int autorId, string texto)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            db.Comentarios.Add(new Comentario { PostagemId = postagemId, AutorId = autorId, Texto = texto.Trim() });
            await db.SaveChangesAsync();

            var donoId = await db.Postagens.Where(p => p.Id == postagemId).Select(p => p.AutorId).FirstOrDefaultAsync();
            var nome = await NomeAsync(db, autorId);
            var resumo = texto.Trim().Length > 60 ? texto.Trim()[..60] + "…" : texto.Trim();
            await _notificacoes.CriarAsync(donoId, autorId, "comentario", $"{nome} comentou: “{resumo}”", $"/comunidade?post={postagemId}");
        }

        private static async Task<string> NomeAsync(AppDbContext db, int usuarioId) =>
            (await db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Nome).FirstOrDefaultAsync())?.Split(' ')[0] ?? "Alguém";

        // Uma publicação só (usada quando a pessoa abre uma notificação)
        public async Task<PostagemView?> PostagemAsync(int postagemId, int? eu)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var p = await db.Postagens.AsNoTracking().Where(x => x.Id == postagemId)
                .Select(x => new
                {
                    x.Id, x.AutorId, x.Texto, x.MidiaUrl, x.TipoMidia, x.PetId, x.DataCriacao,
                    PetNome = x.Pet != null ? x.Pet.Nome : null, Curtidas = x.Curtidas.Count, Comentarios = x.Comentarios.Count,
                    Curti = eu != null && x.Curtidas.Any(c => c.UsuarioId == eu)
                }).FirstOrDefaultAsync();
            if (p == null) return null;
            var autor = (await Consultas.UsuariosPublicosAsync(db, new[] { p.AutorId })).GetValueOrDefault(p.AutorId);
            return autor == null ? null : new PostagemView(p.Id, autor, p.Texto, p.MidiaUrl, p.TipoMidia, p.PetId, p.PetNome, p.DataCriacao, p.Curtidas, p.Comentarios, p.Curti);
        }

        // ================= PERFIS =================

        public async Task<Perfil?> PerfilAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var usuarios = await Consultas.UsuariosPublicosAsync(db, new[] { usuarioId });
            if (!usuarios.TryGetValue(usuarioId, out var u)) return null;

            var amigos = await db.Amizades.CountAsync(a => a.Status == "Aceita" && (a.SolicitanteId == usuarioId || a.DestinatarioId == usuarioId));
            var postagens = await db.Postagens.CountAsync(p => p.AutorId == usuarioId);
            var anunciados = await db.Pets.CountAsync(p => p.UsuarioDoadorId == usuarioId && p.Status != "Adotado");
            var doados = await db.Pets.CountAsync(p => p.UsuarioDoadorId == usuarioId && p.Status == "Adotado");
            var adotados = await db.Pets.CountAsync(p => p.AdotanteId == usuarioId);
            return new Perfil(u, amigos, postagens, anunciados, doados, adotados);
        }

        // Pets do perfil: anunciados agora, já doados e adotados pela pessoa
        public async Task<(List<Pet> anunciados, List<Pet> doados, List<Pet> adotados)> PetsDoPerfilAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var anunciados = await Consultas.PetsLeves(db.Pets.Where(p => p.UsuarioDoadorId == usuarioId && p.Status != "Adotado").OrderByDescending(p => p.Id)).ToListAsync();
            var doados = await Consultas.PetsLeves(db.Pets.Where(p => p.UsuarioDoadorId == usuarioId && p.Status == "Adotado").OrderByDescending(p => p.DataAdocao)).ToListAsync();
            var adotados = await Consultas.PetsLeves(db.Pets.Where(p => p.AdotanteId == usuarioId).OrderByDescending(p => p.DataAdocao)).ToListAsync();
            return (anunciados, doados, adotados);
        }

        public async Task<List<Usuario>> BuscarPessoasAsync(string termo, int limite = 20)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var t = termo.Trim().ToLower();
            var ids = await db.Usuarios.Where(u => u.Nome.ToLower().Contains(t) || u.Cidade.ToLower().Contains(t))
                .OrderBy(u => u.Nome).Select(u => u.Id).Take(limite).ToListAsync();
            var dic = await Consultas.UsuariosPublicosAsync(db, ids);
            return ids.Where(dic.ContainsKey).Select(i => dic[i]).ToList();
        }

        // ================= AMIZADES =================

        private static IQueryable<int> IdsAmigos(AppDbContext db, int eu) =>
            db.Amizades.Where(a => a.Status == "Aceita" && (a.SolicitanteId == eu || a.DestinatarioId == eu))
                .Select(a => a.SolicitanteId == eu ? a.DestinatarioId : a.SolicitanteId);

        public async Task<(Relacao relacao, int? amizadeId)> RelacaoAsync(int eu, int outro)
        {
            if (eu == outro) return (Relacao.EuMesmo, null);
            await using var db = await _fabrica.CreateDbContextAsync();
            var a = await db.Amizades.AsNoTracking().FirstOrDefaultAsync(x =>
                (x.SolicitanteId == eu && x.DestinatarioId == outro) || (x.SolicitanteId == outro && x.DestinatarioId == eu));
            if (a == null) return (Relacao.Nenhuma, null);
            if (a.Status == "Aceita") return (Relacao.Amigos, a.Id);
            return (a.SolicitanteId == eu ? Relacao.PedidoEnviado : Relacao.PedidoRecebido, a.Id);
        }

        public async Task SolicitarAmizadeAsync(int eu, int outro)
        {
            if (eu == outro) return;
            await using var db = await _fabrica.CreateDbContextAsync();
            var existente = await db.Amizades.FirstOrDefaultAsync(x =>
                (x.SolicitanteId == eu && x.DestinatarioId == outro) || (x.SolicitanteId == outro && x.DestinatarioId == eu));
            if (existente != null)
            {
                // A outra pessoa já tinha me convidado: aceitar direto
                if (existente.Status == "Pendente" && existente.DestinatarioId == eu)
                {
                    existente.Status = "Aceita";
                    existente.DataResposta = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                    _avisos.Avisar(outro, "amizade");
                    await _notificacoes.CriarAsync(outro, eu, "amizade_aceita", $"{await NomeAsync(db, eu)} aceitou seu pedido de amizade.", $"/u/{eu}");
                }
                return;
            }
            db.Amizades.Add(new Amizade { SolicitanteId = eu, DestinatarioId = outro });
            await db.SaveChangesAsync();
            _avisos.Avisar(outro, "amizade");
            await _notificacoes.CriarAsync(outro, eu, "amizade", $"{await NomeAsync(db, eu)} quer ser seu amigo(a).", "/amigos", $"amizade:{eu}:{outro}");
        }

        public async Task ResponderAmizadeAsync(int amizadeId, int eu, bool aceitar)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var a = await db.Amizades.FirstOrDefaultAsync(x => x.Id == amizadeId && x.DestinatarioId == eu && x.Status == "Pendente");
            if (a == null) return;
            if (aceitar) { a.Status = "Aceita"; a.DataResposta = DateTime.UtcNow; }
            else db.Amizades.Remove(a);
            await db.SaveChangesAsync();
            _avisos.Avisar(eu, "amizade");
            if (aceitar)
            {
                _avisos.Avisar(a.SolicitanteId, "amizade");
                await _notificacoes.CriarAsync(a.SolicitanteId, eu, "amizade_aceita", $"{await NomeAsync(db, eu)} aceitou seu pedido de amizade.", $"/u/{eu}");
            }
        }

        // Desfazer amizade ou cancelar pedido enviado
        public async Task RemoverAmizadeAsync(int eu, int outro)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await db.Amizades.Where(x => (x.SolicitanteId == eu && x.DestinatarioId == outro) || (x.SolicitanteId == outro && x.DestinatarioId == eu))
                .ExecuteDeleteAsync();
            _avisos.Avisar(outro, "amizade");
        }

        public async Task<List<Usuario>> AmigosAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var ids = await IdsAmigos(db, usuarioId).ToListAsync();
            return (await Consultas.UsuariosPublicosAsync(db, ids)).Values.OrderBy(u => u.Nome).ToList();
        }

        public async Task<List<(int amizadeId, Usuario usuario)>> PedidosRecebidosAsync(int eu)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var pedidos = await db.Amizades.AsNoTracking().Where(a => a.DestinatarioId == eu && a.Status == "Pendente")
                .OrderByDescending(a => a.DataCriacao).Select(a => new { a.Id, a.SolicitanteId }).ToListAsync();
            var u = await Consultas.UsuariosPublicosAsync(db, pedidos.Select(p => p.SolicitanteId));
            return pedidos.Where(p => u.ContainsKey(p.SolicitanteId)).Select(p => (p.Id, u[p.SolicitanteId])).ToList();
        }

        public async Task<List<Usuario>> PedidosEnviadosAsync(int eu)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var ids = await db.Amizades.Where(a => a.SolicitanteId == eu && a.Status == "Pendente").Select(a => a.DestinatarioId).ToListAsync();
            return (await Consultas.UsuariosPublicosAsync(db, ids)).Values.ToList();
        }

        public async Task<int> ContarPedidosAsync(int eu)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Amizades.CountAsync(a => a.DestinatarioId == eu && a.Status == "Pendente");
        }

        // Sugestões: pessoas e ONGs da mesma cidade que ainda não são amigas
        public async Task<List<Usuario>> SugestoesAsync(int eu, string cidade, int limite = 8)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var ligados = await db.Amizades.Where(a => a.SolicitanteId == eu || a.DestinatarioId == eu)
                .Select(a => a.SolicitanteId == eu ? a.DestinatarioId : a.SolicitanteId).ToListAsync();
            ligados.Add(eu);
            var ids = await db.Usuarios.Where(u => !ligados.Contains(u.Id))
                .OrderByDescending(u => u.Cidade == cidade).ThenByDescending(u => u.EhOng).ThenBy(u => u.Id)
                .Select(u => u.Id).Take(limite).ToListAsync();
            var dic = await Consultas.UsuariosPublicosAsync(db, ids);
            return ids.Where(dic.ContainsKey).Select(i => dic[i]).ToList();
        }

        // ================= DEPOIMENTOS (Final Feliz) =================

        public record DepoimentoView(int Id, Usuario Autor, string Titulo, string Texto, string? FotoUrl, int? PetId, string? PetNome, int? PetFotoId, DateTime Data);

        public async Task<List<DepoimentoView>> DepoimentosAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var linhas = await db.Depoimentos.AsNoTracking().OrderByDescending(d => d.DataCriacao)
                .Select(d => new
                {
                    d.Id, d.AutorId, d.Titulo, d.Texto, d.FotoUrl, d.PetId, d.DataCriacao,
                    PetNome = d.Pet != null ? d.Pet.Nome : null,
                    PetFotoId = d.Pet != null ? d.Pet.Imagens.OrderBy(i => i.Id).Select(i => (int?)i.Id).FirstOrDefault() : null
                }).ToListAsync();
            var autores = await Consultas.UsuariosPublicosAsync(db, linhas.Select(l => l.AutorId));
            return linhas.Where(l => autores.ContainsKey(l.AutorId))
                .Select(l => new DepoimentoView(l.Id, autores[l.AutorId], l.Titulo, l.Texto, l.FotoUrl, l.PetId, l.PetNome, l.PetFotoId, l.DataCriacao))
                .ToList();
        }

        public async Task PublicarDepoimentoAsync(int autorId, string titulo, string texto, int? petId, string? fotoUrl)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            db.Depoimentos.Add(new Depoimento { AutorId = autorId, Titulo = titulo.Trim(), Texto = texto.Trim(), PetId = petId, FotoUrl = fotoUrl });
            await db.SaveChangesAsync();

            // Quem doou o pet fica sabendo da história 💛
            if (petId != null)
            {
                var pet = await db.Pets.Where(p => p.Id == petId).Select(p => new { p.UsuarioDoadorId, p.Nome }).FirstOrDefaultAsync();
                if (pet != null)
                    await _notificacoes.CriarAsync(pet.UsuarioDoadorId, autorId, "historia", $"{await NomeAsync(db, autorId)} contou como está a vida de {pet.Nome} no Final Feliz 💛", "/final-feliz");
            }
        }

        public async Task ExcluirDepoimentoAsync(int id, int autorId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await db.Depoimentos.Where(d => d.Id == id && d.AutorId == autorId).ExecuteDeleteAsync();
        }

        // Pets que a pessoa adotou (para marcar no depoimento)
        public async Task<List<Pet>> PetsAdotadosPorAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Pets.AsNoTracking().Where(p => p.AdotanteId == usuarioId).Select(p => new Pet { Id = p.Id, Nome = p.Nome }).ToListAsync();
        }
    }
}
