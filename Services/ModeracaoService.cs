using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Moderação (RF04): aprovação prévia de conteúdo, denúncias, conferência de documentos, bloqueio de contas
    // e os números do Painel do administrador.
    public class ModeracaoService
    {
        public const string StatusRemovido = "Removido";

        private readonly IDbContextFactory<AppDbContext> _fabrica;
        private readonly NotificacaoService _notificacoes;
        private readonly CofreDocumentos _cofre;
        private readonly ILogger<ModeracaoService> _log;
        private readonly SocialService _social;

        public ModeracaoService(IDbContextFactory<AppDbContext> fabrica, NotificacaoService notificacoes, CofreDocumentos cofre, SocialService social, ILogger<ModeracaoService> log)
        {
            _social = social;
            _fabrica = fabrica;
            _notificacoes = notificacoes;
            _cofre = cofre;
            _log = log;
        }

        public static readonly string[] Motivos =
        {
            "Maus-tratos ou abandono",
            "Venda de animal",
            "Animal silvestre ou ilegal",
            "Golpe ou perfil falso",
            "Conteúdo ofensivo ou impróprio",
            "Informações falsas",
            "Spam",
            "Outro motivo",
        };

        public static string NomeTipo(string tipo) => tipo switch
        {
            "pet" => "Pet", "item" => "Item do Bazar", "achado" => "Anúncio de Achados e Perdidos", "post" => "Publicação", "comentario" => "Comentário", "usuario" => "Perfil", _ => tipo
        };

        // ===================== DENÚNCIAS =====================

        // Devolve false se a pessoa já tinha denunciado o mesmo conteúdo (denúncia ainda aberta)
        public async Task<bool> DenunciarAsync(int denuncianteId, string tipo, int alvoId, string motivo, string? detalhes)
        {
            if (!new[] { "pet", "item", "post", "comentario", "usuario", "achado" }.Contains(tipo)) throw new ArgumentException("Tipo inválido");
            if (!Motivos.Contains(motivo)) motivo = "Outro motivo";

            await using var db = await _fabrica.CreateDbContextAsync();
            if (await db.Denuncias.AnyAsync(d => d.DenuncianteId == denuncianteId && d.Tipo == tipo && d.AlvoId == alvoId && d.Status == "Aberta"))
                return false;

            db.Denuncias.Add(new Denuncia
            {
                DenuncianteId = denuncianteId, Tipo = tipo, AlvoId = alvoId, Motivo = motivo,
                Detalhes = string.IsNullOrWhiteSpace(detalhes) ? null : detalhes.Trim()[..Math.Min(detalhes.Trim().Length, 1000)]
            });
            await db.SaveChangesAsync();

            // Avisa os administradores no sininho
            var admins = await db.Usuarios.Where(u => u.EhAdmin).Select(u => u.Id).ToListAsync();
            foreach (var a in admins)
                await _notificacoes.CriarAsync(a, null, "denuncia", $"Nova denúncia: {NomeTipo(tipo).ToLower()} — {motivo}.", "/admin?aba=denuncias");
            return true;
        }

        public record DenunciaView(Denuncia Denuncia, string Denunciante, string AlvoTitulo, string? AlvoLink, int? DonoId, string? DonoNome, int TotalNoAlvo, bool AlvoExiste);

        public async Task<List<DenunciaView>> ListarDenunciasAsync(string status = "Aberta")
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var lista = await db.Denuncias.AsNoTracking()
                .Where(d => status == "Todas" || d.Status == status)
                .OrderByDescending(d => d.DataCriacao).Take(200).ToListAsync();

            var nomes = await Consultas.UsuariosPublicosAsync(db, lista.Select(d => d.DenuncianteId));
            var contagem = await db.Denuncias.Where(d => d.Status == "Aberta").GroupBy(d => new { d.Tipo, d.AlvoId })
                .Select(g => new { g.Key.Tipo, g.Key.AlvoId, Total = g.Count() }).ToListAsync();

            var resultado = new List<DenunciaView>();
            foreach (var d in lista)
            {
                var (titulo, link, donoId, existe) = await DescreverAlvoAsync(db, d.Tipo, d.AlvoId);
                string? donoNome = null;
                if (donoId != null) donoNome = await db.Usuarios.Where(u => u.Id == donoId).Select(u => u.Nome).FirstOrDefaultAsync();
                var total = contagem.FirstOrDefault(c => c.Tipo == d.Tipo && c.AlvoId == d.AlvoId)?.Total ?? 0;
                resultado.Add(new(d, nomes.GetValueOrDefault(d.DenuncianteId)?.Nome ?? "Usuário removido", titulo, link, donoId, donoNome, total, existe));
            }
            return resultado;
        }

        private static async Task<(string titulo, string? link, int? donoId, bool existe)> DescreverAlvoAsync(AppDbContext db, string tipo, int id)
        {
            switch (tipo)
            {
                case "pet":
                    var p = await db.Pets.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Nome, x.Especie, x.UsuarioDoadorId, x.Status }).FirstOrDefaultAsync();
                    return p == null ? ("Pet excluído", null, null, false) : ($"{p.Nome} ({p.Especie}) · {p.Status}", $"/pets/detalhes/{id}", p.UsuarioDoadorId, p.Status != StatusRemovido);
                case "item":
                    var i = await db.ItensDoacao.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Nome, x.UsuarioDoadorId, x.Status }).FirstOrDefaultAsync();
                    return i == null ? ("Item excluído", null, null, false) : ($"{i.Nome} · {i.Status}", "/bazar", i.UsuarioDoadorId, i.Status != StatusRemovido);
                case "achado":
                    var ap = await db.AnimaisPerdidos.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Nome, x.Especie, x.Sexo, x.Tipo, x.UsuarioId, x.Status }).FirstOrDefaultAsync();
                    return ap == null ? ("Anúncio excluído", null, null, false)
                        : ($"{new AnimalPerdido { Nome = ap.Nome, Especie = ap.Especie, Sexo = ap.Sexo, Tipo = ap.Tipo }.Titulo} · {ap.Status}", $"/achados-e-perdidos/{id}", ap.UsuarioId, ap.Status != StatusRemovido);
                case "post":
                    var po = await db.Postagens.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Texto, x.AutorId }).FirstOrDefaultAsync();
                    return po == null ? ("Publicação excluída", null, null, false) : ($"\"{ResumoTexto(po.Texto)}\"", $"/comunidade?post={id}", po.AutorId, true);
                case "comentario":
                    var c = await db.Comentarios.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Texto, x.AutorId, x.PostagemId }).FirstOrDefaultAsync();
                    return c == null ? ("Comentário excluído", null, null, false) : ($"\"{ResumoTexto(c.Texto)}\"", $"/comunidade?post={c.PostagemId}", c.AutorId, true);
                case "usuario":
                    var u = await db.Usuarios.AsNoTracking().Where(x => x.Id == id).Select(x => new { x.Nome, x.Bloqueado }).FirstOrDefaultAsync();
                    return u == null ? ("Conta excluída", null, null, false) : (u.Nome + (u.Bloqueado ? " · bloqueada" : ""), $"/u/{id}", id, !u.Bloqueado);
                default:
                    return (tipo, null, null, false);
            }
        }

        private static string ResumoTexto(string texto) => string.IsNullOrWhiteSpace(texto) ? "(sem texto, só foto/vídeo)" : texto.Length > 90 ? texto[..90] + "…" : texto;

        // acao: "remover" (tira o conteúdo do ar), "bloquear" (bloqueia o dono) ou "descartar" (denúncia improcedente)
        public async Task ResolverDenunciaAsync(int denunciaId, int adminId, string acao, string? observacao)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var d = await db.Denuncias.FirstOrDefaultAsync(x => x.Id == denunciaId) ?? throw new InvalidOperationException("Denúncia não encontrada.");
            var (titulo, _, donoId, _) = await DescreverAlvoAsync(db, d.Tipo, d.AlvoId);

            string resolucao;
            if (acao == "remover")
            {
                await RemoverConteudoAsync(db, d.Tipo, d.AlvoId);
                resolucao = "Conteúdo removido";
                if (donoId != null)
                    await _notificacoes.CriarAsync(donoId.Value, null, "moderacao",
                        $"Um conteúdo seu ({NomeTipo(d.Tipo).ToLower()}) foi removido pela moderação por: {d.Motivo.ToLower()}.", "/privacidade#termos");
            }
            else if (acao == "bloquear" && donoId != null)
            {
                await BloquearAsync(db, donoId.Value, true, $"Denúncia: {d.Motivo}");
                resolucao = "Conta do responsável bloqueada";
            }
            else resolucao = "Denúncia descartada";

            // Resolve junto as outras denúncias abertas sobre o mesmo conteúdo
            var mesmas = await db.Denuncias.Where(x => x.Tipo == d.Tipo && x.AlvoId == d.AlvoId && x.Status == "Aberta").ToListAsync();
            foreach (var m in mesmas)
            {
                m.Status = acao == "descartar" ? "Descartada" : "Resolvida";
                m.ResolvidaPorId = adminId;
                m.DataResolucao = DateTime.UtcNow;
                m.Resolucao = string.IsNullOrWhiteSpace(observacao) ? resolucao : $"{resolucao}: {observacao.Trim()}";
            }
            await db.SaveChangesAsync();

            // Quem denunciou fica sabendo do resultado
            foreach (var quem in mesmas.Select(m => m.DenuncianteId).Distinct())
                await _notificacoes.CriarAsync(quem, null, "moderacao",
                    acao == "descartar" ? "Analisamos sua denúncia e não encontramos violação dos Termos de Uso. Obrigado por ajudar!"
                                        : "Sua denúncia foi analisada e a moderação tomou providências. Obrigado por ajudar a proteger os animais! 🐾", null);
            _log.LogInformation("Denúncia {Id} resolvida pelo admin {Admin}: {Acao}", denunciaId, adminId, acao);
        }

        private static async Task RemoverConteudoAsync(AppDbContext db, string tipo, int id)
        {
            switch (tipo)
            {
                // Pets e itens não são apagados: termos de adoção assinados precisam continuar existindo (prova jurídica)
                case "pet": await db.Pets.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, StatusRemovido)); break;
                case "item": await db.ItensDoacao.Where(i => i.Id == id).ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, StatusRemovido)); break;
                case "achado": await db.AnimaisPerdidos.Where(a => a.Id == id).ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, StatusRemovido)); break;
                case "post": await db.Postagens.Where(p => p.Id == id).ExecuteDeleteAsync(); break;
                case "comentario": await db.Comentarios.Where(c => c.Id == id).ExecuteDeleteAsync(); break;
                case "usuario": await BloquearAsync(db, id, true, "Perfil removido pela moderação"); break;
            }
        }

        // ===================== USUÁRIOS =====================

        public async Task BloquearAsync(int usuarioId, bool bloquear, string? motivo)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await BloquearAsync(db, usuarioId, bloquear, motivo);
        }

        private static async Task BloquearAsync(AppDbContext db, int usuarioId, bool bloquear, string? motivo)
        {
            if (await db.Usuarios.AnyAsync(u => u.Id == usuarioId && u.EhAdmin && bloquear))
                throw new InvalidOperationException("Não é possível bloquear um administrador.");
            await db.Usuarios.Where(u => u.Id == usuarioId).ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Bloqueado, bloquear)
                .SetProperty(u => u.MotivoBloqueio, bloquear ? motivo : null));
            // Conta bloqueada: os anúncios disponíveis saem do catálogo
            if (bloquear)
                await db.Pets.Where(p => p.UsuarioDoadorId == usuarioId && (p.Status == "Disponível" || p.Status == EspeciesPermitidas.StatusModeracao))
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, StatusRemovido));
        }

        public record UsuarioAdmin(int Id, string Nome, string Email, string Documento, string Cidade, bool EhOng, bool EhAdmin, bool Bloqueado,
            string StatusDocumento, DateTime DataCadastro, int Pets, int Denuncias);

        public async Task<List<UsuarioAdmin>> BuscarUsuariosAsync(string? termo, string filtro = "todos")
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var q = db.Usuarios.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(termo))
            {
                var t = termo.Trim().ToLower();
                var dig = DocumentoUtil.SomenteDigitos(termo);
                q = q.Where(u => u.Nome.ToLower().Contains(t) || u.Email.ToLower().Contains(t) ||
                                 (dig.Length >= 3 && u.Documento.Replace(".", "").Replace("-", "").Replace("/", "").Contains(dig)));
            }
            q = filtro switch
            {
                "bloqueados" => q.Where(u => u.Bloqueado),
                "ongs" => q.Where(u => u.EhOng),
                "admins" => q.Where(u => u.EhAdmin),
                "nao-verificados" => q.Where(u => u.StatusValidacaoDocumento != "Aprovado"),
                _ => q
            };
            var lista = await q.OrderByDescending(u => u.DataCadastro).Take(60)
                .Select(u => new UsuarioAdmin(u.Id, u.Nome, u.Email, u.Documento, u.Cidade, u.EhOng, u.EhAdmin, u.Bloqueado, u.StatusValidacaoDocumento, u.DataCadastro,
                    db.Pets.Count(p => p.UsuarioDoadorId == u.Id), 0))
                .ToListAsync();

            var ids = lista.Select(u => u.Id).ToList();
            var denuncias = await db.Denuncias.Where(d => d.Tipo == "usuario" && ids.Contains(d.AlvoId))
                .GroupBy(d => d.AlvoId).Select(g => new { g.Key, Total = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Total);
            return lista.Select(u => u with { Denuncias = denuncias.GetValueOrDefault(u.Id) }).ToList();
        }

        // ===================== DOCUMENTOS =====================

        public record DocumentoPendente(int UsuarioId, string Nome, string Documento, bool EhOng, string Cidade, DateTime? Data, string? Observacao, bool TemFoto);

        public async Task<List<DocumentoPendente>> DocumentosEmAnaliseAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Usuarios.AsNoTracking()
                .Where(u => u.StatusValidacaoDocumento == "Em Análise" || u.StatusValidacaoDocumento == "Em Analise")
                .OrderBy(u => u.DataValidacaoDocumento)
                .Select(u => new DocumentoPendente(u.Id, u.Nome, u.Documento, u.EhOng, u.Cidade, u.DataValidacaoDocumento, u.DocumentoObservacao, u.FotoDocumentoBase64 != null))
                .ToListAsync();
        }

        // A imagem é aberta só na hora, para o administrador conferir (nunca vai para o navegador de outros usuários)
        public async Task<string?> AbrirDocumentoAsync(int usuarioId, int adminId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var foto = await db.Usuarios.AsNoTracking().Where(u => u.Id == usuarioId).Select(u => u.FotoDocumentoBase64).FirstOrDefaultAsync();
            _log.LogInformation("Admin {Admin} abriu o documento do usuário {Usuario}", adminId, usuarioId);
            return _cofre.Abrir(foto);
        }

        public async Task DecidirDocumentoAsync(int usuarioId, bool aprovar, string? motivo, int adminId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId) ?? throw new InvalidOperationException("Usuário não encontrado.");
            u.StatusValidacaoDocumento = aprovar ? "Aprovado" : "Reprovado";
            u.DocumentoObservacao = aprovar ? "Documento conferido manualmente pela equipe do Match-Pet."
                                            : $"Documento recusado na conferência manual{(string.IsNullOrWhiteSpace(motivo) ? "." : $": {motivo.Trim()}")}";
            u.DataValidacaoDocumento = DateTime.UtcNow;
            u.FotoDocumentoBase64 = null; // LGPD: depois da decisão a imagem é descartada (fica só o hash)
            if (!aprovar) u.DocumentoHash = null; // pode reenviar a mesma foto se foi recusada por engano
            await db.SaveChangesAsync();

            await _notificacoes.CriarAsync(usuarioId, null, "documento",
                aprovar ? "Seu documento foi conferido e aprovado! Agora você pode adotar e anunciar. ✅"
                        : "Seu documento foi recusado na conferência. Veja o motivo no seu perfil e envie outra foto.", "/perfil");
            _log.LogInformation("Admin {Admin} {Decisao} o documento do usuário {Usuario}", adminId, aprovar ? "aprovou" : "recusou", usuarioId);
        }

        // Na inicialização: fotos antigas guardadas sem criptografia são protegidas (ou apagadas se já foram decididas)
        public async Task ProtegerDocumentosAntigosAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var antigos = await db.Usuarios.Where(u => u.FotoDocumentoBase64 != null && !u.FotoDocumentoBase64.StartsWith("enc:")).ToListAsync();
            if (antigos.Count == 0) return;
            foreach (var u in antigos)
            {
                var emAnalise = u.StatusValidacaoDocumento is "Em Análise" or "Em Analise" or "Pendente";
                u.FotoDocumentoBase64 = emAnalise ? _cofre.Fechar(u.FotoDocumentoBase64!) : null;
            }
            await db.SaveChangesAsync();
            _log.LogInformation("{Total} fotos de documento antigas foram criptografadas ou descartadas", antigos.Count);
        }

        // ===================== APROVAÇÕES (moderação prévia de tudo que é público) =====================
        // Publicações, comentários, histórias, pets, itens do Bazar e foto/"sobre mim" do perfil
        // só aparecem para os outros depois que um administrador aprova.

        public record Pendente(string Tipo, int Id, int AutorId, string AutorNome, string Titulo, string? Texto,
            string? ImagemUrl, string? VideoUrl, string? Link, DateTime Data);

        public static string NomeTipoPendente(string tipo) => tipo switch
        {
            "post" => "Publicação", "comentario" => "Comentário", "historia" => "História (Final Feliz)",
            "pet" => "Anúncio de pet", "item" => "Item do Bazar", "achado" => "Achados e Perdidos", "perfil" => "Perfil (foto e textos)", _ => tipo
        };

        public async Task<int> ContarPendentesAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await ContarPendentesAsync(db);
        }

        private static async Task<int> ContarPendentesAsync(AppDbContext db) =>
            await db.Postagens.CountAsync(p => !p.Aprovado)
            + await db.Comentarios.CountAsync(c => !c.Aprovado)
            + await db.Depoimentos.CountAsync(d => !d.Aprovado)
            + await db.Pets.CountAsync(p => p.Status == EspeciesPermitidas.StatusModeracao)
            + await db.ItensDoacao.CountAsync(i => i.Status == EspeciesPermitidas.StatusModeracao)
            + await db.AnimaisPerdidos.CountAsync(a => a.Status == AchadosPerdidosService.StatusModeracao)
            + await db.Usuarios.CountAsync(u => u.FotoPerfilPendente != null || u.BioPendente != null || u.DescricaoOngPendente != null);

        public async Task<List<Pendente>> ListarPendentesAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var lista = new List<Pendente>();

            var posts = await db.Postagens.AsNoTracking().Where(p => !p.Aprovado).OrderBy(p => p.DataCriacao).Take(100)
                .Select(p => new { p.Id, p.AutorId, p.Texto, p.MidiaUrl, p.TipoMidia, p.DataCriacao }).ToListAsync();
            lista.AddRange(posts.Select(p => new Pendente("post", p.Id, p.AutorId, "", "Publicação na Comunidade", p.Texto,
                p.TipoMidia == "video" ? null : p.MidiaUrl, p.TipoMidia == "video" ? p.MidiaUrl : null, null, p.DataCriacao)));

            var comentarios = await db.Comentarios.AsNoTracking().Where(c => !c.Aprovado).OrderBy(c => c.DataCriacao).Take(200)
                .Select(c => new { c.Id, c.AutorId, c.Texto, c.PostagemId, c.DataCriacao }).ToListAsync();
            lista.AddRange(comentarios.Select(c => new Pendente("comentario", c.Id, c.AutorId, "", "Comentário", c.Texto, null, null,
                $"/comunidade?post={c.PostagemId}", c.DataCriacao)));

            var historias = await db.Depoimentos.AsNoTracking().Where(d => !d.Aprovado).OrderBy(d => d.DataCriacao).Take(100)
                .Select(d => new { d.Id, d.AutorId, d.Titulo, d.Texto, d.FotoUrl, d.DataCriacao }).ToListAsync();
            lista.AddRange(historias.Select(d => new Pendente("historia", d.Id, d.AutorId, "", $"“{d.Titulo}”", d.Texto, d.FotoUrl, null, null, d.DataCriacao)));

            var pets = await db.Pets.AsNoTracking().Where(p => p.Status == EspeciesPermitidas.StatusModeracao).OrderBy(p => p.DataCadastro).Take(100)
                .Select(p => new { p.Id, p.UsuarioDoadorId, p.Nome, p.Especie, p.Raca, p.Descricao, p.DataCadastro,
                    Foto = p.Imagens.OrderBy(i => i.Id).Select(i => (int?)i.Id).FirstOrDefault() }).ToListAsync();
            lista.AddRange(pets.Select(p => new Pendente("pet", p.Id, p.UsuarioDoadorId, "", $"{p.Nome} · {p.Especie} ({p.Raca})", p.Descricao,
                p.Foto != null ? $"/imagens/pet/{p.Foto}" : null, null, $"/pets/detalhes/{p.Id}", p.DataCadastro)));

            var itens = await db.ItensDoacao.AsNoTracking().Where(i => i.Status == EspeciesPermitidas.StatusModeracao).OrderBy(i => i.DataCadastro).Take(100)
                .Select(i => new { i.Id, i.UsuarioDoadorId, i.Nome, i.Categoria, i.Descricao, i.DataCadastro,
                    Foto = i.Imagens.OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefault() }).ToListAsync();
            lista.AddRange(itens.Select(i => new Pendente("item", i.Id, i.UsuarioDoadorId, "", $"{i.Nome} · {i.Categoria}", i.Descricao,
                i.Foto != null ? $"/imagens/item/{i.Foto}" : null, null, null, i.DataCadastro)));

            var achados = await db.AnimaisPerdidos.AsNoTracking().Where(a => a.Status == AchadosPerdidosService.StatusModeracao).OrderBy(a => a.DataCadastro).Take(100)
                .Select(a => new { a.Id, a.UsuarioId, a.Nome, a.Especie, a.Sexo, a.Tipo, a.Cor, a.Bairro, a.Cidade, a.Descricao, a.DataCadastro,
                    Foto = a.Imagens.OrderBy(i => i.Id).Select(i => (int?)i.Id).FirstOrDefault() }).ToListAsync();
            lista.AddRange(achados.Select(a => new Pendente("achado", a.Id, a.UsuarioId, "",
                $"{(a.Tipo == AnimalPerdido.TipoPerdido ? "🔴 Perdido" : "🟢 Encontrado")}: {new AnimalPerdido { Nome = a.Nome, Especie = a.Especie, Sexo = a.Sexo, Tipo = a.Tipo }.Titulo} · {a.Cor} · {a.Bairro}, {a.Cidade}",
                a.Descricao, a.Foto != null ? $"/imagens/achado/{a.Foto}" : null, null, $"/achados-e-perdidos/{a.Id}", a.DataCadastro)));

            var perfis = await db.Usuarios.AsNoTracking().Where(u => u.FotoPerfilPendente != null || u.BioPendente != null || u.DescricaoOngPendente != null).Take(100)
                .Select(u => new { u.Id, u.FotoPerfilPendente, u.BioPendente, u.DescricaoOngPendente, Data = u.DataUltimaAlteracao ?? u.DataCadastro }).ToListAsync();
            lista.AddRange(perfis.Select(u => new Pendente("perfil", u.Id, u.Id, "",
                u.FotoPerfilPendente != null ? (u.BioPendente != null || u.DescricaoOngPendente != null ? "Nova foto e textos do perfil" : "Nova foto de perfil") : "Novos textos do perfil",
                string.Join("\n\n", new[] { u.BioPendente is { } b ? $"Sobre mim: {b}" : null, u.DescricaoOngPendente is { } d ? $"Descrição da ONG: {d}" : null }.Where(t => t != null)),
                u.FotoPerfilPendente, null, $"/u/{u.Id}", u.Data)));

            var nomes = await Consultas.UsuariosPublicosAsync(db, lista.Select(p => p.AutorId));
            return lista.Select(p => p with { AutorNome = nomes.GetValueOrDefault(p.AutorId)?.Nome ?? "Usuário" })
                .OrderBy(p => p.Data).ToList();
        }

        public async Task DecidirPendenteAsync(string tipo, int id, bool aprovar, int adminId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            int? autor = null;
            string texto = "";
            string? link = null;

            switch (tipo)
            {
                case "post":
                    var post = await db.Postagens.FirstOrDefaultAsync(p => p.Id == id);
                    if (post == null) return;
                    autor = post.AutorId;
                    if (aprovar) { post.Aprovado = true; texto = "Sua publicação foi aprovada e já aparece na Comunidade! 🎉"; link = $"/comunidade?post={id}"; }
                    else { db.Postagens.Remove(post); texto = "Sua publicação não foi aprovada por não seguir os Termos de Uso."; link = "/privacidade#termos"; }
                    break;
                case "comentario":
                    var com = await db.Comentarios.FirstOrDefaultAsync(c => c.Id == id);
                    if (com == null) return;
                    autor = com.AutorId;
                    if (aprovar) com.Aprovado = true;
                    else { db.Comentarios.Remove(com); texto = "Seu comentário não foi aprovado por não seguir os Termos de Uso."; link = "/privacidade#termos"; }
                    break;
                case "historia":
                    var dep = await db.Depoimentos.FirstOrDefaultAsync(d => d.Id == id);
                    if (dep == null) return;
                    autor = dep.AutorId;
                    if (aprovar) { dep.Aprovado = true; texto = "Sua história foi aprovada e já está na galeria Final Feliz! 💛"; link = "/final-feliz"; }
                    else { db.Depoimentos.Remove(dep); texto = "Sua história não foi aprovada por não seguir os Termos de Uso."; link = "/privacidade#termos"; }
                    break;
                case "pet":
                    var pet = await db.Pets.FirstOrDefaultAsync(p => p.Id == id);
                    if (pet == null) return;
                    autor = pet.UsuarioDoadorId;
                    pet.Status = aprovar ? "Disponível" : StatusRemovido;
                    texto = aprovar ? $"O anúncio de {pet.Nome} foi aprovado e já está no catálogo! 🎉"
                                    : $"O anúncio de {pet.Nome} não foi aprovado: confira os Termos de Uso (só animais domésticos, sem venda e com fotos adequadas).";
                    link = $"/pets/detalhes/{id}";
                    break;
                case "item":
                    var item = await db.ItensDoacao.FirstOrDefaultAsync(i => i.Id == id);
                    if (item == null) return;
                    autor = item.UsuarioDoadorId;
                    item.Status = aprovar ? "Disponível" : StatusRemovido;
                    texto = aprovar ? $"Sua doação \"{item.Nome}\" foi aprovada e já está no Bazar! 🎁" : $"A doação \"{item.Nome}\" não foi aprovada por não seguir os Termos de Uso.";
                    link = aprovar ? "/bazar" : "/meus-pets?aba=itens";
                    break;
                case "achado":
                    var ach = await db.AnimaisPerdidos.FirstOrDefaultAsync(x => x.Id == id);
                    if (ach == null) return;
                    autor = ach.UsuarioId;
                    ach.Status = aprovar ? AnimalPerdido.StatusAtivo : StatusRemovido;
                    ach.UltimoLembrete = aprovar ? DateTime.UtcNow : null; // o 1º lembrete vem 2 dias depois de aparecer no catálogo
                    texto = aprovar ? $"Seu anúncio \"{ach.Titulo}\" foi aprovado e já aparece em Achados e Perdidos. Torcemos pelo reencontro! 🔎"
                                    : $"O anúncio \"{ach.Titulo}\" não foi aprovado por não seguir os Termos de Uso.";
                    link = $"/achados-e-perdidos/{id}";
                    break;
                case "perfil":
                    var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
                    if (u == null) return;
                    autor = u.Id;
                    if (aprovar)
                    {
                        if (u.FotoPerfilPendente != null) u.FotoPerfilBase64 = u.FotoPerfilPendente;
                        if (u.BioPendente != null) u.Bio = u.BioPendente;
                        if (u.DescricaoOngPendente != null) u.DescricaoOng = u.DescricaoOngPendente;
                        u.DataUltimaAlteracao = DateTime.UtcNow; // atualiza a foto guardada em cache nos navegadores
                    }
                    u.FotoPerfilPendente = null;
                    u.BioPendente = null;
                    u.DescricaoOngPendente = null;
                    texto = aprovar ? "As mudanças do seu perfil foram aprovadas! ✅" : "As mudanças do seu perfil (foto ou \"sobre mim\") não foram aprovadas.";
                    link = "/perfil";
                    break;
                default: return;
            }
            await db.SaveChangesAsync();

            // Comentário e história aprovados: agora sim avisam o dono da publicação / do pet
            if (aprovar && tipo == "comentario") await _social.AvisarComentarioAsync(id);
            if (aprovar && tipo == "historia") await _social.AvisarDepoimentoAsync(id);
            if (autor != null && texto != "") await _notificacoes.CriarAsync(autor.Value, null, "moderacao", texto, link);
            _log.LogInformation("Admin {Admin} {Decisao} {Tipo} {Id}", adminId, aprovar ? "aprovou" : "recusou", tipo, id);
        }

        // Aprova de uma vez tudo o que está na fila (útil no dia da apresentação)
        public async Task<int> AprovarTodosAsync(int adminId, string? tipo = null)
        {
            var pendentes = (await ListarPendentesAsync()).Where(p => tipo == null || p.Tipo == tipo).ToList();
            foreach (var p in pendentes) await DecidirPendenteAsync(p.Tipo, p.Id, true, adminId);
            return pendentes.Count;
        }

        // ===================== ACESSO PELO QR CODE DA BANCA =====================

        public static string HashCodigo(string codigo) =>
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(codigo.Trim())));

        // Devolve a conta ligada ao código (ou null se o código não existir ou estiver desativado)
        public async Task<Usuario?> UsuarioDoCodigoAsync(string? codigo, bool registrarUso)
        {
            if (string.IsNullOrWhiteSpace(codigo) || codigo.Length < 20 || codigo.Length > 100) return null;
            var hash = HashCodigo(codigo);
            await using var db = await _fabrica.CreateDbContextAsync();
            var acesso = await db.AcessosRapidos.AsNoTracking().FirstOrDefaultAsync(a => a.TokenHash == hash && a.Ativo);
            if (acesso == null) return null;
            var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == acesso.UsuarioId && !u.Bloqueado);
            if (usuario != null && registrarUso)
            {
                // Várias pessoas entram ao mesmo tempo: o contador soma direto no banco (nenhuma entrada se perde)
                await db.AcessosRapidos.Where(a => a.Id == acesso.Id).ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.Usos, a => a.Usos + 1)
                    .SetProperty(a => a.UltimoUso, DateTime.UtcNow));
                _log.LogInformation("Entrada pelo QR Code \"{Nome}\"", acesso.Nome);
            }
            return usuario;
        }

        public async Task<List<AcessoRapido>> AcessosRapidosAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.AcessosRapidos.AsNoTracking().OrderBy(a => a.Id).ToListAsync();
        }

        public async Task AlternarAcessoRapidoAsync(int id, bool ativo)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            await db.AcessosRapidos.Where(a => a.Id == id).ExecuteUpdateAsync(s => s.SetProperty(a => a.Ativo, ativo));
        }

        // ===================== PAINEL: GRÁFICOS E ATIVIDADE RECENTE =====================

        public record Atividade(string Icone, string Cor, string Texto, string? Link, DateTime Data);

        public record Painel(
            int Publicacoes, int Comentarios, int Historias, int Conversas, int Mensagens7Dias, int ItensDisponiveis, int ItensDoados,
            int CadastrosSemana, int AdocoesMes, double TaxaVerificacao,
            List<EstatisticasService.Barra> DocumentosPorSituacao, List<EstatisticasService.Barra> PetsPorSituacao,
            List<EstatisticasService.Barra> DenunciasPorMotivo, List<EstatisticasService.Barra> UsuariosPorCidade,
            List<EstatisticasService.Barra> PendentesPorTipo, List<EstatisticasService.Barra> ConteudoPorDia,
            List<Atividade> Recentes);

        public async Task<Painel> PainelAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var agora = DateTime.UtcNow;
            var inicio = agora.Date.AddDays(-13);

            List<EstatisticasService.Barra> Barras(IEnumerable<(string Rotulo, int Valor)> itens) =>
                itens.Where(i => i.Valor > 0).OrderByDescending(i => i.Valor).Select(i => new EstatisticasService.Barra(i.Rotulo, i.Valor)).ToList();

            var docs = await db.Usuarios.GroupBy(u => u.StatusValidacaoDocumento).Select(g => new { g.Key, N = g.Count() }).ToListAsync();
            var pets = await db.Pets.GroupBy(p => p.Status).Select(g => new { g.Key, N = g.Count() }).ToListAsync();
            var motivos = await db.Denuncias.GroupBy(d => d.Motivo).Select(g => new { g.Key, N = g.Count() }).ToListAsync();
            var cidades = await db.Usuarios.GroupBy(u => u.Cidade).Select(g => new { g.Key, N = g.Count() }).OrderByDescending(x => x.N).Take(7).ToListAsync();

            // Conteúdo enviado por dia (publicações + comentários + histórias)
            var posts = await db.Postagens.Where(p => p.DataCriacao >= inicio).Select(p => p.DataCriacao).ToListAsync();
            var coms = await db.Comentarios.Where(c => c.DataCriacao >= inicio).Select(c => c.DataCriacao).ToListAsync();
            var hist = await db.Depoimentos.Where(d => d.DataCriacao >= inicio).Select(d => d.DataCriacao).ToListAsync();
            var datas = posts.Concat(coms).Concat(hist).Select(d => d.Date).ToList();
            var porDia = Enumerable.Range(0, 14).Select(i => inicio.AddDays(i))
                .Select(d => new EstatisticasService.Barra(d.ToString("dd/MM"), datas.Count(x => x == d))).ToList();

            var pendentes = new (string, int)[]
            {
                ("Publicações", await db.Postagens.CountAsync(p => !p.Aprovado)),
                ("Comentários", await db.Comentarios.CountAsync(c => !c.Aprovado)),
                ("Histórias", await db.Depoimentos.CountAsync(d => !d.Aprovado)),
                ("Pets", await db.Pets.CountAsync(p => p.Status == EspeciesPermitidas.StatusModeracao)),
                ("Itens do Bazar", await db.ItensDoacao.CountAsync(i => i.Status == EspeciesPermitidas.StatusModeracao)),
                ("Achados e Perdidos", await db.AnimaisPerdidos.CountAsync(a => a.Status == AchadosPerdidosService.StatusModeracao)),
                ("Perfis", await db.Usuarios.CountAsync(u => u.FotoPerfilPendente != null || u.BioPendente != null || u.DescricaoOngPendente != null)),
            };

            // Atividade recente: junta os últimos acontecimentos de cada tipo
            var recentes = new List<Atividade>();
            recentes.AddRange((await db.Usuarios.OrderByDescending(u => u.DataCadastro).Take(6)
                .Select(u => new { u.Id, u.Nome, u.EhOng, u.DataCadastro }).ToListAsync())
                .Select(u => new Atividade(u.EhOng ? "bi-building-add" : "bi-person-plus", "azul", $"{u.Nome} criou {(u.EhOng ? "a conta da ONG" : "uma conta")}", $"/u/{u.Id}", u.DataCadastro)));
            recentes.AddRange((await db.TermosAdocao.Where(t => t.DataAssinatura != null).OrderByDescending(t => t.DataAssinatura).Take(4)
                .Select(t => new { t.Codigo, Pet = t.Pet!.Nome, Data = t.DataAssinatura!.Value }).ToListAsync())
                .Select(t => new Atividade("bi-file-earmark-check", "verde", $"Termo {t.Codigo} assinado: {t.Pet} foi adotado(a)", $"/verificar-termo/{t.Codigo}", t.Data)));
            recentes.AddRange((await db.Denuncias.OrderByDescending(d => d.DataCriacao).Take(4)
                .Select(d => new { d.Tipo, d.Motivo, d.Status, d.DataCriacao }).ToListAsync())
                .Select(d => new Atividade("bi-flag", "vermelho", $"Denúncia de {NomeTipo(d.Tipo).ToLower()}: {d.Motivo.ToLower()} ({d.Status.ToLower()})", "/admin?aba=denuncias", d.DataCriacao)));
            recentes.AddRange((await db.Pets.OrderByDescending(p => p.DataCadastro).Take(4)
                .Select(p => new { p.Id, p.Nome, p.Status, p.DataCadastro }).ToListAsync())
                .Select(p => new Atividade("bi-search-heart", "laranja",
                    p.Status == EspeciesPermitidas.StatusModeracao ? $"{p.Nome} foi anunciado e aguarda aprovação" : $"{p.Nome} foi anunciado", $"/pets/detalhes/{p.Id}", p.DataCadastro)));
            recentes.AddRange((await db.Postagens.OrderByDescending(p => p.DataCriacao).Take(4)
                .Select(p => new { p.Id, p.Aprovado, p.DataCriacao, Autor = db.Usuarios.Where(u => u.Id == p.AutorId).Select(u => u.Nome).FirstOrDefault() }).ToListAsync())
                .Select(p => new Atividade("bi-chat-square-text", "cinza",
                    $"{p.Autor ?? "Alguém"} publicou na Comunidade{(p.Aprovado ? "" : " (aguarda aprovação)")}", p.Aprovado ? $"/comunidade?post={p.Id}" : "/admin?aba=aprovacoes", p.DataCriacao)));

            var totalUsuarios = await db.Usuarios.CountAsync();
            return new Painel(
                await db.Postagens.CountAsync(p => p.Aprovado),
                await db.Comentarios.CountAsync(c => c.Aprovado),
                await db.Depoimentos.CountAsync(d => d.Aprovado),
                await db.Conversas.CountAsync(),
                await db.Mensagens.CountAsync(m => m.DataEnvio >= agora.AddDays(-7)),
                await db.ItensDoacao.CountAsync(i => i.Status == "Disponível"),
                await db.ItensDoacao.CountAsync(i => i.Status == "Doado"),
                await db.Usuarios.CountAsync(u => u.DataCadastro >= agora.AddDays(-7)),
                await db.Pets.CountAsync(p => p.Status == "Adotado" && p.DataAdocao >= agora.AddDays(-30)),
                totalUsuarios == 0 ? 0 : 100.0 * docs.Where(d => d.Key == "Aprovado").Sum(d => d.N) / totalUsuarios,
                Barras(docs.Select(d => (d.Key == "Em Analise" ? "Em Análise" : d.Key, d.N))),
                Barras(pets.Select(p => (p.Key == EspeciesPermitidas.StatusModeracao ? "Aguardando aprovação" : p.Key, p.N))),
                Barras(motivos.Select(m => (m.Key, m.N))),
                cidades.Select(c => new EstatisticasService.Barra(c.Key, c.N)).ToList(),
                Barras(pendentes),
                porDia,
                recentes.OrderByDescending(r => r.Data).Take(14).ToList());
        }

        // ===================== VISÃO GERAL =====================

        public record Resumo(int Usuarios, int Ongs, int Verificados, int Bloqueados, int PetsDisponiveis, int PetsAdotados, int Aprovacoes,
            int TermosAssinados, int DenunciasAbertas, int DocumentosAnalise, int Avaliacoes, double MediaSus, List<(string Rotulo, int Valor)> CadastrosPorDia);

        public async Task<Resumo> ResumoAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var inicio = DateTime.UtcNow.Date.AddDays(-13);
            var porDia = await db.Usuarios.Where(u => u.DataCadastro >= inicio)
                .GroupBy(u => u.DataCadastro.Date).Select(g => new { Dia = g.Key, Total = g.Count() }).ToListAsync();
            var serie = Enumerable.Range(0, 14).Select(i => inicio.AddDays(i))
                .Select(d => (d.ToString("dd/MM"), porDia.FirstOrDefault(x => x.Dia == d)?.Total ?? 0)).ToList();

            var notas = await db.AvaliacoesUso.Select(a => a.Pontuacao).ToListAsync();
            return new Resumo(
                await db.Usuarios.CountAsync(),
                await db.Usuarios.CountAsync(u => u.EhOng),
                await db.Usuarios.CountAsync(u => u.StatusValidacaoDocumento == "Aprovado"),
                await db.Usuarios.CountAsync(u => u.Bloqueado),
                await db.Pets.CountAsync(p => p.Status == "Disponível"),
                await db.Pets.CountAsync(p => p.Status == "Adotado"),
                await ContarPendentesAsync(db),
                await db.TermosAdocao.CountAsync(t => t.Status == "Assinado"),
                await db.Denuncias.CountAsync(d => d.Status == "Aberta"),
                await db.Usuarios.CountAsync(u => u.StatusValidacaoDocumento == "Em Análise" || u.StatusValidacaoDocumento == "Em Analise"),
                notas.Count,
                notas.Count == 0 ? 0 : notas.Average(),
                serie);
        }

        // Contador do selo no menu do administrador
        public async Task<int> PendenciasAsync()
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Denuncias.CountAsync(d => d.Status == "Aberta")
                 + await db.Usuarios.CountAsync(u => u.StatusValidacaoDocumento == "Em Análise" || u.StatusValidacaoDocumento == "Em Analise")
                 + await ContarPendentesAsync(db);
        }
    }
}
