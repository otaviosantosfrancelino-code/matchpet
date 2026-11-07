using crudcomdb.Data;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Números da página de Estatísticas (calculados a partir dos dados do site)
    public class EstatisticasService
    {
        private readonly IDbContextFactory<AppDbContext> _fabrica;
        public EstatisticasService(IDbContextFactory<AppDbContext> fabrica) => _fabrica = fabrica;

        public record Barra(string Rotulo, int Valor);

        public class Resumo
        {
            public int Adotados, Disponiveis, EmProcesso, Usuarios, Ongs, ItensDoados, ItensDisponiveis, Conversas, Postagens, Depoimentos;
            public double? DiasMedioAteAdocao;
            public List<Barra> AdocoesPorCidade = new();
            public List<Barra> DisponiveisPorCidade = new();
            public List<Barra> AdocoesPorEspecie = new();
            public List<Barra> AdocoesPorPorte = new();
            public List<Barra> AdocoesPorMes = new();
            public List<Barra> AdocoesPorIdade = new();
            public List<Barra> OngsQueMaisDoaram = new();
            public List<Barra> ItensPorCategoria = new();
        }

        public async Task<Resumo> CalcularAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();

            // Poucos campos de cada pet: o resto é calculado aqui mesmo
            var pets = await db.Pets.AsNoTracking().Select(p => new
            {
                p.Status, p.Especie, p.Porte, p.Idade, p.DataCadastro, p.DataAdocao,
                Cidade = p.UsuarioDoador != null ? p.UsuarioDoador.Cidade : "",
                Doador = p.UsuarioDoador != null ? p.UsuarioDoador.Nome : "",
                EhOng = p.UsuarioDoador != null && p.UsuarioDoador.EhOng
            }).ToListAsync();

            var adotados = pets.Where(p => p.Status == "Adotado").ToList();
            var r = new Resumo
            {
                Adotados = adotados.Count,
                Disponiveis = pets.Count(p => p.Status == "Disponível"),
                EmProcesso = pets.Count(p => p.Status == "Em Processo"),
                Usuarios = await db.Usuarios.CountAsync(),
                Ongs = await db.Usuarios.CountAsync(u => u.EhOng),
                ItensDoados = await db.ItensDoacao.CountAsync(i => i.Status == "Doado"),
                ItensDisponiveis = await db.ItensDoacao.CountAsync(i => i.Status == "Disponível"),
                Conversas = await db.Conversas.CountAsync(),
                Postagens = await db.Postagens.CountAsync(p => p.Aprovado),
                Depoimentos = await db.Depoimentos.CountAsync(d => d.Aprovado)
            };

            var tempos = adotados.Where(p => p.DataAdocao != null && p.DataAdocao > p.DataCadastro)
                .Select(p => (p.DataAdocao!.Value - p.DataCadastro).TotalDays).ToList();
            r.DiasMedioAteAdocao = tempos.Count > 0 ? Math.Round(tempos.Average(), 1) : null;

            static List<Barra> Agrupar<T>(IEnumerable<T> itens, Func<T, string> chave, int top = 10) => itens
                .GroupBy(chave).Select(g => new Barra(string.IsNullOrWhiteSpace(g.Key) ? "Não informado" : g.Key, g.Count()))
                .OrderByDescending(b => b.Valor).ThenBy(b => b.Rotulo).Take(top).ToList();

            r.AdocoesPorCidade = Agrupar(adotados, p => p.Cidade);
            r.DisponiveisPorCidade = Agrupar(pets.Where(p => p.Status == "Disponível"), p => p.Cidade);
            r.AdocoesPorEspecie = Agrupar(adotados, p => p.Especie, 6);
            r.AdocoesPorPorte = new[] { "Pequeno", "Médio", "Grande" }
                .Select(porte => new Barra(porte, adotados.Count(p => p.Porte == porte))).ToList();
            r.AdocoesPorIdade = new List<Barra>
            {
                new("Filhote (até 1)", adotados.Count(p => p.Idade <= 1)),
                new("Jovem (2–4)", adotados.Count(p => p.Idade is >= 2 and <= 4)),
                new("Adulto (5–8)", adotados.Count(p => p.Idade is >= 5 and <= 8)),
                new("Idoso (9+)", adotados.Count(p => p.Idade >= 9)),
            };
            r.OngsQueMaisDoaram = Agrupar(adotados.Where(p => p.EhOng), p => p.Doador, 5);

            // Últimos 12 meses (horário de Brasília)
            var hoje = Consultas.HoraBrasil(DateTime.UtcNow);
            var meses = Enumerable.Range(0, 12).Select(i => new DateTime(hoje.Year, hoje.Month, 1).AddMonths(-11 + i)).ToList();
            var nomes = new[] { "jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez" };
            r.AdocoesPorMes = meses.Select(m => new Barra($"{nomes[m.Month - 1]}/{m:yy}",
                adotados.Count(p => p.DataAdocao is { } d && Consultas.HoraBrasil(d) is var l && l.Year == m.Year && l.Month == m.Month))).ToList();

            r.ItensPorCategoria = (await db.ItensDoacao.AsNoTracking().GroupBy(i => i.Categoria)
                    .Select(g => new { g.Key, Total = g.Count() }).ToListAsync())
                .Select(g => new Barra(g.Key, g.Total)).OrderByDescending(b => b.Valor).ToList();

            return r;
        }
    }
}
