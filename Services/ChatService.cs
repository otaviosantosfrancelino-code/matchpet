using System.Security.Cryptography;
using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Conversas entre usuários (no estilo do Marketplace) e o termo de adoção enviado dentro delas.
    // Cada operação abre o próprio contexto do banco: seguro com várias mensagens chegando ao mesmo tempo.
    public class ChatService
    {
        private readonly IDbContextFactory<AppDbContext> _fabrica;
        private readonly NotificacoesService _avisos;
        private readonly NotificacaoService _notificacoes;
        private readonly RankingService _ranking;

        public ChatService(IDbContextFactory<AppDbContext> fabrica, NotificacoesService avisos, NotificacaoService notificacoes, RankingService ranking)
        {
            _fabrica = fabrica;
            _avisos = avisos;
            _notificacoes = notificacoes;
            _ranking = ranking;
        }

        public record ResumoConversa(int Id, Usuario Outro, int? PetId, string? PetNome, int? PetFotoId, string? PetStatus,
            int? ItemId, string? ItemNome, string UltimaMensagem, DateTime UltimaEm, int NaoLidas);

        public record DetalheConversa(Conversa Conversa, Usuario Eu, Usuario Outro, Pet? Pet, ItemDoacao? Item)
        {
            public bool SouDoadorDoPet => Pet != null && Pet.UsuarioDoadorId == Eu.Id;
        }

        // ---------- LISTAS ----------
        public async Task<List<ResumoConversa>> ListarAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var conversas = await db.Conversas.AsNoTracking()
                .Where(c => c.IniciadorId == usuarioId || c.DestinatarioId == usuarioId)
                .OrderByDescending(c => c.UltimaMensagemEm)
                .Select(c => new
                {
                    c.Id, c.IniciadorId, c.DestinatarioId, c.PetId, c.ItemDoacaoId, c.UltimaMensagemEm,
                    PetNome = c.Pet != null ? c.Pet.Nome : null,
                    PetStatus = c.Pet != null ? c.Pet.Status : null,
                    PetFotoId = c.Pet != null ? c.Pet.Imagens.OrderBy(i => i.Id).Select(i => (int?)i.Id).FirstOrDefault() : null,
                    ItemNome = c.ItemDoacao != null ? c.ItemDoacao.Nome : null,
                    Ultima = c.Mensagens.OrderByDescending(m => m.DataEnvio).Select(m => new { m.Texto, m.Tipo, m.RemetenteId }).FirstOrDefault(),
                    NaoLidas = c.Mensagens.Count(m => !m.Lida && m.RemetenteId != usuarioId)
                })
                .ToListAsync();

            var usuarios = await Consultas.UsuariosPublicosAsync(db, conversas.Select(c => c.IniciadorId == usuarioId ? c.DestinatarioId : c.IniciadorId));

            return conversas
                .Where(c => usuarios.ContainsKey(c.IniciadorId == usuarioId ? c.DestinatarioId : c.IniciadorId))
                .Select(c =>
                {
                    var ultima = c.Ultima == null ? "Conversa iniciada"
                        : c.Ultima.Tipo == "termo" ? "📄 Termo de adoção"
                        : c.Ultima.Tipo == "imagem" ? "📷 Foto"
                        : (c.Ultima.RemetenteId == usuarioId ? "Você: " : "") + c.Ultima.Texto;
                    return new ResumoConversa(c.Id, usuarios[c.IniciadorId == usuarioId ? c.DestinatarioId : c.IniciadorId],
                        c.PetId, c.PetNome, c.PetFotoId, c.PetStatus, c.ItemDoacaoId, c.ItemNome, ultima, c.UltimaMensagemEm, c.NaoLidas);
                })
                .ToList();
        }

        public async Task<int> ContarNaoLidasAsync(int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Mensagens.CountAsync(m => !m.Lida && m.RemetenteId != usuarioId &&
                db.Conversas.Any(c => c.Id == m.ConversaId && (c.IniciadorId == usuarioId || c.DestinatarioId == usuarioId)));
        }

        public async Task<DetalheConversa?> ObterAsync(int conversaId, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var c = await db.Conversas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == conversaId);
            if (c == null || (c.IniciadorId != usuarioId && c.DestinatarioId != usuarioId)) return null;

            var usuarios = await Consultas.UsuariosPublicosAsync(db, new[] { c.IniciadorId, c.DestinatarioId });
            var outroId = c.IniciadorId == usuarioId ? c.DestinatarioId : c.IniciadorId;

            Pet? pet = c.PetId == null ? null : await Consultas.PetsLeves(db.Pets.Where(p => p.Id == c.PetId)).FirstOrDefaultAsync();
            ItemDoacao? item = c.ItemDoacaoId == null ? null : await db.ItensDoacao.AsNoTracking()
                .Where(i => i.Id == c.ItemDoacaoId)
                .Select(i => new ItemDoacao
                {
                    Id = i.Id, Nome = i.Nome, Status = i.Status, Categoria = i.Categoria, UsuarioDoadorId = i.UsuarioDoadorId,
                    Imagens = i.Imagens.OrderBy(x => x.Id).Take(1).Select(x => new ItemImagem { Id = x.Id }).ToList()
                }).FirstOrDefaultAsync();

            return new DetalheConversa(c, usuarios[usuarioId], usuarios[outroId], pet, item);
        }

        public async Task<List<Mensagem>> MensagensAsync(int conversaId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await db.Mensagens.AsNoTracking()
                .Where(m => m.ConversaId == conversaId)
                .OrderBy(m => m.DataEnvio)
                .Include(m => m.TermoAdocao)
                .ToListAsync();
        }

        // ---------- AÇÕES ----------

        // Abre a conversa (ou reaproveita a que já existe sobre o mesmo assunto)
        public async Task<int> ObterOuCriarAsync(int deId, int paraId, int? petId = null, int? itemId = null, string? primeiraMensagem = null)
        {
            if (deId == paraId) throw new InvalidOperationException("Você não pode conversar com você mesmo.");
            await using var db = await _fabrica.CreateDbContextAsync();

            var existente = await db.Conversas.FirstOrDefaultAsync(c =>
                ((c.IniciadorId == deId && c.DestinatarioId == paraId) || (c.IniciadorId == paraId && c.DestinatarioId == deId)) &&
                c.PetId == petId && c.ItemDoacaoId == itemId);
            if (existente != null) return existente.Id;

            var conversa = new Conversa { IniciadorId = deId, DestinatarioId = paraId, PetId = petId, ItemDoacaoId = itemId };
            db.Conversas.Add(conversa);
            await db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(primeiraMensagem))
                await EnviarAsync(conversa.Id, deId, primeiraMensagem);
            return conversa.Id;
        }

        public async Task<Mensagem> EnviarAsync(int conversaId, int remetenteId, string texto, string? midiaUrl = null)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var conversa = await db.Conversas.FirstAsync(c => c.Id == conversaId);
            if (conversa.IniciadorId != remetenteId && conversa.DestinatarioId != remetenteId)
                throw new InvalidOperationException("Você não participa desta conversa.");

            var msg = new Mensagem
            {
                ConversaId = conversaId, RemetenteId = remetenteId, Texto = texto.Trim(),
                Tipo = midiaUrl != null ? "imagem" : "texto", MidiaUrl = midiaUrl
            };
            db.Mensagens.Add(msg);
            conversa.UltimaMensagemEm = msg.DataEnvio;
            await db.SaveChangesAsync();

            _avisos.Avisar(conversa.IniciadorId == remetenteId ? conversa.DestinatarioId : conversa.IniciadorId, "mensagem", conversaId);
            return msg;
        }

        public async Task<string> SalvarFotoAsync(byte[] dados, string contentType, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            return await Consultas.SalvarMidiaAsync(db, dados, contentType, usuarioId);
        }

        public async Task MarcarLidasAsync(int conversaId, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var alteradas = await db.Mensagens
                .Where(m => m.ConversaId == conversaId && m.RemetenteId != usuarioId && !m.Lida)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.Lida, true));
            if (alteradas > 0) _avisos.Avisar(usuarioId, "lidas", conversaId);
        }

        // ---------- TERMO DE ADOÇÃO ----------

        public async Task<string> PreviaTermoAsync(int conversaId, int doadorId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var (pet, doador, adotante, _) = await DadosTermoAsync(db, conversaId, doadorId);
            return GerarTexto(pet, doador, adotante);
        }

        // O doador envia o termo pelo chat (a assinatura dele fica registrada no envio)
        public async Task<TermoAdocao> EnviarTermoAsync(int conversaId, int doadorId, string ip)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var (pet, doador, adotante, conversa) = await DadosTermoAsync(db, conversaId, doadorId);

            if (await db.TermosAdocao.AnyAsync(t => t.ConversaId == conversaId && t.Status == "Aguardando assinatura"))
                throw new InvalidOperationException("Já existe um termo aguardando assinatura nesta conversa.");

            var termo = new TermoAdocao
            {
                PetId = pet.Id, DoadorId = doador.Id, AdotanteId = adotante.Id, ConversaId = conversaId,
                Texto = GerarTexto(pet, doador, adotante), IpDoador = ip,
                Codigo = "MP-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).Insert(4, "-")
            };
            db.TermosAdocao.Add(termo);
            await db.SaveChangesAsync();

            db.Mensagens.Add(new Mensagem { ConversaId = conversaId, RemetenteId = doadorId, Tipo = "termo", TermoAdocaoId = termo.Id, Texto = $"Termo de adoção de {pet.Nome}" });
            conversa.UltimaMensagemEm = DateTime.UtcNow;
            await db.Pets.Where(p => p.Id == pet.Id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, "Em Processo"));
            await db.SaveChangesAsync();

            _avisos.Avisar(adotante.Id, "mensagem", conversaId);
            await _notificacoes.CriarAsync(adotante.Id, doador.Id, "termo", $"{doador.Nome.Split(' ')[0]} enviou o termo de adoção de {pet.Nome} para você assinar.", $"/mensagens/{conversaId}");
            return termo;
        }

        // O adotante assina: o pet passa a constar como adotado por ele
        public async Task AssinarTermoAsync(int termoId, int adotanteId, string ip)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var termo = await db.TermosAdocao.FirstOrDefaultAsync(t => t.Id == termoId)
                ?? throw new InvalidOperationException("Termo não encontrado.");
            if (termo.AdotanteId != adotanteId) throw new InvalidOperationException("Só o adotante pode assinar este termo.");
            if (termo.Status != "Aguardando assinatura") throw new InvalidOperationException("Este termo não está mais aguardando assinatura.");

            var agora = DateTime.UtcNow;
            termo.Status = "Assinado";
            termo.DataAssinatura = agora;
            termo.IpAdotante = ip;

            await db.Pets.Where(p => p.Id == termo.PetId).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, "Adotado")
                .SetProperty(p => p.DataAdocao, agora)
                .SetProperty(p => p.AdotanteId, adotanteId));

            // Outros termos pendentes do mesmo pet (com outras pessoas) são cancelados
            await db.TermosAdocao.Where(t => t.PetId == termo.PetId && t.Id != termo.Id && t.Status == "Aguardando assinatura")
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, "Cancelado"));

            db.Mensagens.Add(new Mensagem { ConversaId = termo.ConversaId, RemetenteId = adotanteId, Tipo = "sistema", Texto = "✅ Termo assinado! A adoção foi concluída. Parabéns pela nova família! 💛" });
            var conversa = await db.Conversas.FirstAsync(c => c.Id == termo.ConversaId);
            conversa.UltimaMensagemEm = agora;
            await db.SaveChangesAsync();

            _avisos.Avisar(termo.DoadorId, "mensagem", termo.ConversaId);
            _avisos.Avisar(adotanteId, "mensagem", termo.ConversaId);

            var nomePet = await db.Pets.Where(p => p.Id == termo.PetId).Select(p => p.Nome).FirstOrDefaultAsync() ?? "o pet";
            var nomeAdotante = (await db.Usuarios.Where(u => u.Id == adotanteId).Select(u => u.Nome).FirstOrDefaultAsync() ?? "").Split(' ')[0];
            await _notificacoes.CriarAsync(termo.DoadorId, adotanteId, "termo_assinado", $"{nomeAdotante} assinou o termo: {nomePet} encontrou um lar! 🏡", $"/mensagens/{termo.ConversaId}");

            // Adoção conta pontos no ranking dos dois
            // (em segundo plano, para não atrasar a assinatura)
            var doadorId = termo.DoadorId;
            _ = Task.Run(async () =>
            {
                try { await _ranking.VerificarAsync(adotanteId); await _ranking.VerificarAsync(doadorId); }
                catch { /* o ranking é só um bônus: falha aqui não afeta a adoção */ }
            });
        }

        public async Task CancelarTermoAsync(int termoId, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var termo = await db.TermosAdocao.FirstOrDefaultAsync(t => t.Id == termoId)
                ?? throw new InvalidOperationException("Termo não encontrado.");
            if (termo.DoadorId != usuarioId && termo.AdotanteId != usuarioId) throw new InvalidOperationException("Sem permissão.");
            if (termo.Status != "Aguardando assinatura") throw new InvalidOperationException("Este termo não pode mais ser cancelado.");

            termo.Status = "Cancelado";
            db.Mensagens.Add(new Mensagem { ConversaId = termo.ConversaId, RemetenteId = usuarioId, Tipo = "sistema", Texto = "O termo de adoção foi cancelado." });
            // O pet volta a ficar disponível se não houver outro termo pendente
            if (!await db.TermosAdocao.AnyAsync(t => t.PetId == termo.PetId && t.Id != termo.Id && t.Status == "Aguardando assinatura"))
                await db.Pets.Where(p => p.Id == termo.PetId && p.Status == "Em Processo").ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, "Disponível"));
            await db.SaveChangesAsync();

            _avisos.Avisar(termo.DoadorId == usuarioId ? termo.AdotanteId : termo.DoadorId, "mensagem", termo.ConversaId);
        }

        // Termo completo para a página de impressão (só para as duas partes)
        public async Task<TermoAdocao?> ObterTermoAsync(int termoId, int usuarioId)
        {
            await using var db = await _fabrica.CreateDbContextAsync();
            var termo = await db.TermosAdocao.AsNoTracking().FirstOrDefaultAsync(t => t.Id == termoId);
            if (termo == null || (termo.DoadorId != usuarioId && termo.AdotanteId != usuarioId)) return null;
            var usuarios = await Consultas.UsuariosPublicosAsync(db, new[] { termo.DoadorId, termo.AdotanteId });
            termo.Doador = usuarios.GetValueOrDefault(termo.DoadorId);
            termo.Adotante = usuarios.GetValueOrDefault(termo.AdotanteId);
            termo.Pet = await db.Pets.AsNoTracking().Where(p => p.Id == termo.PetId).Select(p => new Pet { Id = p.Id, Nome = p.Nome, Especie = p.Especie, Raca = p.Raca }).FirstOrDefaultAsync();
            return termo;
        }

        private static async Task<(Pet pet, Usuario doador, Usuario adotante, Conversa conversa)> DadosTermoAsync(AppDbContext db, int conversaId, int doadorId)
        {
            var conversa = await db.Conversas.FirstOrDefaultAsync(c => c.Id == conversaId)
                ?? throw new InvalidOperationException("Conversa não encontrada.");
            if (conversa.PetId == null) throw new InvalidOperationException("O termo só pode ser enviado em conversas sobre um pet.");

            var pet = await db.Pets.AsNoTracking().FirstAsync(p => p.Id == conversa.PetId);
            if (pet.UsuarioDoadorId != doadorId) throw new InvalidOperationException("Só quem anunciou o pet pode enviar o termo.");
            if (pet.Status == "Adotado") throw new InvalidOperationException("Este pet já foi adotado.");

            var adotanteId = conversa.IniciadorId == doadorId ? conversa.DestinatarioId : conversa.IniciadorId;
            var doador = await db.Usuarios.AsNoTracking().FirstAsync(u => u.Id == doadorId);
            var adotante = await db.Usuarios.AsNoTracking().FirstAsync(u => u.Id == adotanteId);
            return (pet, doador, adotante, conversa);
        }

        internal static string GerarTexto(Pet pet, Usuario doador, Usuario adotante) => $@"TERMO DE DOAÇÃO E ADOÇÃO RESPONSÁVEL DE ANIMAL DOMÉSTICO

DOADOR(A): {doador.Nome}, inscrito(a) no {(DocumentoUtil.EhCnpj(doador.Documento) ? "CNPJ" : "CPF")} sob o nº {DocumentoUtil.Formatar(doador.Documento)}, residente/sediado(a) em {doador.Cidade}/{doador.Estado}.

ADOTANTE: {adotante.Nome}, inscrito(a) no CPF sob o nº {DocumentoUtil.Formatar(adotante.Documento)}, residente em {adotante.Logradouro}{(string.IsNullOrWhiteSpace(adotante.Numero) ? "" : ", " + adotante.Numero)} - {adotante.Bairro}, {adotante.Cidade}/{adotante.Estado}.

OBJETO: o animal de nome {pet.Nome}, espécie {pet.Especie}, raça {pet.Raca}, sexo {pet.Sexo}, porte {pet.Porte}, com aproximadamente {pet.Idade} ano(s){(pet.Castrado ? ", castrado" : "")}.

CLÁUSULA 1ª – O(A) DOADOR(A) transfere gratuitamente ao ADOTANTE a guarda e a responsabilidade pelo animal descrito acima.

CLÁUSULA 2ª – O ADOTANTE compromete-se a oferecer abrigo adequado, alimentação de qualidade, água limpa, cuidados veterinários, vacinação e vermifugação periódicas, além de afeto e convívio familiar.

CLÁUSULA 3ª – O ADOTANTE declara ciência da Lei nº 14.064/2020 (Lei Sansão), que aumenta as penas para maus-tratos a cães e gatos, e da Lei nº 9.605/1998 (Lei de Crimes Ambientais), comprometendo-se a jamais abandonar, maltratar ou permitir que o animal seja maltratado.

CLÁUSULA 4ª – O ADOTANTE não poderá vender, trocar ou repassar o animal a terceiros. Caso não possa mais mantê-lo, deverá comunicar o(a) DOADOR(A) para que uma nova adoção responsável seja buscada.

CLÁUSULA 5ª – O ADOTANTE autoriza o(a) DOADOR(A) a acompanhar a adaptação do animal por meio de contatos e, se combinado, visitas.

CLÁUSULA 6ª – Este termo é firmado eletronicamente na plataforma Match-Pet SP. As assinaturas são registradas com data, hora e endereço IP de cada parte, nos termos da MP nº 2.200-2/2001 e da Lei nº 14.063/2020 (assinatura eletrônica simples).

Termo gerado em {Consultas.HoraBrasil(DateTime.UtcNow):dd/MM/yyyy 'às' HH:mm} (horário de Brasília).";
    }
}
