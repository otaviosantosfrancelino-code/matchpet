using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Data
{
    // Demonstração de Achados e Perdidos: pets perdidos e encontrados em bairros reais de Franca e região,
    // conversas no chat sobre os anúncios, reencontros que viraram histórias no Final Feliz
    // e alguns anúncios aguardando a aprovação da moderação.
    // Roda uma única vez (marcador: existir algum anúncio).
    public static class DadosDemonstracaoAchados
    {
        private const string Dog = "https://images.dog.ceo/breeds/";
        private const string Cat = "https://cdn2.thecatapi.com/images/";

        private record Modelo(string Tipo, string? Nome, string Especie, string? Raca, string Porte, string Sexo, string Cor, bool Coleira,
            string? Sinais, string Descricao, string Bairro, string Cidade, int DiasAtras, string Foto, string Status = AnimalPerdido.StatusAtivo);

        public static async Task CriarAsync(AppDbContext db, ILogger log)
        {
            if (await db.AnimaisPerdidos.AnyAsync()) return;
            var autores = await db.Usuarios.Where(u => u.Email.EndsWith(DadosDemonstracao.DominioDemo) && !u.Bloqueado && !u.EhOng)
                .OrderBy(u => u.Id).Select(u => u.Id).ToListAsync();
            if (autores.Count < 10) return;
            log.LogInformation("Criando anúncios de demonstração de Achados e Perdidos...");

            var rnd = new Random(1107);
            var agora = DateTime.UtcNow;
            const string P = AnimalPerdido.TipoPerdido, E = AnimalPerdido.TipoEncontrado, R = AnimalPerdido.StatusResolvido, M = AchadosPerdidosService.StatusModeracao;

            var modelos = new[]
            {
                // ---------- PERDIDOS ----------
                new Modelo(P, "Thor", "Cão", "Labrador", "Grande", "Macho", "Caramelo", true, "Coleira vermelha com plaquinha \"Thor\", cicatriz pequena na orelha esquerda",
                    "Fugiu pelo portão durante a chuva de domingo. É muito dócil, atende pelo nome e adora petisco. Pode estar assustado com barulho.", "Jardim Petráglia", "Franca", 2, Dog + "labrador/n02099712_6823.jpg"),
                new Modelo(P, "Mel", "Cão", "SRD", "Pequeno", "Fêmea", "Caramelo", false, "Rabo enrolado e mancha branca no peito",
                    "Saiu correndo atrás de um gato e não voltou. É castrada, tem medo de moto e costuma se esconder embaixo de carros.", "Vila Industrial", "Franca", 1, Dog + "mix/penny.jpg"),
                new Modelo(P, "Frajola", "Gato", "SRD", "Pequeno", "Macho", "Preto e branco", false, "Bigode branco e uma mancha preta no nariz",
                    "Gato de apartamento, escapou pela janela do 2º andar. Nunca saiu de casa, deve estar escondido perto do prédio.", "Centro", "Franca", 3, Cat + "MTgzOTAwMg.jpg"),
                new Modelo(P, "Bolinha", "Cão", "Poodle", "Pequeno", "Macho", "Branco", true, "Pelo tosado, coleira azul com guizo, idoso e com catarata",
                    "Ele tem 12 anos e enxerga pouco. Se perdeu durante a feira livre de sábado. Precisa de remédio para o coração, por favor nos ajudem!", "Jardim Aeroporto", "Franca", 4, Dog + "poodle-toy/n02113624_1023.jpg"),
                new Modelo(P, "Luna", "Gato", "Siamês", "Pequeno", "Fêmea", "Bege", true, "Olhos azuis, coleira rosa com nome e telefone apagados",
                    "Sumiu depois de uma mudança de casa. Pode estar tentando voltar para o endereço antigo no Jardim Integração.", "Jardim Integração", "Franca", 6, Cat + "MTg3ODYyOQ.jpg"),
                new Modelo(P, "Rex", "Cão", "Pastor Alemão", "Grande", "Macho", "Preto", true, "Orelhas em pé, coleira de couro marrom",
                    "Pulou o muro durante os fogos de artifício. Não é bravo, mas fica nervoso com estranhos. Se encontrar, ofereça água e nos avise.", "Jardim Consolação", "Franca", 5, Dog + "rottweiler/n02106550_4217.jpg"),
                new Modelo(P, "Pipoca", "Cão", "Shih Tzu", "Pequeno", "Fêmea", "Malhado", false, "Laço rosa na cabeça, pelo comprido",
                    "Fugiu enquanto a porta estava aberta para a entrega. É muito carinhosa e vai com qualquer pessoa.", "Centro", "Batatais", 2, Dog + "shihtzu/n02086240_2417.jpg"),
                new Modelo(P, "Tigrão", "Gato", "SRD", "Médio", "Macho", "Rajado", false, "Gordinho, rabo grosso, castrado",
                    "Costuma passear no quintal e voltar à noite, mas está sumido há 4 dias. Pode ter entrado em alguma garagem.", "Jardim Paulistano", "Ribeirão Preto", 4, Cat + "MTgzNTk1Nw.jpg"),
                new Modelo(P, "Belinha", "Cão", "Dachshund", "Pequeno", "Fêmea", "Marrom", true, "Salsichinha, coleira verde, manca um pouco da pata traseira",
                    "Escapou no passeio, a guia arrebentou. Ela é tímida e pode estar escondida em algum terreno baldio.", "Vila Santa Cruz", "Franca", 8, Dog + "dachshund/daschund-2.jpg"),
                new Modelo(P, "Nino", "Pássaro", "Calopsita", "Pequeno", "Macho", "Cinza", false, "Topete amarelo, assobia a música do Pica-Pau",
                    "Voou pela janela da cozinha. Ele é manso e pousa no ombro das pessoas. Se ouvir um assobio diferente, pode ser ele!", "Parque Universitário", "Franca", 1, "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Nymphicus_hollandicus_-_Forst_01.jpg/960px-Nymphicus_hollandicus_-_Forst_01.jpg"),

                // ---------- ENCONTRADOS (procuram o dono) ----------
                new Modelo(E, null, "Cão", "SRD", "Médio", "Macho", "Preto", true, "Coleira vermelha sem plaquinha, muito bem cuidado",
                    "Encontrei andando sozinho na avenida, parecia perdido. Está comigo em casa, comendo bem e muito manso. Claramente tem dono!", "Jardim Petráglia", "Franca", 1, Dog + "mix/mydogggg_2025-11-06.jpg"),
                new Modelo(E, null, "Gato", "SRD", "Pequeno", "Fêmea", "Tricolor", false, "Castrada (tem a marquinha na orelha), muito dócil",
                    "Apareceu no meu quintal e não quer ir embora. Está limpa e acostumada com gente. Deve ser de alguma casa do bairro.", "Cidade Nova", "Franca", 2, Cat + "MjA0NDg2Mw.jpg"),
                new Modelo(E, null, "Cão", "Golden Retriever", "Grande", "Fêmea", "Caramelo", true, "Coleira de couro com fivela dourada",
                    "Encontrada perto do Parque Fernando Costa. Está assustada mas é muito carinhosa. Deixei com um vizinho que tem quintal grande.", "Centro", "Franca", 3, Dog + "retriever-golden/n02099601_4256.jpg"),
                new Modelo(E, null, "Cão", "Pinscher", "Pequeno", "Macho", "Preto", false, "Muito pequeno, parece filhote, unhas cortadas",
                    "Achei tremendo de frio no ponto de ônibus. Está bem cuidado, deve ter fugido há pouco tempo.", "Jardim Aeroporto", "Franca", 1, Dog + "pinscher-miniature/n02107312_5719.jpg"),
                new Modelo(E, null, "Gato", "SRD", "Médio", "Macho", "Laranja", true, "Coleira com guizo azul",
                    "Entrou na loja onde eu trabalho e ficou o dia inteiro. Tem coleira, então com certeza tem família procurando.", "Centro", "Restinga", 2, Cat + "MTg2Njg2MQ.jpg"),
                new Modelo(E, null, "Cão", "Husky Siberiano", "Grande", "Macho", "Cinza", true, "Olhos azuis, coleira laranja",
                    "Encontrado na estrada vicinal sentido Cristais. Está bem alimentado e é amigável. Está no canil de um amigo.", "Zona Rural", "Cristais Paulista", 4, Dog + "husky/n02110185_14906.jpg"),

                // ---------- REENCONTROS (viraram Final Feliz) ----------
                new Modelo(P, "Princesa", "Cão", "SRD", "Médio", "Fêmea", "Caramelo", true, "Coleira rosa", "Se perdeu durante a chuva forte da semana passada.",
                    "São Joaquim", "Franca", 12, Dog + "cattledog-australian/img_2432.jpg", R),
                new Modelo(P, "Simba", "Gato", "SRD", "Pequeno", "Macho", "Laranja", false, null, "Fugiu pela janela e passou três noites fora.",
                    "Jardim Noêmia", "Franca", 15, Cat + "MTc5MjI3MA.jpg", R),
                new Modelo(E, null, "Cão", "Beagle", "Médio", "Macho", "Tricolor", true, "Coleira com o nome Max", "Encontrado no Jardim Zelinda, muito bem cuidado.",
                    "Jardim Zelinda", "Franca", 10, Dog + "hound-basset/n02088238_10129.jpg", R),

                // ---------- AGUARDANDO APROVAÇÃO (para a demonstração da moderação) ----------
                new Modelo(P, "Bidu", "Cão", "SRD", "Médio", "Macho", "Bege", true, "Coleira azul",
                    "Saiu de casa ontem à noite e não voltou. Muito brincalhão.", "Vila Aparecida", "Franca", 0, Dog + "mix/penny.jpg", M),
                new Modelo(E, null, "Gato", "SRD", "Pequeno", "Fêmea", "Branco", false, null,
                    "Filhote encontrado dentro do motor do meu carro, já está bem e alimentado.", "Jardim Francano", "Franca", 0, Cat + "MTgzOTYzNQ.jpg", M),
            };

            var anuncios = new List<AnimalPerdido>();
            for (var i = 0; i < modelos.Length; i++)
            {
                var m = modelos[i];
                var quando = agora.AddDays(-m.DiasAtras).AddHours(-rnd.Next(1, 12));
                var a = new AnimalPerdido
                {
                    Tipo = m.Tipo, Nome = m.Nome, Especie = m.Especie, Raca = m.Raca, Porte = m.Porte, Sexo = m.Sexo, Cor = m.Cor, UsaColeira = m.Coleira,
                    SinaisParticulares = m.Sinais, Descricao = m.Descricao, Bairro = m.Bairro, Cidade = m.Cidade, Estado = "SP",
                    DataOcorrido = quando.AddHours(-rnd.Next(2, 20)), DataCadastro = quando, Status = m.Status,
                    UsuarioId = autores[(i * 3 + 1) % autores.Count],
                    UltimoLembrete = m.Status == AnimalPerdido.StatusAtivo ? agora.AddHours(-rnd.Next(1, 40)) : null,
                    DataResolucao = m.Status == R ? quando.AddDays(rnd.Next(2, 5)) : null,
                    Imagens = new() { new AnimalPerdidoImagem { Base64Data = m.Foto } }
                };
                anuncios.Add(a);
            }
            db.AnimaisPerdidos.AddRange(anuncios);
            await db.SaveChangesAsync();

            // ---------- CONVERSAS NO CHAT SOBRE OS ANÚNCIOS ----------
            int OutroAutor(int dono) { int id; do { id = autores[rnd.Next(autores.Count)]; } while (id == dono); return id; }
            var roteiros = new (int Indice, string[] Falas)[]
            {
                (0, new[] { "Olá! Acho que vi o Thor ontem perto da padaria da Av. Champagnat, por volta das 19h.", "Sério?! Ele estava sozinho? Para que lado foi?", "Estava sozinho sim, foi no sentido do Jardim Petráglia. Tentei chamar, mas ele correu.", "Muito obrigado! Vou procurar por lá agora mesmo 🙏" }),
                (1, new[] { "Oi! Tem uma cachorrinha caramelo parecida com a Mel dormindo na garagem do meu prédio.", "Meu Deus! Pode me mandar uma foto? Ela tem uma mancha branca no peito.", "Mandei! Tem a mancha sim 😊" }),
                (3, new[] { "Boa tarde, vi um poodle branco idoso perto da feira, estava com uma senhora.", "Obrigada pela informação! Ele precisa do remédio, vou até lá agora." }),
                (10, new[] { "Olá! Acho que esse cachorro preto é do meu vizinho, ele está procurando há dois dias.", "Que ótimo! Pode pedir para ele me chamar aqui no chat?", "Já avisei, ele vai falar com você 🐾" }),
                (12, new[] { "Oi, essa golden parece muito com a Nala da minha tia! Ela tem uma cicatriz na pata da frente?", "Tem sim! Uma cicatriz pequena na pata direita.", "É ela!!! Minha tia vai chorar de alegria. Como combinamos para buscar?" }),
                (15, new[] { "Esse husky pode ser do sítio do meu primo em Cristais. Vou confirmar com ele." }),
            };
            foreach (var (indice, falas) in roteiros)
            {
                var anuncio = anuncios[indice];
                var interessado = OutroAutor(anuncio.UsuarioId);
                var inicio = anuncio.DataCadastro.AddHours(rnd.Next(2, 20));
                var conversa = new Conversa { IniciadorId = interessado, DestinatarioId = anuncio.UsuarioId, AnimalPerdidoId = anuncio.Id, DataCriacao = inicio, UltimaMensagemEm = inicio };
                db.Conversas.Add(conversa);
                await db.SaveChangesAsync();
                for (var f = 0; f < falas.Length; f++)
                {
                    var quando = inicio.AddMinutes(f * rnd.Next(3, 40));
                    db.Mensagens.Add(new Mensagem { ConversaId = conversa.Id, RemetenteId = f % 2 == 0 ? interessado : anuncio.UsuarioId, Texto = falas[f], DataEnvio = quando, Lida = true });
                    conversa.UltimaMensagemEm = quando;
                }
            }

            // ---------- HISTÓRIAS DE REENCONTRO NO FINAL FELIZ ----------
            var agradecimentos = new[]
            {
                "A Princesa voltou para casa depois de 3 dias! Uma moradora do São Joaquim viu o anúncio, reconheceu a coleira rosa e me chamou no chat. Obrigada a todos que compartilharam! 💛",
                "O Simba estava escondido no telhado do vizinho. Quem avisou foi uma pessoa que viu o anúncio aqui no Match-Pet. Nunca vou esquecer essa ajuda! 🧡",
                "Encontrei o Max andando sozinho e em dois dias a família dele me chamou pelo chat. Ver o reencontro foi emocionante demais. Obrigado, Match-Pet! 🐶",
            };
            var resolvidos = anuncios.Where(a => a.Status == R).ToList();
            for (var i = 0; i < resolvidos.Count; i++)
            {
                var a = resolvidos[i];
                db.Depoimentos.Add(new Depoimento
                {
                    AutorId = a.UsuarioId, AnimalPerdidoId = a.Id, Titulo = AchadosPerdidosService.TituloHistoria(a),
                    Texto = agradecimentos[i % agradecimentos.Length], FotoUrl = a.Imagens[0].Base64Data, Aprovado = true,
                    DataCriacao = a.DataResolucao ?? agora
                });
            }
            await db.SaveChangesAsync();
            log.LogInformation("Achados e Perdidos de demonstração criados: {Total} anúncios.", anuncios.Count);
        }
    }
}
