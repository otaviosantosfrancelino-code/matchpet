using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Rankings da página de Estatísticas: quem mais adotou, quem mais doou e ONGs mais bem avaliadas
    public class RankingService
    {
        private readonly IDbContextFactory<AppDbContext> _fabrica;
        private readonly NotificacaoService _notificacoes;

        public RankingService(IDbContextFactory<AppDbContext> fabrica, NotificacaoService notificacoes)
        {
            _fabrica = fabrica;
            _notificacoes = notificacoes;
        }

        public record Posicao(int Lugar, Usuario Usuario, int Pontos, string Detalhe);

        public static readonly (string Chave, string Rotulo)[] Periodos =
        {
            ("7d", "7 dias"), ("30d", "30 dias"), ("6m", "6 meses"), ("12m", "12 meses"), ("ano", "Este ano"), ("tudo", "Desde o início")
        };

        public static string Rotulo(string periodo) => Periodos.FirstOrDefault(p => p.Chave == periodo).Rotulo ?? "Desde o início";

        private static DateTime Inicio(string periodo)
        {
            var agora = DateTime.UtcNow;
            return periodo switch
            {
                "7d" => agora.AddDays(-7),
                "30d" => agora.AddDays(-30),
                "6m" => agora.AddMonths(-6),
                "12m" => agora.AddMonths(-12),
                "ano" => new DateTime(Consultas.HoraBrasil(agora).Year, 1, 1, 3, 0, 0, DateTimeKind.Utc),
                _ => new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc) // "desde o início"
            };
        }

        // Pessoas que mais adotaram (termos assinados / pets marcados como adotados por elas)
        public async Task<List<Posicao>> AdotantesAsync(string periodo, int top = 10)
        {
            var inicio = Inicio(periodo);
            await using var db = await _fabrica.CreateDbContextAsync();
            var grupos = await db.Pets.AsNoTracking()
                .Where(p => p.AdotanteId != null && p.DataAdocao != null && p.DataAdocao >= inicio)
                .GroupBy(p => p.AdotanteId!.Value)
                .Select(g => new { Id = g.Key, Total = g.Count(), Ultima = g.Max(p => p.DataAdocao) })
                .OrderByDescending(g => g.Total).ThenByDescending(g => g.Ultima).Take(top).ToListAsync();
            var u = await Consultas.UsuariosPublicosAsync(db, grupos.Select(g => g.Id));
            return grupos.Where(g => u.ContainsKey(g.Id))
                .Select((g, i) => new Posicao(i + 1, u[g.Id], g.Total, g.Total == 1 ? "1 pet adotado" : $"{g.Total} pets adotados")).ToList();
        }

        // Pessoas (não ONGs) que mais doaram: pets que encontraram lar + itens do Bazar entregues
        public async Task<List<Posicao>> DoadoresAsync(string periodo, int top = 10)
        {
            var inicio = Inicio(periodo);
            await using var db = await _fabrica.CreateDbContextAsync();
            var pets = await db.Pets.AsNoTracking()
                .Where(p => p.Status == "Adotado" && p.DataAdocao != null && p.DataAdocao >= inicio && !p.UsuarioDoador!.EhOng)
                .GroupBy(p => p.UsuarioDoadorId).Select(g => new { Id = g.Key, Total = g.Count() }).ToListAsync();
            var itens = await db.ItensDoacao.AsNoTracking()
                .Where(i => i.Status == "Doado" && i.DataCadastro >= inicio && !i.UsuarioDoador!.EhOng)
                .GroupBy(i => i.UsuarioDoadorId).Select(g => new { Id = g.Key, Total = g.Count() }).ToListAsync();

            var soma = pets.Select(p => p.Id).Union(itens.Select(i => i.Id))
                .Select(id => new
                {
                    Id = id,
                    Pets = pets.FirstOrDefault(p => p.Id == id)?.Total ?? 0,
                    Itens = itens.FirstOrDefault(i => i.Id == id)?.Total ?? 0
                })
                .OrderByDescending(x => x.Pets * 3 + x.Itens).ThenByDescending(x => x.Pets).Take(top).ToList();

            var u = await Consultas.UsuariosPublicosAsync(db, soma.Select(s => s.Id));
            return soma.Where(s => u.ContainsKey(s.Id))
                .Select((s, i) => new Posicao(i + 1, u[s.Id], s.Pets + s.Itens,
                    $"{s.Pets} {(s.Pets == 1 ? "pet doado" : "pets doados")} · {s.Itens} {(s.Itens == 1 ? "item" : "itens")} no Bazar")).ToList();
        }

        // ONGs mais bem avaliadas: adoções concluídas, curtidas recebidas, amigos e histórias de adotantes
        public async Task<List<Posicao>> OngsAsync(string periodo, int top = 10)
        {
            var inicio = Inicio(periodo);
            await using var db = await _fabrica.CreateDbContextAsync();
            var ongs = await db.Usuarios.AsNoTracking().Where(u => u.EhOng).Select(u => u.Id).ToListAsync();
            if (ongs.Count == 0) return new();

            var adocoes = await db.Pets.Where(p => ongs.Contains(p.UsuarioDoadorId) && p.Status == "Adotado" && p.DataAdocao >= inicio)
                .GroupBy(p => p.UsuarioDoadorId).Select(g => new { Id = g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.N);
            var curtidas = await db.Curtidas.Where(c => c.DataCriacao >= inicio)
                .Join(db.Postagens.Where(p => p.Aprovado && ongs.Contains(p.AutorId)), c => c.PostagemId, p => p.Id, (c, p) => p.AutorId)
                .GroupBy(id => id).Select(g => new { Id = g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.N);
            var amigos = await db.Amizades.Where(a => a.Status == "Aceita" && (ongs.Contains(a.SolicitanteId) || ongs.Contains(a.DestinatarioId)))
                .Select(a => ongs.Contains(a.SolicitanteId) ? a.SolicitanteId : a.DestinatarioId)
                .GroupBy(id => id).Select(g => new { Id = g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.N);
            var historias = await db.Depoimentos.Where(d => d.Aprovado && d.Pet != null && ongs.Contains(d.Pet.UsuarioDoadorId) && d.DataCriacao >= inicio)
                .GroupBy(d => d.Pet!.UsuarioDoadorId).Select(g => new { Id = g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Id, x => x.N);

            var pontuacao = ongs.Select(id =>
                {
                    int A(Dictionary<int, int> d) => d.GetValueOrDefault(id);
                    return new { Id = id, Adocoes = A(adocoes), Curtidas = A(curtidas), Amigos = A(amigos), Historias = A(historias),
                        Pontos = A(adocoes) * 10 + A(historias) * 5 + A(amigos) * 2 + A(curtidas) };
                })
                .Where(x => x.Pontos > 0).OrderByDescending(x => x.Pontos).Take(top).ToList();

            var u = await Consultas.UsuariosPublicosAsync(db, pontuacao.Select(p => p.Id));
            return pontuacao.Where(p => u.ContainsKey(p.Id))
                .Select((p, i) => new Posicao(i + 1, u[p.Id], p.Pontos,
                    $"{p.Adocoes} adoções · {p.Historias} histórias · {p.Curtidas} patinhas · {p.Amigos} amigos")).ToList();
        }

        // Depois de uma adoção/doação: se a pessoa entrou no Top 10, ela recebe uma notificação
        public async Task VerificarAsync(int usuarioId)
        {
            foreach (var periodo in new[] { "30d", "tudo" })
            {
                var listas = new (string nome, List<Posicao> lista)[]
                {
                    ("adotantes", await AdotantesAsync(periodo)),
                    ("doadores", await DoadoresAsync(periodo)),
                    ("ONGs", await OngsAsync(periodo)),
                };
                foreach (var (nome, lista) in listas)
                {
                    var minha = lista.FirstOrDefault(p => p.Usuario.Id == usuarioId);
                    if (minha == null) continue;
                    var quando = periodo == "30d" ? "dos últimos 30 dias" : "de todos os tempos";
                    var medalha = minha.Lugar switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => "🏆" };
                    await _notificacoes.CriarAsync(usuarioId, null, "ranking",
                        $"{medalha} Você está em {minha.Lugar}º lugar no ranking de {nome} {quando}!",
                        $"/estatisticas?periodo={periodo}#rankings", chave: $"ranking:{nome}:{periodo}:{minha.Lugar}");
                }
            }
        }
    }
}
