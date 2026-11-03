using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;
using F = crudcomdb.Data.DadosDemonstracaoComunidade.Foto;

namespace crudcomdb.Data
{
    // Terceira leva de demonstração: muito mais pessoas, ONGs, pets (com adoções espalhadas pelos últimos 12 meses
    // para os rankings), itens, vídeos, comentários, patinhas, amizades, histórias e notificações.
    // Roda uma única vez (marcador: conta helena.duarte@demo...).
    public static class DadosDemonstracaoV3
    {
        private const string D = DadosDemonstracao.DominioDemo;
        private const string WikiOrig = "https://upload.wikimedia.org/wikipedia/commons/";
        private const string Dog = "https://images.dog.ceo/breeds/";
        private const string Cat = "https://cdn2.thecatapi.com/images/";

        private static string Mulher(int n) => $"https://randomuser.me/api/portraits/women/{n}.jpg";
        private static string Homem(int n) => $"https://randomuser.me/api/portraits/men/{n}.jpg";

        private static readonly (string Url, string Legenda)[] VideosNovos =
        {
            (WikiOrig + "0/0c/Pro_dog_walker_-_Tokyo_-_2024_Nov_1.webm", "Passeador profissional com a turma toda 🐕🐕🐕"),
            (WikiOrig + "0/04/Parrot_remained_trained_to_dance.webm", "Olha esse papagaio dançando! 🦜💃"),
            (WikiOrig + "2/2c/Pet_rabbit_eating_grass.webm", "Hora do lanche do coelhinho 🐰🌱"),
            (WikiOrig + "3/3f/Christmas_Puppy_ctr.webm", "O presente de Natal mais fofo que existe 🎄🐶"),
            (WikiOrig + "4/49/Berlin_Labradoodle_schwimmen.webm", "Dia de piscina! Nadador profissional 🏊‍♂️🐕"),
            (WikiOrig + "6/6d/Housecat_Grooming_Itself.webm", "Spa day: momento de autocuidado felino 🛁🐈"),
            (WikiOrig + "6/6f/Kitten_jumping_on_bamboo_mat.webm", "Filhote descobrindo o mundo aos pulinhos 😻"),
            (WikiOrig + "7/77/-01-_Miniature_Pinscher_puppy.webm", "Pinscher pequenininho, coragem gigante 😂"),
            (WikiOrig + "8/84/Cat_body_language.webm", "Você sabe ler a linguagem corporal do seu gato? Repare no rabo! 🐱"),
            (WikiOrig + "8/8a/Katzensprung_in_Zeitlupe.webm", "Salto felino em câmera lenta. Pura física! 🚀"),
            (WikiOrig + "a/ac/Cat_lapping_water_off_ground_in_slow_motion.gk.webm", "Como os gatos bebem água (em câmera lenta) 💧"),
            (WikiOrig + "c/cb/Salto_-_Cat_Jump.webm", "Mais um salto olímpico aqui em casa 🥇"),
            (WikiOrig + "f/f1/Cat_jumping_backwards.webm", "Quando o pepino aparece... 🥒😹"),
            (WikiOrig + "f/fa/Siamese_Maine_Coon_3_2019-06-10.webm", "Mistura de siamês com Maine Coon: o charme em pessoa 💙"),
            (WikiOrig + "3/35/Sammy_springt.webm", "O Sammy pulando de alegria quando chego em casa 💛"),
            (WikiOrig + "8/8a/Dog_Stuck_In_The_Weeds_and_Vines.webm", "Resgate do dia: ele ficou preso no mato, mas já está bem! 🙏"),
        };

        private static readonly string[] FotosCaes =
        {
            "akita/512px-ainu-dog.jpg", "akita/512px-akita_inu.jpg", "basenji/n02110806_4218.jpg", "basenji/n02110806_4876.jpg",
            "bulldog-english/murphy.jpg", "bulldog-english/jager-1.jpg", "bulldog-french/n02108915_4160.jpg", "bulldog-french/n02108915_1119.jpg",
            "cattledog-australian/img_2432.jpg", "cattledog-australian/img_1418.jpg", "chow/n02112137_16270.jpg", "chow/n02112137_6506.jpg",
            "cockapoo/guri4.jpg", "cockapoo/gracie.jpg", "dachshund/daschund-2.jpg", "dachshund/stretched_dachshund.jpg",
            "doberman/n02107142_15003.jpg", "doberman/n02107142_2153.jpg", "greyhound-italian/n02091032_8244.jpg", "greyhound-italian/n02091032_7200.jpg",
            "hound-basset/n02088238_10129.jpg", "hound-basset/n02088238_9691.jpg", "husky/n02110185_13434.jpg", "husky/n02110185_14906.jpg",
            "labrador/jessieincognito.jpg", "labrador/n02099712_6823.jpg", "malamute/n02110063_4629.jpg", "malamute/n02110063_8019.jpg",
            "mix/mydogggg_2025-11-06.jpg", "mix/penny.jpg", "newfoundland/n02111277_1040.jpg", "newfoundland/n02111277_4404.jpg",
            "papillon/n02086910_8590.jpg", "papillon/n02086910_933.jpg", "pekinese/n02086079_15842.jpg", "pekinese/n02086079_14359.jpg",
            "pinscher-miniature/n02107312_5719.jpg", "pinscher-miniature/n02107312_6650.jpg", "poodle-toy/n02113624_1023.jpg", "poodle-toy/n02113624_2311.jpg",
            "pug/n02110958_2009.jpg", "pug/n02110958_16492.jpg", "retriever-golden/n02099601_4256.jpg", "retriever-golden/20200731_180910_200731.jpg",
            "rottweiler/n02106550_11415.jpg", "rottweiler/n02106550_4217.jpg", "samoyed/n02111889_2410.jpg", "samoyed/n02111889_5680.jpg",
            "setter-irish/n02100877_407.jpg", "setter-irish/n02100877_6436.jpg", "sheepdog-shetland/n02105855_17107.jpg", "sheepdog-shetland/n02105855_11668.jpg",
            "shiba/shiba-7.jpg", "shiba/shiba-1.jpg", "shihtzu/n02086240_2417.jpg", "shihtzu/n02086240_306.jpg",
            "springer-english/n02102040_3260.jpg", "springer-english/n02102040_1082.jpg", "stbernard/n02109525_7656.jpg", "stbernard/n02109525_2648.jpg",
            "terrier-yorkshire/20200319_143130.jpg", "terrier-yorkshire/n02094433_3613.jpg", "weimaraner/n02092339_1672.jpg", "weimaraner/n02092339_7959.jpg",
            "whippet/n02091134_10618.jpg", "whippet/n02091134_7794.jpg",
        };

        private static readonly string[] FotosGatos =
        {
            "1ov", "1qq", "29d", "2bi", "2nc", "47a", "4s9", "70p", "79v", "7m4", "7pCnjXd47", "9r7", "MTc5MjI3MA", "MTcyMTMxOQ",
            "MTg2Njg2MQ", "MTg3ODYyOQ", "MTgzNTk1Nw", "MTgzOTAwMg", "MTgzOTYzNQ", "MjA0NDg2Mw", "WEgQATxKa", "adf", "ail", "b04",
            "ba3", "bc4", "blm", "bqj", "c8p", "ci3", "cmm", "coa", "cu0", "d74", "dvq", "egu", "pK_t-KYIi", "qm", "tu", "xTo8K7wb3",
        };

        // Raça e porte a partir da pasta da foto do dog.ceo
        private static readonly Dictionary<string, (string Raca, string Porte)> Racas = new()
        {
            ["akita"] = ("Akita", "Grande"), ["basenji"] = ("Basenji", "Médio"), ["bulldog-english"] = ("Buldogue Inglês", "Médio"),
            ["bulldog-french"] = ("Buldogue Francês", "Pequeno"), ["cattledog-australian"] = ("Boiadeiro Australiano", "Médio"),
            ["chow"] = ("Chow Chow", "Médio"), ["cockapoo"] = ("Cockapoo", "Pequeno"), ["dachshund"] = ("Dachshund", "Pequeno"),
            ["doberman"] = ("Dobermann", "Grande"), ["greyhound-italian"] = ("Galgo Italiano", "Pequeno"), ["hound-basset"] = ("Basset Hound", "Médio"),
            ["husky"] = ("Husky Siberiano", "Grande"), ["labrador"] = ("Labrador", "Grande"), ["malamute"] = ("Malamute do Alasca", "Grande"),
            ["mix"] = ("SRD", "Médio"), ["newfoundland"] = ("Terra-Nova", "Grande"), ["papillon"] = ("Papillon", "Pequeno"),
            ["pekinese"] = ("Pequinês", "Pequeno"), ["pinscher-miniature"] = ("Pinscher", "Pequeno"), ["poodle-toy"] = ("Poodle Toy", "Pequeno"),
            ["pug"] = ("Pug", "Pequeno"), ["retriever-golden"] = ("Golden Retriever", "Grande"), ["rottweiler"] = ("Rottweiler", "Grande"),
            ["samoyed"] = ("Samoieda", "Grande"), ["setter-irish"] = ("Setter Irlandês", "Grande"), ["sheepdog-shetland"] = ("Pastor de Shetland", "Médio"),
            ["shiba"] = ("Shiba Inu", "Médio"), ["shihtzu"] = ("Shih Tzu", "Pequeno"), ["springer-english"] = ("Springer Spaniel", "Médio"),
            ["stbernard"] = ("São Bernardo", "Grande"), ["terrier-yorkshire"] = ("Yorkshire", "Pequeno"), ["weimaraner"] = ("Weimaraner", "Grande"),
            ["whippet"] = ("Whippet", "Médio"),
        };

        private static readonly string[] NomesPets =
        {
            "Apolo", "Mel", "Nescau", "Kira", "Thor", "Maya", "Bob", "Lola", "Simba", "Nala", "Fred", "Pandora", "Chico", "Laika", "Toddy",
            "Jade", "Bento", "Belinha", "Ozzy", "Cacau", "Marley", "Bruma", "Pipoca", "Zara", "Theo", "Princesa", "Rambo", "Estrela", "Bolt", "Dalila",
            "Scooby", "Sushi", "Duda", "Floquinho", "Hércules", "Penélope", "Tobias", "Canela", "Brutus", "Lady", "Nico", "Pituca", "Ragnar", "Olívia",
            "Feijão", "Jujuba", "Rocky", "Bidu", "Amendoim", "Fiona", "Juca", "Babi", "Pretinha", "Fumacinha", "Ziggy", "Meg", "Tequila", "Pepe",
            "Charlotte", "Bartolomeu", "Mostarda", "Neguinha", "Sansão", "Kika", "Faísca", "Docinho", "Loki", "Bianca", "Kiwi", "Muffin", "Tapioca",
            "Salem", "Garfield", "Luna", "Frajola", "Mingau", "Nuvem", "Chiquinha", "Pantera", "Bolacha", "Sardinha", "Felícia", "Zorro", "Isis",
            "Pompom", "Romeu", "Julieta", "Biscoito", "Paçoca", "Café", "Caramelo", "Tigrinho", "Malhado", "Branquinha", "Ônix", "Safira", "Merlin",
            "Chanel", "Dengo", "Fofucho", "Petúnia", "Xodó", "Brisa", "Dinamite", "Quindim", "Rosinha", "Gaia",
        };

        private static readonly string[] DescricoesCao =
        {
            "Muito carinhoso, adora colo e brincadeiras de bolinha.", "Resgatado das ruas, hoje confia nas pessoas e ama um cafuné.",
            "Brincalhão e cheio de energia. Ideal para famílias ativas.", "Calmo, educado e se dá bem com outros cães.",
            "Anda bem na guia e já faz as necessidades no lugar certo.", "Protetor e leal, perfeito para casa com quintal.",
            "Adora crianças e passa o dia esperando alguém para brincar.", "Tímido no começo, mas depois vira sombra do tutor.",
            "Vacinado, vermifugado e com microchip. Só falta você!", "Companheiro para caminhadas e para maratonar séries no sofá.",
        };
        private static readonly string[] DescricoesGato =
        {
            "Ronrona alto e ama dormir no sol da janela.", "Curioso e brincalhão, adora varinhas com penas.",
            "Tranquilo, independente e muito limpinho.", "Carente na medida certa: pede carinho e depois vai cochilar.",
            "Testado negativo para FIV/FeLV, castrado e vacinado.", "Convive bem com cães e outros gatos.",
            "Foi resgatado filhote e hoje é um gato confiante e dócil.", "Precisa de casa com janelas teladas. Muito amoroso!",
        };

        public static async Task CriarAsync(AppDbContext db, ILogger log)
        {
            if (await db.Usuarios.AnyAsync(u => u.Email == "helena.duarte" + D)) return;
            log.LogInformation("Criando a terceira leva de cadastros de demonstração...");

            var rnd = new Random(2027);
            var agora = DateTime.UtcNow;
            var senha = SenhaService.GerarHash(DadosDemonstracao.SenhaDemo);

            // ===================== PESSOAS E ONGs =====================
            Usuario Conta(string nome, string email, string doc, string cidade, string fone, string? foto, string bio, bool ong = false, int diasConta = 120) => new()
            {
                Nome = nome, Email = email + D, Documento = doc, Telefone = fone, Senha = senha,
                CEP = "14400-000", Logradouro = "Rua de Demonstração", Numero = rnd.Next(10, 999).ToString(), Bairro = "Centro",
                Cidade = cidade, Estado = "SP", EhOng = ong, ChavePix = ong ? "pix." + email + D : null, DescricaoOng = ong ? bio : null, Bio = bio,
                FotoPerfilBase64 = foto, StatusValidacaoDocumento = "Aprovado", DocumentoObservacao = "Conta de demonstração.",
                DataValidacaoDocumento = agora, Classificacao = ong ? "ONG Verificada" : "Usuário Verificado",
                EmailConfirmado = true, CodigoConfirmacao = "ISENTO", AceitouTermos = true, DataAceiteTermos = agora,
                IpAceite = "seed", DataCadastro = agora.AddDays(-diasConta)
            };

            var dadosPessoas = new (string Nome, bool Mulher, int Foto, string Cidade, string Bio)[]
            {
                ("Helena Duarte", true, 12, "Franca", "Mãe de pet em tempo integral. Já adotei 4 e não vou parar 💛"),
                ("Rodrigo Pacheco", false, 14, "Ribeirão Preto", "Engenheiro e tutor de dois vira-latas caramelo."),
                ("Mariana Teixeira", true, 33, "Batatais", "Voluntária em feiras de adoção aos sábados 🐾"),
                ("Felipe Cardoso", false, 41, "Franca", "Corro com meu husky todos os dias às 6h."),
                ("Vanessa Moreira", true, 50, "Sertãozinho", "Gateira assumida. 5 gatos e contando 😹"),
                ("André Barbosa", false, 3, "Orlândia", "Médico veterinário, atendo ONGs com desconto."),
                ("Paula Azevedo", true, 7, "São Joaquim da Barra", "Lar temporário de filhotes. Já são 31 encaminhados!"),
                ("Leonardo Castro", false, 18, "Barretos", "Tenho chácara e espaço para cães grandes."),
                ("Carolina Dias", true, 26, "Franca", "Designer, faço artes gratuitas para ONGs divulgarem os pets."),
                ("Eduardo Pires", false, 27, "Bebedouro", "Adotei a Kira em 2025 e virei defensor da adoção."),
                ("Renata Gomes", true, 36, "Jardinópolis", "Professora de educação infantil. Levo os pets para conhecerem as crianças."),
                ("Gustavo Lopes", false, 29, "Franca", "Personal trainer. Meu cachorro treina comigo!"),
                ("Aline Vieira", true, 39, "Igarapava", "Resgato gatinhos abandonados na área rural."),
                ("Matheus Rezende", false, 34, "São Carlos", "Estudante de Medicina Veterinária na USP."),
                ("Tatiane Barros", true, 47, "Franca", "Faço petiscos naturais e doo parte para abrigos 🦴"),
                ("Ricardo Monteiro", false, 55, "Araraquara", "Aposentado. Meu companheiro é um basset de 10 anos."),
                ("Sabrina Cunha", true, 60, "Ribeirão Preto", "Advogada animalista. Tire dúvidas sobre a Lei Sansão!"),
                ("Vinícius Ramos", false, 62, "Ituverava", "Adotei um cão idoso e foi a melhor escolha da vida."),
                ("Priscila Andrade", true, 63, "Franca", "Mãe da Mel e da Jujuba, duas pinschers bravas e amorosas."),
                ("Fábio Moura", false, 66, "Patrocínio Paulista", "Motorista de aplicativo. Transporto pets de ONGs de graça."),
                ("Débora Siqueira", true, 70, "Cristais Paulista", "Costuro caminhas com retalhos e doo para abrigos."),
                ("Henrique Tavares", false, 71, "Franca", "Fotógrafo voluntário das feiras de adoção 📸"),
                ("Letícia Prado", true, 72, "Batatais", "Estudante de Biologia, amo aves e coelhos 🐰🦜"),
                ("Samuel Fonseca", false, 73, "Sertãozinho", "Pai de 3 filhos e 2 cães. Casa cheia!"),
                ("Bianca Correia", true, 75, "Franca", "Adestradora positiva. Dicas toda semana no meu perfil."),
                ("Murilo Antunes", false, 77, "Morro Agudo", "Tenho um sítio e sempre acolho mais um."),
                ("Elisa Campos", true, 79, "Ribeirão Preto", "Gatos pretos são os mais amorosos. Fica a dica 🖤"),
                ("César Batista", false, 80, "Franca", "Doador de ração mensal para três ONGs."),
                ("Yasmin Lacerda", true, 81, "Guará", "Adotei a primeira gatinha pelo Match-Pet ✨"),
                ("Otávio Brandão", false, 82, "Franca", "Programador. Fiz um comedouro automático para meu gato 🤖"),
                ("Lívia Rangel", true, 84, "Nuporanga", "Apaixonada por cães de porte grande."),
                ("Daniel Queiroz", false, 86, "Pedregulho", "Moro sozinho e meu cão é minha família."),
                ("Clara Nunes", true, 87, "Franca", "Bióloga, ajudo na castração de colônias de gatos."),
                ("Igor Peixoto", false, 88, "São Carlos", "Gamer e tutor de um shiba que ignora tudo 😂"),
                ("Manuela Rios", true, 89, "Restinga", "Criança apaixonada por animais (conta da minha mãe) 🧒🐶"),
                ("Caio Figueiredo", false, 90, "Franca", "Empresário, patrocino feiras de adoção."),
                ("Sofia Leal", true, 91, "Araraquara", "Fiz TCC sobre adoção responsável 🎓"),
                ("Arthur Medeiros", false, 92, "Ribeirão Preto", "Tutor de um dobermann gentil chamado Barão."),
                ("Luana Farias", true, 93, "Franca", "Mãe de pet idoso. Cuidado especial é amor."),
                ("Heitor Borges", false, 94, "Barretos", "Peão de rodeio que ama cachorro 🤠🐕"),
            };
            var pessoas = dadosPessoas.Select((p, i) =>
            {
                var email = string.Join('.', p.Nome.ToLowerInvariant().Split(' ').Select(Simplificar));
                return Conta(p.Nome, email, DadosDemonstracao.GerarCpf($"93{i + 1:0000000}"), p.Cidade, $"(16) 99501-{1000 + i}",
                    p.Mulher ? Mulher(p.Foto) : Homem(p.Foto), p.Bio, diasConta: rnd.Next(30, 500));
            }).ToList();

            var dadosOngs = new (string Nome, string Email, string Cidade, string Bio)[]
            {
                ("Instituto Patas do Bem Ribeirão", "patasdobem", "Ribeirão Preto", "Abrigo com 120 animais resgatados. Castração gratuita para famílias de baixa renda."),
                ("Abrigo Esperança Animal Barretos", "esperancaanimal", "Barretos", "Desde 2015 resgatando cães de grande porte e dando uma segunda chance."),
                ("SOS Gatinhos Franca", "sosgatinhos", "Franca", "Especialistas em felinos: resgate, tratamento e adoção responsável."),
                ("Projeto Vira-Lata Feliz Batatais", "viralatafeliz", "Batatais", "Feiras mensais, campanhas de vacinação e educação nas escolas."),
                ("Associação Amigos dos Animais Bebedouro", "amigosanimais", "Bebedouro", "Mais de 3.000 adoções realizadas e uma clínica social própria."),
                ("Lar dos Focinhos Orlândia", "lardosfocinhos", "Orlândia", "Lar temporário coletivo para cães idosos e com deficiência."),
            };
            var ongs = dadosOngs.Select((o, i) => Conta(o.Nome, o.Email, DadosDemonstracao.GerarCnpj($"93{i + 1:000000}0001"), o.Cidade,
                $"(16) 99601-{2000 + i}", null, o.Bio, true, rnd.Next(400, 900))).ToList();

            db.Usuarios.AddRange(pessoas);
            db.Usuarios.AddRange(ongs);
            await db.SaveChangesAsync();

            var novosIds = pessoas.Concat(ongs).Select(u => u.Id).ToHashSet();
            var antigos = await db.Usuarios.Where(u => u.Email.EndsWith(D) && !novosIds.Contains(u.Id)).ToListAsync();
            var todasOngs = ongs.Concat(antigos.Where(u => u.EhOng)).ToList();
            var todasPessoas = pessoas.Concat(antigos.Where(u => !u.EhOng)).ToList();
            var todos = todasPessoas.Concat(todasOngs).ToList();

            // ===================== PETS =====================
            // Adoções espalhadas: algumas nesta semana, outras no mês, no semestre e no ano (os filtros do ranking mostram diferenças)
            int DiasAdocao(int i) => (i % 4) switch { 0 => rnd.Next(0, 7), 1 => rnd.Next(7, 30), 2 => rnd.Next(30, 180), _ => rnd.Next(180, 360) };
            // Alguns adotantes adotam várias vezes (para o pódio ficar interessante)
            Usuario Adotante() => pessoas[(int)(Math.Pow(rnd.NextDouble(), 2.2) * pessoas.Count)];
            Usuario Doador() => rnd.NextDouble() < 0.72 ? todasOngs[rnd.Next(todasOngs.Count)] : pessoas[rnd.Next(pessoas.Count)];

            var pets = new List<Pet>();
            var nomeIdx = 0;
            void NovoPet(string especie, string raca, string porte, string foto, string descricao, int i)
            {
                var status = i % 5 < 2 ? "Adotado" : i % 13 == 7 ? "Em Processo" : "Disponível";
                var adotado = status == "Adotado";
                var dataAdocao = agora.AddDays(-DiasAdocao(i)).AddHours(-rnd.Next(0, 20));
                var p = new Pet
                {
                    Nome = NomesPets[nomeIdx++ % NomesPets.Length], Especie = especie, Raca = raca, Porte = porte,
                    Sexo = rnd.Next(2) == 0 ? "Macho" : "Fêmea", Idade = rnd.Next(0, 13), Castrado = rnd.NextDouble() < 0.8,
                    PrecisaDeQuintal = porte == "Grande", RecomendadoParaCriancas = rnd.NextDouble() < 0.75, Descricao = descricao,
                    Status = status, UsuarioDoadorId = Doador().Id, FotoUrl = "",
                    DataCadastro = adotado ? dataAdocao.AddDays(-rnd.Next(7, 60)) : agora.AddDays(-rnd.Next(1, 300)),
                    Imagens = new List<PetImagem> { new() { Base64Data = foto } }
                };
                if (adotado)
                {
                    p.DataAdocao = dataAdocao;
                    var adotante = Adotante();
                    while (adotante.Id == p.UsuarioDoadorId) adotante = Adotante();
                    p.AdotanteId = adotante.Id;
                }
                pets.Add(p);
            }
            for (var i = 0; i < FotosCaes.Length; i++)
            {
                var (raca, porte) = Racas[FotosCaes[i].Split('/')[0]];
                NovoPet("Cão", raca, porte, Dog + FotosCaes[i], DescricoesCao[rnd.Next(DescricoesCao.Length)], i);
            }
            for (var i = 0; i < FotosGatos.Length; i++)
                NovoPet("Gato", "SRD", rnd.Next(3) == 0 ? "Médio" : "Pequeno", Cat + FotosGatos[i] + ".jpg", DescricoesGato[rnd.Next(DescricoesGato.Length)], i + 1);
            db.Pets.AddRange(pets);
            await db.SaveChangesAsync();

            // ===================== ITENS DO BAZAR =====================
            ItemDoacao Item(string nome, string categoria, string destino, string descricao, string foto, bool doado) => new()
            {
                Nome = nome, Categoria = categoria, EspecieDestino = destino, Descricao = descricao, Status = doado ? "Doado" : "Disponível",
                UsuarioDoadorId = (doado || rnd.NextDouble() < 0.7 ? pessoas[(int)(Math.Pow(rnd.NextDouble(), 1.8) * pessoas.Count)] : ongs[rnd.Next(ongs.Count)]).Id,
                FotoUrl = "", DataCadastro = agora.AddDays(-rnd.Next(0, 300)),
                Imagens = new List<ItemImagem> { new() { Base64Data = foto } }
            };
            var itens = new List<ItemDoacao>
            {
                Item("Ração seca 15 kg (frango)", "Ração/Alimento", "Cão", "Saco lacrado, validade 2027.", F.RacaoCao, false),
                Item("Ração filhotes 7 kg", "Ração/Alimento", "Cão", "Sobrou da fase de filhote do meu cão.", F.RacaoCao, true),
                Item("Sachês para gatos (20 un.)", "Ração/Alimento", "Gato", "Sabores variados.", F.RacaoUmidaGato, false),
                Item("Ração úmida castrados", "Ração/Alimento", "Gato", "Caixa fechada com 12 unidades.", F.RacaoUmidaGato, true),
                Item("Comedouro de inox", "Brinquedo/Acessório", "Gato", "Antiderrapante, fácil de lavar.", F.ComedouroGato, false),
                Item("Caminha vermelha G", "Casinha/Cama", "Cão", "Para cães grandes, capa removível.", F.CaminhaVermelha, false),
                Item("Caminha azul P", "Casinha/Cama", "Todos", "Acolchoada e lavável.", F.CaminhaAzul, true),
                Item("Arranhador artesanal", "Brinquedo/Acessório", "Gato", "Feito com sisal e madeira de reflorestamento.", F.Arranhador, false),
                Item("Arranhador torre 1,2 m", "Brinquedo/Acessório", "Gato", "Três andares com toca.", F.ArranhadorTorre, true),
                Item("Gaiola para periquitos", "Gaiola/Aquário", "Pássaro", "Com comedouro e bebedouro.", F.GaiolaPassaro, false),
                Item("Gaiola de madeira vintage", "Gaiola/Aquário", "Pássaro", "Decorativa e funcional.", F.GaiolaMadeira, true),
                Item("Guia listrada 1,5 m", "Brinquedo/Acessório", "Cão", "Nova, mosquetão reforçado.", F.Guia, false),
                Item("Coleira de couro P", "Brinquedo/Acessório", "Cão", "Couro legítimo, pouco uso.", F.Coleira, true),
                Item("Coleira verde M", "Brinquedo/Acessório", "Cão", "Com plaquinha de identificação.", F.ColeiraVerde, false),
                Item("Caixa de transporte nº 3", "Brinquedo/Acessório", "Todos", "Aprovada para viagens de avião.", F.CaixaTransporte, false),
                Item("Mochila pet com visor", "Brinquedo/Acessório", "Gato", "Para gatos e cães até 6 kg.", F.MochilaTransporte, true),
                Item("Bolsa de transporte macia", "Brinquedo/Acessório", "Gato", "Dobrável, com bolso lateral.", F.BolsaGato, false),
                Item("Aquário para roedores", "Gaiola/Aquário", "Outros", "60 litros, com tampa telada.", F.AquarioHamster, false),
                Item("Gaiola para hamster 2 andares", "Gaiola/Aquário", "Outros", "Com roda e bebedouro.", F.GaiolaHamster, true),
                Item("Peitoral acolchoado M", "Brinquedo/Acessório", "Cão", "Evita puxões no pescoço.", F.Peitoral, false),
                Item("Ratinhos de pelúcia (kit 6)", "Brinquedo/Acessório", "Gato", "Com catnip.", F.RatinhoGato, true),
                Item("Bolinha interativa para gatos", "Brinquedo/Acessório", "Gato", "Com guizo interno.", F.BolinhaGato, false),
                Item("Meias antiderrapantes", "Brinquedo/Acessório", "Cão", "Ótimas para cães idosos.", F.Meias, false),
                Item("Roupinha de inverno M", "Brinquedo/Acessório", "Cão", "Moletom forrado.", F.Roupinha, true),
                Item("Pá de areia higiênica", "Remédio/Higiene", "Gato", "Nova.", F.PaAreia, false),
                Item("Caixa de areia com tampa", "Remédio/Higiene", "Gato", "Reduz o cheiro e a sujeira.", F.CaixaAreia, true),
                Item("Bolinhas de tênis (kit 5)", "Brinquedo/Acessório", "Cão", "Usadas, mas inteiras.", F.Bolinha, false),
                Item("Casinha de madeira M", "Casinha/Cama", "Cão", "Telhado de duas águas.", F.Casinha, true),
                Item("Casinha vermelha G", "Casinha/Cama", "Cão", "Para cães grandes, isolada do chão.", F.CasinhaVermelha, false),
                Item("Kit ração + petiscos", "Ração/Alimento", "Cão", "Doação de fim de mês para quem precisa.", F.RacaoCao, true),
            };
            db.ItensDoacao.AddRange(itens);
            await db.SaveChangesAsync();

            // ===================== AMIZADES =====================
            var existentes = await db.Amizades.Select(a => new { a.SolicitanteId, a.DestinatarioId }).ToListAsync();
            var pares = existentes.Select(a => (a.SolicitanteId, a.DestinatarioId)).ToHashSet();
            void Amigos(Usuario a, Usuario b, bool aceita = true)
            {
                if (a.Id == b.Id || pares.Contains((a.Id, b.Id)) || pares.Contains((b.Id, a.Id))) return;
                pares.Add((a.Id, b.Id));
                db.Amizades.Add(new Amizade { SolicitanteId = a.Id, DestinatarioId = b.Id, Status = aceita ? "Aceita" : "Pendente", DataCriacao = agora.AddDays(-rnd.Next(1, 200)), DataResposta = aceita ? agora.AddDays(-rnd.Next(0, 30)) : null });
            }
            foreach (var p in pessoas)
                foreach (var outro in todos.OrderBy(_ => rnd.Next()).Take(rnd.Next(6, 14))) Amigos(p, outro);
            foreach (var o in ongs)
                foreach (var outro in todasOngs.Where(x => x.Id != o.Id)) Amigos(o, outro);
            // Pedidos pendentes para as contas antigas verem a função
            var pendentes = new List<(Usuario De, Usuario Para)>();
            foreach (var antigo in antigos.Where(a => !a.EhOng).Take(6))
                foreach (var p in pessoas.OrderBy(_ => rnd.Next()).Take(2))
                    if (!pares.Contains((p.Id, antigo.Id)) && !pares.Contains((antigo.Id, p.Id))) { Amigos(p, antigo, false); pendentes.Add((p, antigo)); }
            await db.SaveChangesAsync();

            // ===================== PUBLICAÇÕES =====================
            var posts = new List<Postagem>();
            Postagem Post(Usuario autor, string texto, string? midia, string? tipo, Pet? pet, double diasAtras)
            {
                var p = new Postagem { AutorId = autor.Id, Texto = texto, MidiaUrl = midia, TipoMidia = tipo, PetId = pet?.Id, DataCriacao = agora.AddDays(-diasAtras).AddMinutes(-rnd.Next(0, 600)) };
                posts.Add(p);
                return p;
            }
            Usuario Alguem() => pessoas[rnd.Next(pessoas.Count)];

            // Vídeos novos (aparecem nos Reels)
            for (var i = 0; i < VideosNovos.Length; i++)
                Post(i % 4 == 0 ? ongs[i % ongs.Count] : Alguem(), VideosNovos[i].Legenda, VideosNovos[i].Url, "video", null, i * 1.7 + 0.2);
            // Vídeos antigos repostados por outras pessoas
            var legendasVideo = new[] { "Não canso de assistir 😂", "Alguém mais tem um desses em casa? 🐾", "Melhor vídeo do dia!", "Energia de segunda-feira 💥", "Só alegria por aqui 💛", "Marca aqui quem também ama! 🐶🐱" };
            for (var i = 0; i < DadosDemonstracaoComunidade.Videos.Length; i++)
                Post(Alguem(), legendasVideo[i], DadosDemonstracaoComunidade.Videos[i], "video", null, 3 + i * 4.3);

            // Fotos dos pets adotados (os novos tutores mostram o dia a dia)
            var legendasAdotado = new[]
            {
                "Primeira semana do {0} em casa e já manda em tudo 😂", "{0} descobriu o sofá. Não sai mais de lá!",
                "Um mês com o(a) {0}. Gratidão a quem cuidou antes de mim 💛", "Passeio de domingo com o(a) {0} 🌳",
                "Olha a pose do(a) {0} para a foto 📸", "{0} dormindo depois de brincar a tarde toda 😴",
                "Adotar o(a) {0} foi a melhor decisão do ano! #adotenaocompre", "Banho tomado, {0} cheiroso(a) e pronto para o cafuné 🛁",
            };
            foreach (var pet in pets.Where(p => p.Status == "Adotado").Take(30))
            {
                var tutor = pessoas.First(u => u.Id == pet.AdotanteId);
                var dias = Math.Max(0.1, (agora - pet.DataAdocao!.Value).TotalDays - rnd.Next(0, 5));
                Post(tutor, string.Format(legendasAdotado[rnd.Next(legendasAdotado.Length)], pet.Nome), pet.Imagens[0].Base64Data, "imagem", pet, Math.Min(dias, 90));
            }
            // ONGs divulgando pets disponíveis
            var legendasOng = new[]
            {
                "Conheça o(a) {0}! Está esperando uma família há semanas. Toque no nome para ver o perfil 🏡",
                "{0} está pronto(a) para adoção: vacinado(a), vermifugado(a) e cheio(a) de amor 💉💛",
                "Feira de adoção neste sábado! O(a) {0} vai estar lá esperando por você 🐾",
                "Compartilhe! O(a) {0} precisa de um lar com urgência 🙏",
            };
            foreach (var pet in pets.Where(p => p.Status == "Disponível").OrderBy(_ => rnd.Next()).Take(18))
            {
                var dono = todos.First(u => u.Id == pet.UsuarioDoadorId);
                Post(dono, string.Format(legendasOng[rnd.Next(legendasOng.Length)], pet.Nome), pet.Imagens[0].Base64Data, "imagem", pet, rnd.NextDouble() * 40);
            }
            // Itens doados
            foreach (var item in itens.Where(i => i.Status == "Doado").Take(6))
            {
                var dono = todos.First(u => u.Id == item.UsuarioDoadorId);
                Post(dono, $"Doei {item.Nome.ToLowerInvariant()} pelo Bazar Solidário. Fico feliz em ajudar! 🎁", item.Imagens[0].Base64Data, "imagem", null, rnd.NextDouble() * 60);
            }
            // Textos (dicas, campanhas, desabafos)
            var textos = new (int Autor, string Texto)[]
            {
                (16, "⚖️ Você sabia? Abandonar animal é crime (Lei 9.605/98, art. 32) e, para cães e gatos, a pena vai até 5 anos (Lei 14.064/20). Denuncie!"),
                (24, "Dica de adestramento: ensine o 'senta' antes de toda refeição. Em uma semana vira hábito e o cão fica mais calmo 🦴"),
                (5, "Lembrete do vet: vermífugo a cada 3 meses e antirrábica todo ano. Seu pet agradece 💉"),
                (32, "Castração salva vidas! Uma gata não castrada pode gerar mais de 100 filhotes em 5 anos. Procure os mutirões da sua cidade."),
                (14, "Petisco caseiro: banana amassada + aveia, forno por 20 min. Meus cães piram! 🍌"),
                (0, "Hoje faz 1 ano que adotei minha primeira cachorrinha pelo Match-Pet. Já são 4 adotados aqui em casa e muito amor 💛"),
                (8, "Fiz artes gratuitas para 3 ONGs essa semana. Se a sua precisar, é só chamar no chat! 🎨"),
                (19, "Precisando de transporte para levar um pet ao veterinário? Faço de graça para ONGs aos domingos 🚗"),
                (6, "Lar temporário lotado! 6 filhotes precisando de família. Quem puder ajudar a divulgar, agradeço 🙏"),
                (36, "Defendi meu TCC sobre adoção responsável hoje! Obrigada a todas as ONGs que participaram da pesquisa 🎓"),
                (27, "Doei 50 kg de ração para o Lar dos Focinhos esse mês. Bora fazer corrente do bem? 💪"),
                (12, "Resgate concluído: 3 gatinhos encontrados numa caixa na estrada. Estão bem e em lar temporário 🐱"),
            };
            foreach (var (autor, texto) in textos) Post(pessoas[autor], texto, null, null, null, rnd.NextDouble() * 45);
            foreach (var o in ongs)
                Post(o, $"🎉 Mutirão de vacinação e castração no próximo sábado em {o.Cidade}! Vagas limitadas, inscrições pelo chat.", null, null, null, rnd.NextDouble() * 20);

            db.Postagens.AddRange(posts);
            await db.SaveChangesAsync();

            // ===================== PATINHAS, COMENTÁRIOS E NOTIFICAÇÕES =====================
            var comentarios = new[]
            {
                "Que lindo! 😍", "Amei demais!", "Que fofura 💛", "Parabéns pela adoção! 🎉", "Que história incrível 🥹", "Muito bom ver isso!",
                "Compartilhando!", "Quero conhecer!", "Que olhar 😍", "Adote, não compre! 🐾", "Obrigado por divulgar!", "Já mandei mensagem pelo chat!",
                "Isso aquece o coração ❤️", "Que sorte a de vocês!", "Morri de rir 😂", "Que dica boa, obrigada!", "Vou tentar aqui em casa!",
                "Que trabalho lindo 👏", "Contem comigo para a próxima feira!", "Precisamos de mais pessoas assim 💪", "Ele(a) parece muito feliz!",
                "Meu sonho ter um assim 🥺", "Salvei para mostrar para minha família", "Vocês são incríveis 💛",
            };
            var notificacoes = new List<Notificacao>();
            void Notificar(int para, int ator, string tipo, string texto, string link, string chave, DateTime quando)
            {
                if (para == ator) return;
                notificacoes.Add(new Notificacao { UsuarioId = para, AtorId = ator, Tipo = tipo, Texto = texto, Link = link, Chave = chave, DataCriacao = quando, Lida = quando < agora.AddDays(-2) });
            }
            string Nome(int id) => todos.First(u => u.Id == id).Nome.Split(' ')[0];

            // Posts antigos (das levas anteriores) também ganham patinhas dos novos usuários
            var idsNovos = posts.Select(x => x.Id).ToList();
            var postsAntigos = await db.Postagens.Where(p => !idsNovos.Contains(p.Id)).ToListAsync();
            foreach (var post in posts.Concat(postsAntigos))
            {
                var novo = posts.Contains(post);
                var curtidores = (novo ? todos : pessoas.Cast<Usuario>().ToList()).OrderBy(_ => rnd.Next()).Take(novo ? rnd.Next(6, 32) : rnd.Next(2, 9)).ToList();
                foreach (var u in curtidores)
                {
                    var quando = post.DataCriacao.AddMinutes(rnd.Next(5, 60 * 48));
                    if (quando > agora) quando = agora.AddMinutes(-rnd.Next(1, 120));
                    db.Curtidas.Add(new Curtida { PostagemId = post.Id, UsuarioId = u.Id, DataCriacao = quando });
                    if (quando > agora.AddDays(-20))
                        Notificar(post.AutorId, u.Id, "curtida", $"{u.Nome.Split(' ')[0]} deu uma patinha na sua publicação.", $"/comunidade?post={post.Id}", $"curtida:{post.Id}:{u.Id}", quando);
                }
                foreach (var u in todos.Where(x => x.Id != post.AutorId).OrderBy(_ => rnd.Next()).Take(novo ? rnd.Next(1, 7) : rnd.Next(0, 3)))
                {
                    var quando = post.DataCriacao.AddMinutes(rnd.Next(10, 60 * 72));
                    if (quando > agora) quando = agora.AddMinutes(-rnd.Next(1, 120));
                    db.Comentarios.Add(new Comentario { PostagemId = post.Id, AutorId = u.Id, Texto = comentarios[rnd.Next(comentarios.Length)], DataCriacao = quando });
                    if (quando > agora.AddDays(-20))
                        Notificar(post.AutorId, u.Id, "comentario", $"{u.Nome.Split(' ')[0]} comentou na sua publicação.", $"/comunidade?post={post.Id}", $"comentario:{post.Id}:{u.Id}:{quando.Ticks}", quando);
                }
            }
            await db.SaveChangesAsync();

            foreach (var (de, para) in pendentes)
                Notificar(para.Id, de.Id, "amizade", $"{de.Nome.Split(' ')[0]} quer ser seu amigo(a).", $"/u/{de.Id}", $"amizade:{de.Id}:{para.Id}", agora.AddHours(-rnd.Next(1, 40)));

            // Doadores ficam sabendo das adoções
            foreach (var pet in pets.Where(p => p.Status == "Adotado" && p.DataAdocao > agora.AddDays(-30)))
                Notificar(pet.UsuarioDoadorId, pet.AdotanteId!.Value, "termo_assinado", $"{Nome(pet.AdotanteId!.Value)} assinou o termo de adoção de {pet.Nome}. 🎉", $"/pets/detalhes/{pet.Id}", $"termo:{pet.Id}", pet.DataAdocao!.Value);

            // ===================== HISTÓRIAS DO FINAL FELIZ =====================
            var historias = new (string Titulo, string Texto)[]
            {
                ("Ele me escolheu na feira", "Fui só para olhar e voltei com o {0} no colo. Ele deitou no meu pé e não saiu mais. Hoje não imagino minha vida sem ele."),
                ("A casa ficou mais alegre", "Desde que o(a) {0} chegou, as crianças largaram o celular para brincar no quintal. Foi a melhor decisão da família."),
                ("Superou o medo", "O(a) {0} chegou com medo de tudo. Com paciência e carinho, hoje recebe as visitas abanando o rabo no portão."),
                ("Adotar um adulto foi incrível", "Todo mundo quer filhote, mas o(a) {0} já veio educado(a), castrado(a) e cheio(a) de amor para dar. Recomendo demais!"),
                ("Companhia na pandemia de solidão", "Moro sozinho(a) e o(a) {0} trouxe rotina e alegria para os meus dias. A ONG acompanhou tudo pelo chat."),
                ("Do abrigo para o sofá", "O(a) {0} passou meses no abrigo. Hoje dorme no sofá, tem cama própria e até perfil na Comunidade!"),
                ("Termo assinado em minutos", "Conversei com a ONG pelo chat, fiz a visita e assinei o termo no celular mesmo. Super simples e seguro. Bem-vindo(a), {0}!"),
                ("Melhores amigos", "Eu tinha medo de ele não se dar bem com meu outro cão, mas o(a) {0} e ele viraram inseparáveis."),
                ("Uma segunda chance", "O(a) {0} foi resgatado(a) machucado(a) na estrada. Hoje está saudável, gordinho(a) e muito amado(a)."),
                ("Recomendo o Match-Pet", "Encontrei o(a) {0} pelo filtro de pets que combinam com apartamento. Deu match na hora! 💛"),
                ("Meu parceiro de corrida", "Corro 5 km por dia e agora tenho companhia. O(a) {0} é mais animado(a) do que eu!"),
                ("Velhinho e feliz", "Adotei o(a) {0} com 10 anos. Ele merecia uma aposentadoria tranquila e hoje é o rei da casa."),
            };
            var adotadosComHistoria = pets.Where(p => p.Status == "Adotado").OrderBy(_ => rnd.Next()).Take(historias.Length).ToList();
            for (var i = 0; i < adotadosComHistoria.Count; i++)
            {
                var pet = adotadosComHistoria[i];
                var dias = Math.Max(0, (int)(agora - pet.DataAdocao!.Value).TotalDays - rnd.Next(0, 5));
                db.Depoimentos.Add(new Depoimento { AutorId = pet.AdotanteId!.Value, PetId = pet.Id, Titulo = historias[i].Titulo, Texto = string.Format(historias[i].Texto, pet.Nome), DataCriacao = agora.AddDays(-dias) });
                Notificar(pet.UsuarioDoadorId, pet.AdotanteId!.Value, "historia", $"{Nome(pet.AdotanteId!.Value)} contou a história de {pet.Nome} no Final Feliz 💛", "/final-feliz", $"historia:{pet.Id}", agora.AddDays(-dias));
            }

            // ===================== CONVERSAS =====================
            var disponiveis = pets.Where(p => p.Status != "Adotado").OrderBy(_ => rnd.Next()).Take(10).ToList();
            var perguntas = new[]
            {
                ("Oi! O(a) {0} ainda está disponível? Me apaixonei pelas fotos 😍", "Está sim! Quer marcar uma visita para conhecer?", "Quero! Pode ser sábado de manhã?", "Combinado, te espero às 10h 🐾"),
                ("Olá! Tenho quintal grande e outro cão. O(a) {0} se dá bem com outros animais?", "Se dá super bem! Convive com vários aqui no abrigo.", "Que ótimo! Como funciona a adoção?", "Você vem conhecer, e se der match eu te envio o termo aqui pelo chat mesmo."),
                ("Boa tarde! Moro em apartamento, o(a) {0} se adapta?", "Se adapta sim, só precisa de passeios diários.", "Perfeito, passeio todo dia de manhã e à noite.", "Então vocês vão se dar muito bem! 💛"),
            };
            foreach (var pet in disponiveis)
            {
                var interessado = Alguem();
                if (interessado.Id == pet.UsuarioDoadorId) continue;
                var c = new Conversa { IniciadorId = interessado.Id, DestinatarioId = pet.UsuarioDoadorId, PetId = pet.Id, DataCriacao = agora.AddDays(-rnd.Next(0, 15)) };
                db.Conversas.Add(c);
                await db.SaveChangesAsync();
                var (m1, m2, m3, m4) = perguntas[rnd.Next(perguntas.Length)];
                var t = c.DataCriacao;
                var msgs = new[] { (true, string.Format(m1, pet.Nome)), (false, m2), (true, m3), (false, m4) };
                for (var k = 0; k < msgs.Length; k++)
                {
                    t = t.AddMinutes(rnd.Next(3, 180));
                    if (t > agora) t = agora.AddMinutes(-(msgs.Length - k));
                    db.Mensagens.Add(new Mensagem { ConversaId = c.Id, RemetenteId = msgs[k].Item1 ? interessado.Id : pet.UsuarioDoadorId, Texto = msgs[k].Item2, DataEnvio = t, Lida = k < msgs.Length - 1 });
                }
                c.UltimaMensagemEm = t;
            }
            await db.SaveChangesAsync();

            // ===================== RANKINGS (avisa quem está no pódio desde o início) =====================
            var podio = await db.Pets.Where(p => p.Status == "Adotado" && p.AdotanteId != null)
                .GroupBy(p => p.AdotanteId!.Value).Select(g => new { Id = g.Key, Total = g.Count() })
                .OrderByDescending(x => x.Total).Take(3).ToListAsync();
            for (var i = 0; i < podio.Count; i++)
                notificacoes.Add(new Notificacao
                {
                    UsuarioId = podio[i].Id, Tipo = "ranking", DataCriacao = agora.AddHours(-i - 1),
                    Texto = $"{(i switch { 0 => "🥇", 1 => "🥈", _ => "🥉" })} Você está em {i + 1}º lugar no ranking de adotantes de todos os tempos!",
                    Link = "/estatisticas?periodo=tudo#rankings", Chave = $"ranking:adotantes:tudo:{i + 1}"
                });

            db.Notificacoes.AddRange(notificacoes);
            await db.SaveChangesAsync();
            log.LogInformation("Terceira leva criada: {Pessoas} pessoas, {Ongs} ONGs, {Pets} pets, {Posts} publicações, {Notif} notificações.",
                pessoas.Count, ongs.Count, pets.Count, posts.Count, notificacoes.Count);
        }

        // "Débora" -> "debora" (e-mails de demonstração sem acento)
        private static string Simplificar(string s)
        {
            var normal = s.Normalize(System.Text.NormalizationForm.FormD);
            return new string(normal.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
        }
    }
}
