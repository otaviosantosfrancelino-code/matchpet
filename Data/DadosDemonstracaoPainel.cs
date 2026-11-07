using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Data
{
    // Quarta leva de demonstração: deixa o Painel do administrador "vivo" para a apresentação.
    //  - 32 cadastros novos espalhados pelos últimos 14 dias (gráfico de cadastros), com documentos
    //    aprovados, pendentes, em análise e recusados, ONGs e 2 contas bloqueadas;
    //  - fila de aprovações com publicações, comentários, histórias, pets, itens e perfis
    //    (inclui exemplos que devem ser RECUSADOS: venda de filhotes, papagaio silvestre e spam);
    //  - denúncias abertas, resolvidas e descartadas.
    // Roda uma única vez (marcador: conta rafaela.moura@demo...).
    public static class DadosDemonstracaoPainel
    {
        private const string D = DadosDemonstracao.DominioDemo;
        private const string Dog = "https://images.dog.ceo/breeds/";
        private const string Cat = "https://cdn2.thecatapi.com/images/";
        private static string Mulher(int n) => $"https://randomuser.me/api/portraits/women/{n}.jpg";
        private static string Homem(int n) => $"https://randomuser.me/api/portraits/men/{n}.jpg";

        public static async Task CriarAsync(AppDbContext db, ILogger log)
        {
            if (await db.Usuarios.AnyAsync(u => u.Email == "rafaela.moura" + D)) return;
            log.LogInformation("Criando dados de demonstração do painel do administrador...");

            var rnd = new Random(2026);
            var agora = DateTime.UtcNow;
            var senha = SenhaService.GerarHash(Guid.NewGuid().ToString("N")); // contas sem login (só para o painel)
            var adminId = await db.Usuarios.Where(u => u.EhAdmin && !u.ContaDemonstracao).Select(u => (int?)u.Id).FirstOrDefaultAsync();

            // ---------- CADASTROS ----------
            // (nome, e-mail, cidade, mulher?, foto, situação do documento, ONG?)
            var pessoas = new (string Nome, string Email, string Cidade, bool Mulher, int Foto, string Doc, bool Ong)[]
            {
                ("Rafaela Moura", "rafaela.moura", "Franca", true, 51, "Aprovado", false),
                ("Diego Cardoso", "diego.cardoso", "Franca", false, 52, "Aprovado", false),
                ("Letícia Prado", "leticia.prado", "Ribeirão Preto", true, 53, "Pendente", false),
                ("Murilo Teixeira", "murilo.teixeira", "Batatais", false, 54, "Em Análise", false),
                ("Camila Rezende", "camila.rezende", "Franca", true, 55, "Aprovado", false),
                ("Vinícius Lopes", "vinicius.lopes", "Restinga", false, 56, "Pendente", false),
                ("Bianca Nogueira", "bianca.nogueira", "Patrocínio Paulista", true, 57, "Aprovado", false),
                ("Henrique Barros", "henrique.barros", "Franca", false, 58, "Reprovado", false),
                ("Natália Freitas", "natalia.freitas", "Cristais Paulista", true, 59, "Em Análise", false),
                ("Gustavo Peixoto", "gustavo.peixoto", "Franca", false, 60, "Aprovado", false),
                ("Isabela Campos", "isabela.campos", "Ribeirão Preto", true, 61, "Pendente", false),
                ("Rodrigo Siqueira", "rodrigo.siqueira", "Pedregulho", false, 62, "Aprovado", false),
                ("Amanda Vieira", "amanda.vieira", "Franca", true, 63, "Aprovado", false),
                ("Felipe Andrade", "felipe.andrade", "Batatais", false, 64, "Pendente", false),
                ("Larissa Gomes", "larissa.gomes", "Franca", true, 65, "Em Análise", false),
                ("Eduardo Pires", "eduardo.pires", "Ituverava", false, 66, "Aprovado", false),
                ("Sabrina Costa", "sabrina.costa", "Franca", true, 67, "Reprovado", false),
                ("Leonardo Ramos", "leonardo.ramos", "Ribeirão Preto", false, 68, "Pendente", false),
                ("Priscila Antunes", "priscila.antunes", "Franca", true, 69, "Aprovado", false),
                ("Caio Fernandes", "caio.fernandes", "Restinga", false, 70, "Em Análise", false),
                ("Tatiane Moraes", "tatiane.moraes", "Franca", true, 71, "Aprovado", false),
                ("Bruno Castilho", "bruno.castilho", "Batatais", false, 72, "Pendente", false),
                ("Jéssica Rocha", "jessica.rocha", "Franca", true, 73, "Aprovado", false),
                ("Marcelo Duarte", "marcelo.duarte", "Patrocínio Paulista", false, 74, "Pendente", false),
                ("Vanessa Lima", "vanessa.lima", "Franca", true, 75, "Em Análise", false),
                ("Otávio Rangel", "otavio.rangel", "Franca", false, 76, "Aprovado", false),
                ("Aline Bastos", "aline.bastos", "Ribeirão Preto", true, 77, "Reprovado", false),
                ("Thiago Monteiro", "thiago.monteiro", "Franca", false, 78, "Aprovado", false),
                ("Instituto Quatro Patas Ribeirão", "quatropatas.rp", "Ribeirão Preto", false, 0, "Aprovado", true),
                ("Projeto Lar Temporário Batatais", "lartemporario.batatais", "Batatais", false, 0, "Em Análise", true),
                // Contas bloqueadas pela moderação
                ("Carlos Henrique Vendas", "carlos.vendas", "Franca", false, 79, "Pendente", false),
                ("Perfil Suspeito Promoções", "promocoes.pet", "Franca", false, 0, "Pendente", false),
            };

            var observacoes = new Dictionary<string, string>
            {
                ["Em Análise"] = "Leitura automática inconclusiva: a foto está com reflexo sobre o número do documento. Aguarda conferência manual.",
                ["Reprovado"] = "O nome no documento não confere com o nome do cadastro.",
                ["Aprovado"] = "RG verificado: nome e número conferem com o cadastro.",
                ["Pendente"] = "",
            };

            var usuarios = new List<Usuario>();
            for (var i = 0; i < pessoas.Length; i++)
            {
                var p = pessoas[i];
                var cadastro = agora.AddDays(-rnd.Next(0, 14)).AddHours(-rnd.Next(0, 20)).AddMinutes(-rnd.Next(0, 59));
                var u = new Usuario
                {
                    Nome = p.Nome, Email = p.Email + D,
                    Documento = p.Ong ? DadosDemonstracao.GerarCnpj($"6{i:D2}53047000{i % 10}") : DadosDemonstracao.GerarCpf($"7{i:D2}48163{i % 10}"),
                    Telefone = $"(16) 9{rnd.Next(1000, 9999)}-{rnd.Next(1000, 9999)}", Senha = senha,
                    CEP = "14400-000", Logradouro = "Rua de Demonstração", Numero = rnd.Next(10, 999).ToString(), Bairro = "Centro",
                    Cidade = p.Cidade, Estado = "SP", EhOng = p.Ong,
                    DescricaoOng = p.Ong ? "Resgatamos e cuidamos de cães e gatos até encontrarem uma família responsável." : null,
                    ChavePix = p.Ong ? p.Email + D : null,
                    FotoPerfilBase64 = p.Foto > 0 ? (p.Mulher ? Mulher(p.Foto) : Homem(p.Foto)) : null,
                    StatusValidacaoDocumento = p.Doc, DocumentoObservacao = observacoes[p.Doc] == "" ? null : observacoes[p.Doc],
                    DataValidacaoDocumento = p.Doc == "Pendente" ? null : cadastro.AddHours(2),
                    Classificacao = p.Doc == "Aprovado" ? (p.Ong ? "ONG Verificada" : "Novo Usuário Verificado") : "Novo Usuário",
                    EmailConfirmado = true, CodigoConfirmacao = "ISENTO", AceitouTermos = true, DataAceiteTermos = cadastro, IpAceite = "seed",
                    DataCadastro = cadastro,
                };
                if (p.Email is "carlos.vendas" or "promocoes.pet")
                {
                    u.Bloqueado = true;
                    u.MotivoBloqueio = p.Email == "carlos.vendas" ? "Denúncia: Venda de animal" : "Denúncia: Golpe ou perfil falso";
                }
                usuarios.Add(u);
            }
            db.Usuarios.AddRange(usuarios);
            await db.SaveChangesAsync();
            Usuario U(string email) => usuarios.First(u => u.Email == email + D);

            // ---------- FILA DE APROVAÇÕES ----------
            var postsAprovados = await db.Postagens.Where(p => p.Aprovado).OrderByDescending(p => p.Id).Select(p => p.Id).Take(30).ToListAsync();

            DateTime Recente() => agora.AddHours(-rnd.Next(1, 46)).AddMinutes(-rnd.Next(0, 59));

            db.Postagens.AddRange(
                new Postagem { AutorId = U("rafaela.moura").Id, Aprovado = false, DataCriacao = Recente(),
                    Texto = "Adotei a Luna aqui pelo Match-Pet há 2 semanas e ela já é dona da casa! Obrigada à ONG pelo carinho 💛", MidiaUrl = Cat + "MTgzOTAwMg.jpg", TipoMidia = "imagem" },
                new Postagem { AutorId = U("diego.cardoso").Id, Aprovado = false, DataCriacao = Recente(),
                    Texto = "Alguém sabe de feira de adoção em Franca neste fim de semana? Quero levar meus filhos para conhecer 🐶" },
                new Postagem { AutorId = U("camila.rezende").Id, Aprovado = false, DataCriacao = Recente(),
                    Texto = "Primeiro banho do Tobias depois do resgate. Olha a diferença! 🛁", MidiaUrl = Dog + "mix/penny.jpg", TipoMidia = "imagem" },
                new Postagem { AutorId = U("gustavo.peixoto").Id, Aprovado = false, DataCriacao = Recente(),
                    Texto = "Dica: antes de adotar, converse com todos da casa e calcule os custos com ração e veterinário. Adoção é para a vida toda!" },
                // Deve ser recusada: venda de animais é proibida
                new Postagem { AutorId = U("vinicius.lopes").Id, Aprovado = false, DataCriacao = Recente(),
                    Texto = "VENDO filhotes de husky com olhos azuis, R$ 800 cada. Chama no WhatsApp que faço desconto!!", MidiaUrl = Dog + "husky/n02110185_13434.jpg", TipoMidia = "imagem" },
                new Postagem { AutorId = U("amanda.vieira").Id, Aprovado = false, DataCriacao = Recente(),
                    Texto = "Campanha de castração gratuita no bairro São José! Divulguem 🙏" });

            var comentarios = new[]
            {
                ("bianca.nogueira", "Que lindo! Parabéns pela adoção 😍"),
                ("rodrigo.siqueira", "Ele tem irmãos para adoção também?"),
                ("priscila.antunes", "Que trabalho incrível dessa ONG, sempre ajudo com ração."),
                ("eduardo.pires", "Moro em apartamento, ele se adaptaria?"),
                ("tatiane.moraes", "Compartilhei com a minha família, quem sabe alguém adota!"),
                ("jessica.rocha", "Que fofura 🐾🐾"),
                ("thiago.monteiro", "Adotei o meu aqui também, recomendo demais."),
                // Deve ser recusado: spam
                ("promocoes.pet", "GANHE DINHEIRO RÁPIDO trabalhando de casa!!! Clique no link do meu perfil 💰💰💰"),
            };
            foreach (var (autor, texto) in comentarios)
                db.Comentarios.Add(new Comentario { PostagemId = postsAprovados[rnd.Next(postsAprovados.Count)], AutorId = U(autor).Id, Texto = texto, Aprovado = false, DataCriacao = Recente() });

            db.Depoimentos.AddRange(
                new Depoimento { AutorId = U("otavio.rangel").Id, Aprovado = false, DataCriacao = Recente(), Titulo = "O Bob mudou a nossa casa",
                    Texto = "Adotamos o Bob há 3 meses. Ele chegou tímido, com medo de tudo, e hoje dorme no pé da cama das crianças. Obrigado, Match-Pet!" },
                new Depoimento { AutorId = U("jessica.rocha").Id, Aprovado = false, DataCriacao = Recente(), Titulo = "Mia, a gata que escolheu a gente",
                    Texto = "Fomos conhecer outro gatinho, mas a Mia subiu no meu colo e não saiu mais. O termo digital deixou tudo muito simples." });

            Pet PetPendente(Usuario dono, string nome, string especie, string raca, string sexo, string porte, int idade, string descricao, string foto) => new()
            {
                Nome = nome, Especie = especie, Raca = raca, Sexo = sexo, Porte = porte, Idade = idade, Descricao = descricao,
                Castrado = rnd.Next(2) == 0, PrecisaDeQuintal = porte == "Grande", RecomendadoParaCriancas = rnd.Next(3) > 0,
                Status = EspeciesPermitidas.StatusModeracao, UsuarioDoadorId = dono.Id, DataCadastro = Recente(),
                Imagens = new() { new PetImagem { Base64Data = foto } }
            };
            db.Pets.AddRange(
                PetPendente(U("quatropatas.rp"), "Pipoca", "Cão", "SRD", "Fêmea", "Médio", 2, "Resgatada na rodovia, muito dócil e brincalhona. Vacinada e vermifugada.", Dog + "mix/mydogggg_2025-11-06.jpg"),
                PetPendente(U("quatropatas.rp"), "Simba", "Gato", "SRD", "Macho", "Pequeno", 1, "Gatinho laranja muito carinhoso, usa a caixinha de areia.", Cat + "MTg2Njg2MQ.jpg"),
                PetPendente(U("camila.rezende"), "Thor", "Cão", "Rottweiler", "Macho", "Grande", 4, "Precisa de casa com quintal grande e tutor experiente.", Dog + "rottweiler/n02106550_11415.jpg"),
                PetPendente(U("priscila.antunes"), "Nina", "Coelho", "Mini Lop", "Fêmea", "Pequeno", 1, "Coelhinha mansa, vai com gaiola e bebedouro.", "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a6/Holland_Lop_Rabbit.jpg/960px-Holland_Lop_Rabbit.jpg"),
                // Deve ser recusado: papagaio-verdadeiro é ave silvestre (não consta na Portaria Ibama nº 93/1998)
                PetPendente(U("marcelo.duarte"), "Louro", "Pássaro", "Papagaio-verdadeiro", "Macho", "Pequeno", 6, "Papagaio falante, sabe cantar o hino. Vai com a gaiola.", "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2b/Amazona_aestiva_-upper_body-8a_%281%29.jpg/960px-Amazona_aestiva_-upper_body-8a_%281%29.jpg"));

            ItemDoacao ItemPendente(Usuario dono, string nome, string categoria, string especie, string descricao, string foto) => new()
            {
                Nome = nome, Categoria = categoria, EspecieDestino = especie, Descricao = descricao, Status = EspeciesPermitidas.StatusModeracao,
                UsuarioDoadorId = dono.Id, DataCadastro = Recente(), Imagens = new() { new ItemImagem { Base64Data = foto } }
            };
            db.ItensDoacao.AddRange(
                ItemPendente(U("tatiane.moraes"), "Ração para filhotes 3 kg", "Ração/Alimento", "Cão", "Pacote fechado, validade até o fim do ano.", DadosDemonstracaoComunidade.Foto.RacaoCao),
                ItemPendente(U("eduardo.pires"), "Caixa de transporte média", "Brinquedo/Acessório", "Gato", "Usada poucas vezes, em ótimo estado.", DadosDemonstracaoComunidade.Foto.CaixaTransporte),
                ItemPendente(U("bianca.nogueira"), "Caminha acolchoada G", "Casinha/Cama", "Cão", "Lavável, capa removível.", DadosDemonstracaoComunidade.Foto.CaminhaAzul));

            // Perfis com foto ou texto novo aguardando aprovação
            U("leticia.prado").BioPendente = "Veterinária recém-formada, apaixonada por gatos. Posso ajudar com dúvidas sobre vacinação! 🐱";
            U("felipe.andrade").FotoPerfilPendente = Homem(81);
            U("felipe.andrade").BioPendente = "Voluntário em feiras de adoção aos sábados.";
            U("isabela.campos").FotoPerfilPendente = Mulher(82);
            U("lartemporario.batatais").DescricaoOngPendente = "Somos um grupo de voluntários que oferece lar temporário para cães idosos até a adoção definitiva.";
            await db.SaveChangesAsync();

            // ---------- DENÚNCIAS ----------
            var petsAlvo = await db.Pets.Where(p => p.Status == "Disponível").OrderBy(p => p.Id).Select(p => p.Id).Take(10).ToListAsync();
            var itensAlvo = await db.ItensDoacao.Where(i => i.Status == "Disponível").OrderBy(i => i.Id).Select(i => i.Id).Take(5).ToListAsync();
            var denunciantes = usuarios.Where(u => !u.Bloqueado).ToList();
            Usuario Alguem() => denunciantes[rnd.Next(denunciantes.Count)];

            void Denuncia(string tipo, int alvo, string motivo, string? detalhes, string status, string? resolucao = null)
            {
                var data = agora.AddDays(-rnd.Next(0, 12)).AddHours(-rnd.Next(0, 23));
                db.Denuncias.Add(new Denuncia
                {
                    DenuncianteId = Alguem().Id, Tipo = tipo, AlvoId = alvo, Motivo = motivo, Detalhes = detalhes, Status = status, DataCriacao = data,
                    ResolvidaPorId = status == "Aberta" ? null : adminId, DataResolucao = status == "Aberta" ? null : data.AddHours(rnd.Next(2, 30)),
                    Resolucao = resolucao
                });
            }

            if (petsAlvo.Count >= 6 && postsAprovados.Count >= 6)
            {
                // Abertas (para a banca ver o fluxo)
                Denuncia("pet", petsAlvo[0], "Venda de animal", "O anunciante pediu R$ 300 de \"taxa de adoção\" pelo chat.", "Aberta");
                Denuncia("post", postsAprovados[0], "Informações falsas", "A foto da publicação é de outro animal, já vi em outro site.", "Aberta");
                Denuncia("usuario", U("henrique.barros").Id, "Golpe ou perfil falso", "Perfil criado ontem pedindo Pix para \"resgate\".", "Aberta");
                Denuncia("pet", petsAlvo[1], "Maus-tratos ou abandono", "As fotos mostram o cachorro muito magro e preso em corrente curta.", "Aberta");
                Denuncia("post", postsAprovados[1], "Spam", null, "Aberta");
                if (itensAlvo.Count > 0) Denuncia("item", itensAlvo[0], "Informações falsas", "O item está sendo vendido em outro site.", "Aberta");
                // Já resolvidas pela moderação
                Denuncia("usuario", U("carlos.vendas").Id, "Venda de animal", "Anunciava filhotes de raça por R$ 1.500.", "Resolvida", "Conta do responsável bloqueada");
                Denuncia("usuario", U("promocoes.pet").Id, "Golpe ou perfil falso", "Mandou link suspeito no chat.", "Resolvida", "Conta do responsável bloqueada");
                Denuncia("post", postsAprovados[2], "Conteúdo ofensivo ou impróprio", null, "Resolvida", "Conteúdo removido");
                Denuncia("pet", petsAlvo[2], "Animal silvestre ou ilegal", "Anúncio de jabuti.", "Resolvida", "Conteúdo removido");
                Denuncia("pet", petsAlvo[3], "Maus-tratos ou abandono", "Fotos antigas, o animal já foi adotado.", "Resolvida", "Conteúdo removido");
                // Descartadas (sem violação)
                Denuncia("post", postsAprovados[3], "Spam", "Achei repetido.", "Descartada", "Denúncia descartada");
                Denuncia("pet", petsAlvo[4], "Informações falsas", null, "Descartada", "Denúncia descartada");
                Denuncia("usuario", U("gustavo.peixoto").Id, "Outro motivo", "Não gostei do comentário dele.", "Descartada", "Denúncia descartada");
            }
            await db.SaveChangesAsync();
            log.LogInformation("Dados do painel criados: {Usuarios} cadastros, fila de aprovações e denúncias.", usuarios.Count);
        }
    }
}
