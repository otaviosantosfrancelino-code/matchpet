using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Pets favoritos (coração). Os números dos favoritos ficam em memória durante a visita,
    // para que cada card do catálogo não precise ir ao banco.
    public class FavoritosService
    {
        private readonly IDbContextFactory<AppDbContext> _fabrica;
        private readonly AuthService _auth;
        private Task<HashSet<int>>? _ids;
        private int? _dono;

        public FavoritosService(IDbContextFactory<AppDbContext> fabrica, AuthService auth)
        {
            _fabrica = fabrica;
            _auth = auth;
        }

        public event Action? OnMudou;

        // Vários cards pedem ao mesmo tempo: todos aguardam a mesma consulta
        private Task<HashSet<int>> IdsAsync()
        {
            var eu = _auth.UsuarioLogado?.Id;
            if (eu == null) return Task.FromResult(new HashSet<int>());
            if (_ids == null || _dono != eu || _ids.IsFaulted)
            {
                _dono = eu;
                _ids = CarregarAsync(eu.Value);
            }
            return _ids;
        }

        private async Task<HashSet<int>> CarregarAsync(int eu)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return (await db.Favoritos.Where(f => f.UsuarioId == eu).Select(f => f.PetId).ToListAsync()).ToHashSet();
        }

        public async Task<bool> EhFavoritoAsync(int petId) => (await IdsAsync()).Contains(petId);
        public async Task<int> ContarAsync() => (await IdsAsync()).Count;

        // Devolve true se o pet ficou favoritado
        public async Task<bool> AlternarAsync(int petId)
        {
            var eu = _auth.UsuarioLogado?.Id ?? throw new InvalidOperationException("Entre na sua conta para salvar favoritos.");
            var ids = await IdsAsync();
            await using var db = await _fabrica.CreateDbContextAsync();
            bool agora;
            if (ids.Contains(petId))
            {
                await db.Favoritos.Where(f => f.UsuarioId == eu && f.PetId == petId).ExecuteDeleteAsync();
                ids.Remove(petId);
                agora = false;
            }
            else
            {
                if (!await db.Favoritos.AnyAsync(f => f.UsuarioId == eu && f.PetId == petId))
                {
                    db.Favoritos.Add(new Favorito { UsuarioId = eu, PetId = petId });
                    await db.SaveChangesAsync();
                }
                ids.Add(petId);
                agora = true;
            }
            OnMudou?.Invoke();
            return agora;
        }

        public async Task<List<Pet>> ListarAsync()
        {
            var eu = _auth.UsuarioLogado?.Id;
            if (eu == null) return new();
            await using var db = await _fabrica.CreateDbContextAsync();
            var ids = await db.Favoritos.Where(f => f.UsuarioId == eu).OrderByDescending(f => f.DataCriacao).Select(f => f.PetId).ToListAsync();
            var pets = await Consultas.PetsLeves(db.Pets.Where(p => ids.Contains(p.Id) && p.Status != ModeracaoService.StatusRemovido)).ToListAsync();
            return pets.OrderBy(p => ids.IndexOf(p.Id)).ToList();
        }

        // Quantas pessoas favoritaram cada pet do doador (mostrado em "Meus anúncios")
        public async Task<Dictionary<int, int>> ContarPorPetAsync(IEnumerable<int> petIds)
        {
            var lista = petIds.ToList();
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Favoritos.Where(f => lista.Contains(f.PetId)).GroupBy(f => f.PetId)
                .Select(g => new { g.Key, Total = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Total);
        }
    }

    // Questionário de adoção responsável e avaliação de usabilidade (SUS)
    public class QuestionariosService
    {
        private readonly IDbContextFactory<AppDbContext> _fabrica;
        public QuestionariosService(IDbContextFactory<AppDbContext> fabrica) => _fabrica = fabrica;

        public async Task<PerfilAdotante?> PerfilAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.PerfisAdotante.AsNoTracking().FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
        }

        public async Task SalvarPerfilAsync(int usuarioId, PerfilAdotante dados)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var atual = await db.PerfisAdotante.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
            if (atual == null) { atual = new PerfilAdotante { UsuarioId = usuarioId }; db.PerfisAdotante.Add(atual); }

            atual.Moradia = dados.Moradia;
            atual.MoradiaPropria = dados.MoradiaPropria;
            atual.ProprietarioPermite = dados.MoradiaPropria || dados.ProprietarioPermite;
            atual.TemQuintal = dados.TemQuintal;
            atual.TelasDeProtecao = dados.TelasDeProtecao;
            atual.TemCriancas = dados.TemCriancas;
            atual.TemOutrosAnimais = dados.TemOutrosAnimais;
            atual.JaTevePets = dados.JaTevePets;
            atual.TodosConcordam = dados.TodosConcordam;
            atual.CienteDosCustos = dados.CienteDosCustos;
            atual.AceitaAcompanhamento = dados.AceitaAcompanhamento;
            atual.OutrosAnimais = dados.TemOutrosAnimais ? dados.OutrosAnimais?.Trim() : null;
            atual.Motivacao = dados.Motivacao?.Trim();
            atual.HorasSozinho = Math.Clamp(dados.HorasSozinho, 0, 24);
            atual.DataAtualizacao = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        public async Task<double> RegistrarAvaliacaoAsync(int? usuarioId, string perfil, IReadOnlyList<int> notas, string? comentario)
        {
            var pontuacao = Sus.Calcular(notas);
            await using var db = await _fabrica.CreateDbContextAsync();
            db.AvaliacoesUso.Add(new AvaliacaoUso
            {
                UsuarioId = usuarioId,
                Perfil = perfil,
                Respostas = string.Join(",", notas),
                Pontuacao = pontuacao,
                Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim()[..Math.Min(comentario.Trim().Length, 800)]
            });
            await db.SaveChangesAsync();
            return pontuacao;
        }

        public async Task<bool> JaAvaliouAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.AvaliacoesUso.AnyAsync(a => a.UsuarioId == usuarioId);
        }

        public record ResultadoSus(int Total, double Media, double[] MediaPorPergunta, Dictionary<string, (int total, double media)> PorPerfil,
            Dictionary<string, int> PorFaixa, List<(DateTime data, string perfil, double nota, string comentario)> Comentarios);

        public async Task<ResultadoSus> ResultadoAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var todas = await db.AvaliacoesUso.AsNoTracking().OrderByDescending(a => a.DataCriacao).ToListAsync();
            var notas = todas.Select(a => a.Respostas.Split(',').Select(int.Parse).ToArray()).Where(n => n.Length == 10).ToList();
            var porPergunta = Enumerable.Range(0, 10).Select(i => notas.Count == 0 ? 0 : notas.Average(n => n[i])).ToArray();
            var porPerfil = todas.GroupBy(a => a.Perfil).ToDictionary(g => g.Key, g => (g.Count(), g.Average(a => a.Pontuacao)));
            var faixas = new[] { "Excelente", "Bom", "Razoável", "Ruim", "Inaceitável" }
                .ToDictionary(f => f, f => todas.Count(a => Sus.Classificar(a.Pontuacao) == f));
            var comentarios = todas.Where(a => a.Comentario != null).Take(30).Select(a => (a.DataCriacao, a.Perfil, a.Pontuacao, a.Comentario!)).ToList();
            return new(todas.Count, todas.Count == 0 ? 0 : todas.Average(a => a.Pontuacao), porPergunta, porPerfil, faixas, comentarios);
        }
    }
}
