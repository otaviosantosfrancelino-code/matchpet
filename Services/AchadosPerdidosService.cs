using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Achados e Perdidos: catálogo separado de pets perdidos e de pets encontrados que parecem ter dono.
    //  - Todo anúncio passa pela aprovação da moderação (menos os de administradores).
    //  - A cada 2 dias quem anunciou recebe um lembrete perguntando se o pet já foi encontrado.
    //  - Quando é encontrado, o anúncio vira ENCONTRADO e uma história vai para o Final Feliz.
    public class AchadosPerdidosService
    {
        public const string StatusModeracao = "Em Moderação";
        public static readonly TimeSpan IntervaloLembrete = TimeSpan.FromDays(2);

        public static readonly string[] Cores =
            { "Preto", "Branco", "Caramelo", "Marrom", "Cinza", "Rajado", "Tricolor", "Preto e branco", "Bege", "Laranja", "Malhado" };

        private readonly IDbContextFactory<AppDbContext> _fabrica;
        private readonly NotificacaoService _notificacoes;

        public AchadosPerdidosService(IDbContextFactory<AppDbContext> fabrica, NotificacaoService notificacoes)
        {
            _fabrica = fabrica;
            _notificacoes = notificacoes;
        }

        public class Filtro
        {
            public string? Tipo { get; set; }      // Perdido, Encontrado
            public string? Especie { get; set; }   // Cão, Gato, Outros
            public string? Porte { get; set; }
            public string? Sexo { get; set; }
            public string? Cor { get; set; }
            public string? Cidade { get; set; }
            public string? Texto { get; set; }
            public bool MostrarReencontrados { get; set; }
            public string Ordem { get; set; } = "recentes"; // recentes, antigos
        }

        // Versão leve para listas: só o número da 1ª foto, sem a imagem
        private static IQueryable<AnimalPerdido> Leve(IQueryable<AnimalPerdido> q) => q.AsNoTracking().Select(a => new AnimalPerdido
        {
            Id = a.Id, Tipo = a.Tipo, Nome = a.Nome, Especie = a.Especie, Raca = a.Raca, Porte = a.Porte, Sexo = a.Sexo, Cor = a.Cor,
            UsaColeira = a.UsaColeira, SinaisParticulares = a.SinaisParticulares, Descricao = a.Descricao, Bairro = a.Bairro, Cidade = a.Cidade, Estado = a.Estado,
            DataOcorrido = a.DataOcorrido, UsuarioId = a.UsuarioId, Status = a.Status, DataCadastro = a.DataCadastro, DataResolucao = a.DataResolucao,
            Imagens = a.Imagens.OrderBy(i => i.Id).Take(1).Select(i => new AnimalPerdidoImagem { Id = i.Id, AnimalPerdidoId = i.AnimalPerdidoId }).ToList()
        });

        public async Task<List<AnimalPerdido>> BuscarAsync(Filtro f)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var q = db.AnimaisPerdidos.Where(a => a.Status == AnimalPerdido.StatusAtivo || (f.MostrarReencontrados && a.Status == AnimalPerdido.StatusResolvido));
            if (!string.IsNullOrEmpty(f.Tipo)) q = q.Where(a => a.Tipo == f.Tipo);
            if (!string.IsNullOrEmpty(f.Especie))
                q = f.Especie == "Outros" ? q.Where(a => a.Especie != "Cão" && a.Especie != "Gato") : q.Where(a => a.Especie == f.Especie);
            if (!string.IsNullOrEmpty(f.Porte)) q = q.Where(a => a.Porte == f.Porte);
            if (!string.IsNullOrEmpty(f.Sexo)) q = q.Where(a => a.Sexo == f.Sexo);
            if (!string.IsNullOrEmpty(f.Cor)) q = q.Where(a => a.Cor == f.Cor);
            if (!string.IsNullOrWhiteSpace(f.Cidade)) { var c = f.Cidade.Trim().ToLower(); q = q.Where(a => a.Cidade.ToLower().Contains(c)); }
            if (!string.IsNullOrWhiteSpace(f.Texto))
            {
                var t = f.Texto.Trim().ToLower();
                q = q.Where(a => (a.Nome != null && a.Nome.ToLower().Contains(t)) || a.Descricao.ToLower().Contains(t) || a.Bairro.ToLower().Contains(t)
                                 || (a.Raca != null && a.Raca.ToLower().Contains(t)) || (a.SinaisParticulares != null && a.SinaisParticulares.ToLower().Contains(t)));
            }
            q = f.Ordem == "antigos" ? q.OrderBy(a => a.DataOcorrido) : q.OrderByDescending(a => a.DataOcorrido);
            return await Leve(q).Take(200).ToListAsync();
        }

        public async Task<List<string>> CidadesAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.AnimaisPerdidos.Where(a => a.Status == AnimalPerdido.StatusAtivo).Select(a => a.Cidade).Distinct().OrderBy(c => c).ToListAsync();
        }

        public async Task<(int perdidos, int encontrados, int reencontros)> NumerosAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return (await db.AnimaisPerdidos.CountAsync(a => a.Status == AnimalPerdido.StatusAtivo && a.Tipo == AnimalPerdido.TipoPerdido),
                    await db.AnimaisPerdidos.CountAsync(a => a.Status == AnimalPerdido.StatusAtivo && a.Tipo == AnimalPerdido.TipoEncontrado),
                    await db.AnimaisPerdidos.CountAsync(a => a.Status == AnimalPerdido.StatusResolvido));
        }

        // Anúncio completo (fotos só pelo número) + quem anunciou (dados públicos)
        public async Task<(AnimalPerdido anuncio, Usuario? autor)?> ObterAsync(int id)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var a = await db.AnimaisPerdidos.AsNoTracking().Where(x => x.Id == id).Select(x => new AnimalPerdido
            {
                Id = x.Id, Tipo = x.Tipo, Nome = x.Nome, Especie = x.Especie, Raca = x.Raca, Porte = x.Porte, Sexo = x.Sexo, Cor = x.Cor,
                UsaColeira = x.UsaColeira, SinaisParticulares = x.SinaisParticulares, Descricao = x.Descricao, Bairro = x.Bairro, Cidade = x.Cidade, Estado = x.Estado,
                DataOcorrido = x.DataOcorrido, UsuarioId = x.UsuarioId, Status = x.Status, DataCadastro = x.DataCadastro, DataResolucao = x.DataResolucao,
                Imagens = x.Imagens.OrderBy(i => i.Id).Select(i => new AnimalPerdidoImagem { Id = i.Id, AnimalPerdidoId = i.AnimalPerdidoId }).ToList()
            }).FirstOrDefaultAsync();
            if (a == null) return null;
            var autor = (await Consultas.UsuariosPublicosAsync(db, new[] { a.UsuarioId })).GetValueOrDefault(a.UsuarioId);
            return (a, autor);
        }

        public async Task<List<AnimalPerdido>> MeusAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await Leve(db.AnimaisPerdidos.Where(a => a.UsuarioId == usuarioId).OrderByDescending(a => a.DataCadastro)).ToListAsync();
        }

        // Devolve true se já ficou visível (administrador) ou false se aguarda aprovação
        public async Task<bool> PublicarAsync(AnimalPerdido anuncio, IEnumerable<string> fotos, Usuario autor)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var direto = Consultas.PublicaDireto(autor);
            anuncio.Id = 0;
            anuncio.UsuarioId = autor.Id;
            anuncio.Nome = string.IsNullOrWhiteSpace(anuncio.Nome) ? null : anuncio.Nome.Trim();
            anuncio.Status = direto ? AnimalPerdido.StatusAtivo : StatusModeracao;
            anuncio.DataCadastro = DateTime.UtcNow;
            anuncio.DataOcorrido = DateTime.SpecifyKind(anuncio.DataOcorrido.Date.AddHours(12), DateTimeKind.Utc);
            anuncio.UltimoLembrete = null;
            anuncio.Imagens = fotos.Select(f => new AnimalPerdidoImagem { Base64Data = f }).ToList();
            db.AnimaisPerdidos.Add(anuncio);
            await db.SaveChangesAsync();
            return direto;
        }

        public async Task ExcluirAsync(int id, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await db.AnimaisPerdidos.Where(a => a.Id == id && a.UsuarioId == usuarioId).ExecuteDeleteAsync();
        }

        // "Ainda não foi encontrado": o próximo lembrete vem daqui a 2 dias
        public async Task AindaProcurandoAsync(int id, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await db.AnimaisPerdidos.Where(a => a.Id == id && a.UsuarioId == usuarioId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.UltimoLembrete, DateTime.UtcNow));
        }

        public static string TituloHistoria(AnimalPerdido a)
        {
            var nome = string.IsNullOrWhiteSpace(a.Nome) ? (a.Sexo == "Fêmea" ? "A pet" : "O pet") : a.Nome;
            return a.Tipo == AnimalPerdido.TipoPerdido
                ? $"{nome} foi encontrad{(a.Sexo == "Fêmea" ? "a" : "o")} através do Match-Pet"
                : $"{nome} voltou para a família através do Match-Pet";
        }

        // Pet encontrado: o anúncio vira ENCONTRADO e a história vai para o Final Feliz (com aprovação da moderação).
        // Devolve true se a história já ficou visível.
        public async Task<bool> MarcarEncontradoAsync(int id, Usuario autor, string agradecimento, string? fotoUrl)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var a = await db.AnimaisPerdidos.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == autor.Id)
                    ?? throw new InvalidOperationException("Anúncio não encontrado.");
            a.Status = AnimalPerdido.StatusResolvido;
            a.DataResolucao = DateTime.UtcNow;

            var foto = fotoUrl ?? await db.AnimaisPerdidosImagens.Where(i => i.AnimalPerdidoId == id).OrderBy(i => i.Id)
                .Select(i => "/imagens/achado/" + i.Id).FirstOrDefaultAsync();
            var direto = Consultas.PublicaDireto(autor);
            var texto = string.IsNullOrWhiteSpace(agradecimento)
                ? "Obrigado a todos que compartilharam e ajudaram na busca! 💛"
                : agradecimento.Trim()[..Math.Min(agradecimento.Trim().Length, 2500)];
            var historia = new Depoimento
            {
                AutorId = autor.Id, AnimalPerdidoId = id, Titulo = TituloHistoria(a)[..Math.Min(TituloHistoria(a).Length, 120)],
                Texto = texto, FotoUrl = foto, Aprovado = direto
            };
            db.Depoimentos.Add(historia);
            await db.SaveChangesAsync();

            // Quem conversou sobre o anúncio fica sabendo do final feliz
            var interessados = await db.Conversas.Where(c => c.AnimalPerdidoId == id)
                .Select(c => c.IniciadorId == autor.Id ? c.DestinatarioId : c.IniciadorId).Distinct().ToListAsync();
            foreach (var u in interessados)
                await _notificacoes.CriarAsync(u, autor.Id, "achado", $"Boa notícia: {a.Titulo} foi encontrado! Obrigado por ajudar. 💛", $"/achados-e-perdidos/{id}", $"achado-resolvido:{id}:{u}");
            return direto;
        }

        // Rodado pelo serviço em segundo plano: a cada 2 dias pergunta a quem anunciou se o pet já foi encontrado
        public async Task<int> EnviarLembretesAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var limite = DateTime.UtcNow - IntervaloLembrete;
            var pendentes = await db.AnimaisPerdidos
                .Where(a => a.Status == AnimalPerdido.StatusAtivo && (a.UltimoLembrete ?? a.DataCadastro) <= limite)
                .Take(200).ToListAsync();
            foreach (var a in pendentes)
            {
                var texto = a.Tipo == AnimalPerdido.TipoPerdido
                    ? $"{a.Titulo} já foi encontrado? Toque aqui para responder. Se sim, vamos celebrar no Final Feliz! 🐾"
                    : $"Você encontrou a família de {a.Titulo}? Toque aqui para responder. 🐾";
                await _notificacoes.CriarAsync(a.UsuarioId, null, "achado", texto, $"/achados-e-perdidos/{a.Id}/encontrado",
                    $"lembrete-achado:{a.Id}:{DateTime.UtcNow:yyyyMMdd}");
                a.UltimoLembrete = DateTime.UtcNow;
            }
            await db.SaveChangesAsync();
            return pendentes.Count;
        }
    }
}
