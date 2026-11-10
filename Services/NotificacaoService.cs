using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Notificações do sininho: alguém curtiu, comentou, pediu amizade, enviou/assinou termo, entrou no ranking...
    public class NotificacaoService
    {
        private readonly IDbContextFactory<AppDbContext> _fabrica;
        private readonly NotificacoesService _tempoReal;

        public NotificacaoService(IDbContextFactory<AppDbContext> fabrica, NotificacoesService tempoReal)
        {
            _fabrica = fabrica;
            _tempoReal = tempoReal;
        }

        public record Item(int Id, string Tipo, string Texto, string? Link, bool Lida, DateTime Data, Usuario? Ator);

        // "chave" evita repetir a mesma notificação (ex.: curtir, descurtir e curtir de novo)
        public async Task CriarAsync(int usuarioId, int? atorId, string tipo, string texto, string? link, string? chave = null)
        {
            if (atorId != null && atorId == usuarioId) return; // ninguém é notificado das próprias ações
            await using var db = await _fabrica.CreateDbContextAsync();
            if (chave != null && await db.Notificacoes.AnyAsync(n => n.UsuarioId == usuarioId && n.Chave == chave)) return;

            db.Notificacoes.Add(new Notificacao { UsuarioId = usuarioId, AtorId = atorId, Tipo = tipo, Texto = texto, Link = link, Chave = chave });
            await db.SaveChangesAsync();
            _tempoReal.Avisar(usuarioId, "notificacao");
        }

        public async Task<List<Item>> ListarAsync(int usuarioId, int quantidade = 40)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var lista = await db.Notificacoes.AsNoTracking().Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.DataCriacao).Take(quantidade).ToListAsync();
            var atores = await Consultas.UsuariosPublicosAsync(db, lista.Where(n => n.AtorId != null).Select(n => n.AtorId!.Value));
            return lista.Select(n => new Item(n.Id, n.Tipo, n.Texto, n.Link, n.Lida, n.DataCriacao,
                n.AtorId != null ? atores.GetValueOrDefault(n.AtorId.Value) : null)).ToList();
        }

        public async Task<int> ContarNaoLidasAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Notificacoes.CountAsync(n => n.UsuarioId == usuarioId && !n.Lida);
        }

        public async Task MarcarTodasLidasAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var n = await db.Notificacoes.Where(x => x.UsuarioId == usuarioId && !x.Lida).ExecuteUpdateAsync(s => s.SetProperty(x => x.Lida, true));
            if (n > 0) _tempoReal.Avisar(usuarioId, "notificacao");
        }

        public async Task MarcarLidaAsync(int id, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await db.Notificacoes.Where(x => x.Id == id && x.UsuarioId == usuarioId).ExecuteUpdateAsync(s => s.SetProperty(x => x.Lida, true));
            _tempoReal.Avisar(usuarioId, "notificacao");
        }

        public static string Icone(string tipo) => tipo switch
        {
            "curtida" => "🐾",
            "comentario" => "💬",
            "amizade" => "👋",
            "amizade_aceita" => "🤝",
            "termo" => "📄",
            "termo_assinado" => "✅",
            "ranking" => "🏆",
            "adocao" => "🏡",
            "historia" => "💛",
            "achado" => "🔎",
            "denuncia" => "🚩",
            "moderacao" => "🛡️",
            "documento" => "🪪",
            "acompanhamento" => "🩺",
            _ => "🔔"
        };
    }
}
