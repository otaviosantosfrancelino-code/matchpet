using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Data
{
    // Segunda leva de cadastros de demonstração: mais cidades, pessoas, ONGs, pets, itens com fotos reais,
    // amizades, publicações, vídeos, comentários, conversas, termos assinados e histórias do Final Feliz.
    // Roda uma única vez (se a conta lucas.almeida@demo... já existir, não faz nada).
    // Fotos: dog.ceo, thecatapi.com, Wikimedia Commons (licenças livres) e randomuser.me (retratos de exemplo).
    public static class DadosDemonstracaoComunidade
    {
        private const string D = DadosDemonstracao.DominioDemo;

        // ----- Fotos -----
        private const string Dog = "https://images.dog.ceo/breeds/";
        private const string Cat = "https://cdn2.thecatapi.com/images/";
        private const string Wiki = "https://thumb.wikimedia.org/wikipedia/commons/thumb/";
        private const string WikiOrig = "https://upload.wikimedia.org/wikipedia/commons/";

        private static string Mulher(int n) => $"https://randomuser.me/api/portraits/women/{n}.jpg";
        private static string Homem(int n) => $"https://randomuser.me/api/portraits/men/{n}.jpg";

        private static readonly string[] Videos =
        {
            WikiOrig + "2/21/Puppy_playing.webm",
            WikiOrig + "f/f3/Kitten_playing_-_Tokyo_-_Jan_7_2020.webm",
            WikiOrig + "7/7a/Cat_playing_with_a_laser_pointer.webm",
            WikiOrig + "0/04/Goldendoodle_and_Black_Lab_puppies_play_fighting_in_slow_motion.webm",
            WikiOrig + "5/54/Goldendoodle_and_Black_Lab_puppies_playing_in_slow_motion.webm",
            WikiOrig + "d/da/Cat_playing_with_a_round_track_toy.webm",
        };

        // Fotos reais dos produtos do Bazar (Wikimedia Commons)
        private static class Foto
        {
            public const string RacaoCao = Wiki + "b/b3/CHAMPS_dry_dog_food_pack_by_PENTA_GROUP.jpg/960px-CHAMPS_dry_dog_food_pack_by_PENTA_GROUP.jpg";
            public const string ComedouroGato = Wiki + "f/f7/A_cat_pauses_over_a_bowl_of_food%2C_2011.jpg/960px-A_cat_pauses_over_a_bowl_of_food%2C_2011.jpg";
            public const string RacaoUmidaGato = WikiOrig + "e/e1/Cat_eating_wet_plant-based_cat_food.jpg";
            public const string CaminhaVermelha = WikiOrig + "f/f4/Dog_sleeping_in_a_dog_bed.JPG";
            public const string CaminhaAzul = Wiki + "b/bd/Small_dog_in_doggy_bed.jpg/960px-Small_dog_in_doggy_bed.jpg";
            public const string Arranhador = Wiki + "c/ca/Homemade_scratching_post.jpg/960px-Homemade_scratching_post.jpg";
            public const string ArranhadorTorre = Wiki + "4/48/Scratching_post.jpg/330px-Scratching_post.jpg";
            public const string GaiolaPassaro = Wiki + "a/a0/Aratinga_canicularis_-_two_pets_in_small_cage-4a.jpg/960px-Aratinga_canicularis_-_two_pets_in_small_cage-4a.jpg";
            public const string GaiolaMadeira = Wiki + "3/34/Timber_Bird_Cage_vintage.jpg/960px-Timber_Bird_Cage_vintage.jpg";
            public const string Guia = WikiOrig + "3/3e/PPD-Leash-PinkGreenStripes.jpg";
            public const string Coleira = Wiki + "2/22/Leather_dog_collar_made_by_Les_cuirs_d%27Agathe_%28DSC06406%29.jpg/960px-Leather_dog_collar_made_by_Les_cuirs_d%27Agathe_%28DSC06406%29.jpg";
            public const string ColeiraVerde = Wiki + "e/e4/Leather_dog_collar_made_by_Les_cuirs_d%27Agathe_%28DSC07529%29.jpg/960px-Leather_dog_collar_made_by_Les_cuirs_d%27Agathe_%28DSC07529%29.jpg";
            public const string CaixaTransporte = Wiki + "f/f4/Dogtainers_Pet_Transport_Clipper_Cat_Cage_Plastic_Travel_Crate_Labelled.jpg/960px-Dogtainers_Pet_Transport_Clipper_Cat_Cage_Plastic_Travel_Crate_Labelled.jpg";
            public const string MochilaTransporte = Wiki + "3/38/Ruffit_Dog_Carrier.jpg/960px-Ruffit_Dog_Carrier.jpg";
            public const string BolsaGato = Wiki + "6/64/Cat_in_soft_side_cat_carrier.jpg/960px-Cat_in_soft_side_cat_carrier.jpg";
            public const string AquarioHamster = Wiki + "5/55/Finished_hamster_habitat_project.jpg/960px-Finished_hamster_habitat_project.jpg";
            public const string GaiolaHamster = Wiki + "4/4b/Home_built_enclosure_for_a_Syrian_Hamster.png/960px-Home_built_enclosure_for_a_Syrian_Hamster.png";
            public const string Peitoral = WikiOrig + "f/ff/Best.Bocce.Mascot.Ever_-HRD_-Milo_%2813976934936%29.jpg";
            public const string RatinhoGato = Wiki + "4/4b/Tabby_Cat_with_Toy_Mouse.jpg/960px-Tabby_Cat_with_Toy_Mouse.jpg";
            public const string BolinhaGato = Wiki + "6/6f/Tetrakis_cuboctahedron_cat_toy.jpg/960px-Tetrakis_cuboctahedron_cat_toy.jpg";
            public const string Meias = Wiki + "0/0c/Dog_socks_%287943172506%29.jpg/960px-Dog_socks_%287943172506%29.jpg";
            public const string Roupinha = Wiki + "d/d4/Dog_sweater_2.jpg/960px-Dog_sweater_2.jpg";
            public const string PaAreia = Wiki + "5/53/Blue_cat-shaped_litter_scoop_or_sifter_-_front_view.jpg/960px-Blue_cat-shaped_litter_scoop_or_sifter_-_front_view.jpg";
            public const string CaixaAreia = WikiOrig + "4/48/Modkat_Cat_Litter_Box_White_by_Modko.jpg";
            public const string Bolinha = Wiki + "d/db/Dog_ball_and_Chihuahua_2026-05-03.jpg/960px-Dog_ball_and_Chihuahua_2026-05-03.jpg";
            public const string Casinha = Wiki + "5/5b/Casa_para_perros.jpg/960px-Casa_para_perros.jpg";
            public const string CasinhaVermelha = Wiki + "e/e5/Red_wooden_doghouse_in_Tuntorp_1.jpg/960px-Red_wooden_doghouse_in_Tuntorp_1.jpg";
        }

        public static async Task CriarAsync(AppDbContext db, ILogger log)
        {
            if (await db.Usuarios.AnyAsync(u => u.Email == "lucas.almeida" + D)) return;
            log.LogInformation("Criando cadastros de demonstração da Comunidade...");

            var rnd = new Random(2026);
            var agora = DateTime.UtcNow;
            var senha = SenhaService.GerarHash(DadosDemonstracao.SenhaDemo);

            // ===================== 1. FOTOS REAIS NOS ITENS ANTIGOS =====================
            var fotosItensAntigos = new Dictionary<string, string>
            {
                ["Ração Premium 15kg"] = Foto.RacaoCao,
                ["Ração para Gatos 3kg"] = Foto.RacaoUmidaGato,
                ["Casinha de Madeira G"] = Foto.Casinha,
                ["Caminha Acolchoada"] = Foto.CaminhaAzul,
                ["Arranhador com Torre"] = Foto.ArranhadorTorre,
                ["Gaiola para Pássaros"] = Foto.GaiolaPassaro,
                ["Coleira + Guia Peitoral"] = Foto.Guia,
                ["Caixa de Transporte"] = Foto.CaixaTransporte,
                ["Comedouro Duplo Inox"] = Foto.ComedouroGato,
            };
            var itensAntigos = await db.ItensDoacao.Include(i => i.Imagens)
                .Where(i => i.UsuarioDoador!.Email.EndsWith(D)).ToListAsync();
            foreach (var item in itensAntigos)
            {
                if (!fotosItensAntigos.TryGetValue(item.Nome, out var url)) continue;
                foreach (var img in item.Imagens.Where(i => i.Base64Data.StartsWith("data:image/svg"))) img.Base64Data = url;
                item.FotoUrl = "";
            }

            // Datas mais realistas nos pets antigos de demonstração
            var petsAntigos = await db.Pets.Where(p => p.UsuarioDoador!.Email.EndsWith(D)).ToListAsync();
            foreach (var p in petsAntigos)
            {
                p.DataCadastro = agora.AddDays(-rnd.Next(20, 200));
                if (p.Status == "Adotado") p.DataAdocao = p.DataCadastro.AddDays(rnd.Next(7, 60));
            }
            await db.SaveChangesAsync();

            // ===================== 2. PESSOAS E ONGs =====================
            Usuario Conta(string nome, string email, string doc, string cidade, string bairro, string fone, string? foto, string bio,
                bool ong = false, string? pix = null, int diasConta = 120) => new()
            {
                Nome = nome, Email = email + D, Documento = doc, Telefone = fone, Senha = senha,
                CEP = "14400-000", Logradouro = "Rua de Demonstração", Numero = rnd.Next(10, 999).ToString(), Bairro = bairro,
                Cidade = cidade, Estado = "SP", EhOng = ong, ChavePix = pix, DescricaoOng = ong ? bio : null, Bio = bio,
                FotoPerfilBase64 = foto, StatusValidacaoDocumento = "Aprovado", DocumentoObservacao = "Conta de demonstração.",
                DataValidacaoDocumento = agora, Classificacao = ong ? "ONG Verificada" : "Usuário Verificado",
                EmailConfirmado = true, CodigoConfirmacao = "ISENTO", AceitouTermos = true, DataAceiteTermos = agora,
                IpAceite = "seed", DataCadastro = agora.AddDays(-diasConta)
            };
            string Cpf(int n) => DadosDemonstracao.GerarCpf($"92{n:0000000}");
            string Cnpj(int n) => DadosDemonstracao.GerarCnpj($"92{n:000000}0001");

            var pessoas = new List<Usuario>
            {
                Conta("Lucas Almeida", "lucas.almeida", Cpf(1), "Franca", "Jardim Petráglia", "(16) 99301-1001", Homem(32), "Pai do Thor e da Mel. Corredor de fim de semana e voluntário em feiras de adoção. 🐕🏃", diasConta: 300),
                Conta("Beatriz Rocha", "beatriz.rocha", Cpf(2), "Franca", "Cidade Nova", "(16) 99301-1002", Mulher(44), "Veterinária recém-formada. Apaixonada por gatos e por adoção responsável 🐱💉", diasConta: 260),
                Conta("Rafael Mendes", "rafael.mendes", Cpf(3), "Ribeirão Preto", "Jardim Irajá", "(16) 99301-1003", Homem(45), "Fotógrafo de pets nas horas vagas. Se precisar de fotos para o anúncio, me chama! 📸", diasConta: 220),
                Conta("Camila Nogueira", "camila.nogueira", Cpf(4), "Batatais", "Centro", "(16) 99301-1004", Mulher(68), "Professora, mãe da Luna (vira-lata mais linda do mundo).", diasConta: 180),
                Conta("Thiago Ferreira", "thiago.ferreira", Cpf(5), "Patrocínio Paulista", "Centro", "(16) 99301-1005", Homem(11), "Moro em sítio e sempre tem espaço para mais um amigo de quatro patas. 🌳", diasConta: 400),
                Conta("Ana Clara Souza", "anaclara.souza", Cpf(6), "Franca", "Jardim Aeroporto", "(16) 99301-1006", Mulher(21), "Estudante de Direito. Defensora da Lei Sansão! ⚖️🐾", diasConta: 90),
                Conta("Pedro Henrique Costa", "pedro.costa", Cpf(7), "Cristais Paulista", "Centro", "(16) 99301-1007", Homem(76), "Adotei o Bento em 2025 e minha vida mudou.", diasConta: 150),
                Conta("Larissa Martins", "larissa.martins", Cpf(8), "Sertãozinho", "Centro", "(16) 99301-1008", Mulher(65), "Lar temporário de gatinhos. Já são 23 encaminhados para novas famílias 💛", diasConta: 330),
                Conta("Gabriel Oliveira", "gabriel.oliveira", Cpf(9), "Ituverava", "Centro", "(16) 99301-1009", Homem(52), "Adestrador. Dicas de comportamento canino toda semana.", diasConta: 210),
                Conta("Fernanda Lima", "fernanda.lima", Cpf(10), "Araraquara", "Centro", "(16) 99301-1010", Mulher(17), "Mãe de 3 gatos e 1 calopsita. Casa cheia, coração também.", diasConta: 120),
                Conta("Bruno Carvalho", "bruno.carvalho", Cpf(11), "São Carlos", "Centro", "(16) 99301-1011", Homem(64), "Programador e tutor do Biscoito. 💻🐶", diasConta: 75),
                Conta("Isabela Ribeiro", "isabela.ribeiro", Cpf(12), "Franca", "Vila Industrial", "(16) 99301-1012", Mulher(29), "Faço caminhas e roupinhas para pets e doo parte para ONGs da cidade 🧶", diasConta: 260),
                Conta("Marcos Vinícius Santos", "marcos.santos", Cpf(13), "Pedregulho", "Centro", "(16) 99301-1013", Homem(85), "Aposentado, encontrei no Pandoro um grande companheiro.", diasConta: 140),
                Conta("Juliana Freitas", "juliana.freitas", Cpf(14), "Restinga", "Centro", "(16) 99301-1014", Mulher(90), "Protetora independente. Resgates na zona rural.", diasConta: 500),
                Conta("Diego Araújo", "diego.araujo", Cpf(15), "Campinas", "Cambuí", "(19) 99301-1015", Homem(22), "Mudei de cidade, mas sigo ajudando a ONG de Franca de longe.", diasConta: 60),
                Conta("Natália Pereira", "natalia.pereira", Cpf(16), "São Paulo", "Vila Mariana", "(11) 99301-1016", Mulher(57), "Apaixonada por cães idosos. Adotar um 'velhinho' é amor em dobro.", diasConta: 45),
            };
            var ongs = new List<Usuario>
            {
                Conta("Projeto Quatro Patas Ituverava", "quatropatas", Cnpj(1), "Ituverava", "Centro", "(16) 99401-2001", null,
                    "Castramos mais de 2.000 animais desde 2018 e mantemos um abrigo com 60 cães e gatos esperando um lar.", true, "pix.quatropatas" + D, 700),
                Conta("Gatil Esperança Sertãozinho", "gatilesperanca", Cnpj(2), "Sertãozinho", "Centro", "(16) 99401-2002", null,
                    "Gatil especializado em felinos resgatados. Todos saem castrados, vacinados e testados para FIV/FeLV.", true, "pix.gatil" + D, 520),
                Conta("ONG Amor de Bicho Araraquara", "amordebicho", Cnpj(3), "Araraquara", "Centro", "(16) 99401-2003", null,
                    "Feiras de adoção todo segundo sábado do mês na praça central. Aceitamos doações de ração e medicamentos.", true, "pix.amordebicho" + D, 610),
                Conta("Patinhas Solidárias Patrocínio", "patinhassolidarias", Cnpj(4), "Patrocínio Paulista", "Centro", "(16) 99401-2004", null,
                    "Grupo de voluntários que cuida dos animais de rua de Patrocínio Paulista e região.", true, "pix.patinhassolidarias" + D, 380),
            };
            db.Usuarios.AddRange(pessoas);
            db.Usuarios.AddRange(ongs);
            await db.SaveChangesAsync();

            // Contas da primeira leva (para dar continuidade às histórias)
            var novosIds = pessoas.Concat(ongs).Select(u => u.Id).ToList();
            var antigos = await db.Usuarios.Where(u => u.Email.EndsWith(D) && !novosIds.Contains(u.Id)).ToListAsync();
            Usuario Antigo(string email) => antigos.First(u => u.Email == email + D);
            var patinhas = Antigo("patinhas"); var felinos = Antigo("amigosfelinos"); var sosBichos = Antigo("sosbichos");
            var mariana = Antigo("mariana"); var carlos = Antigo("carlos"); var juliana = Antigo("juliana");
            Usuario P(string email) => pessoas.First(u => u.Email == email + D);
            Usuario O(string email) => ongs.First(u => u.Email == email + D);

            // ===================== 3. PETS =====================
            var novosPets = new List<Pet>();
            Pet NovoPet(Usuario dono, string nome, string especie, string raca, string sexo, string porte, int idade, bool castrado,
                bool quintal, bool criancas, string descricao, string foto, string status = "Disponível", Usuario? adotante = null, int diasCadastro = 30)
            {
                var cadastro = agora.AddDays(-diasCadastro);
                var p = new Pet
                {
                    Nome = nome, Especie = especie, Raca = raca, Sexo = sexo, Porte = porte, Idade = idade, Castrado = castrado,
                    PrecisaDeQuintal = quintal, RecomendadoParaCriancas = criancas, Descricao = descricao, Status = status,
                    UsuarioDoadorId = dono.Id, DataCadastro = cadastro, FotoUrl = "",
                    Imagens = new List<PetImagem> { new() { Base64Data = foto } }
                };
                if (status == "Adotado")
                {
                    p.AdotanteId = adotante?.Id;
                    p.DataAdocao = cadastro.AddDays(rnd.Next(5, Math.Max(6, Math.Min(70, diasCadastro - 1))));
                }
                novosPets.Add(p);
                return p;
            }

            // Disponíveis
            NovoPet(O("quatropatas"), "Paçoquinha", "Cão", "Beagle", "Fêmea", "Médio", 2, true, true, true, "Fareja tudo e ama passear. Convive bem com outros cães.", Dog + "beagle/n02088364_4706.jpg", diasCadastro: 12);
            NovoPet(O("quatropatas"), "Tyson", "Cão", "Boxer", "Macho", "Grande", 3, true, true, true, "Grandão brincalhão, muito carinhoso com crianças. Precisa de espaço.", Dog + "boxer/n02108089_12738.jpg", diasCadastro: 40);
            NovoPet(O("quatropatas"), "Frida", "Cão", "Boxer", "Fêmea", "Grande", 5, true, true, false, "Calma, protetora e muito leal. Ideal para casa com quintal.", Dog + "boxer/n02108089_1956.jpg", diasCadastro: 95);
            NovoPet(O("quatropatas"), "Zeus", "Cão", "Rottweiler", "Macho", "Grande", 4, true, true, false, "Treinado, obediente e dócil com a família. Indicado para tutores experientes.", Dog + "rottweiler/n02106550_10966.jpg", diasCadastro: 150);
            NovoPet(O("quatropatas"), "Kiara", "Cão", "Pit Bull", "Fêmea", "Médio", 3, true, true, true, "Doce como mel! Desmente qualquer preconceito com a raça.", Dog + "pitbull/20190801_154956.jpg", diasCadastro: 70);
            NovoPet(O("gatilesperanca"), "Mia", "Gato", "SRD", "Fêmea", "Pequeno", 1, true, false, true, "Rajadinha curiosa, adora caçar bolinhas de papel.", Cat + "247.jpg", diasCadastro: 8);
            NovoPet(O("gatilesperanca"), "Sombra", "Gato", "SRD", "Macho", "Médio", 3, true, false, true, "Gato preto lindo, tranquilo e muito companheiro.", Cat + "b1c.jpg", diasCadastro: 120);
            NovoPet(O("gatilesperanca"), "Pérola", "Gato", "SRD", "Fêmea", "Pequeno", 2, true, false, true, "Olhos de esmeralda e personalidade dócil. Testada negativo para FIV/FeLV.", Cat + "dar.jpg", diasCadastro: 33);
            NovoPet(O("gatilesperanca"), "Cinza", "Gato", "Azul Russo", "Macho", "Médio", 6, true, false, false, "Elegante e independente. Prefere casas mais calmas.", Cat + "s4wQfYoEk.jpg", diasCadastro: 210);
            NovoPet(O("amordebicho"), "Bidu", "Cão", "Border Collie", "Macho", "Médio", 2, true, true, true, "Super inteligente! Aprende comandos rapidinho. Precisa de atividade diária.", Dog + "collie-border/caesar.jpg", diasCadastro: 22);
            NovoPet(O("amordebicho"), "Pingo", "Cão", "Chihuahua", "Macho", "Pequeno", 6, true, false, true, "Pequeno no tamanho, gigante no amor. Perfeito para apartamento.", Dog + "chihuahua/msdaisy.jpg", diasCadastro: 18);
            NovoPet(O("amordebicho"), "Lili", "Cão", "Lhasa Apso", "Fêmea", "Pequeno", 8, true, false, true, "Idosinha carinhosa que só quer um colo quentinho.", Dog + "lhasa/n02098413_2607.jpg", diasCadastro: 180);
            NovoPet(O("amordebicho"), "Algodão", "Cão", "Maltês", "Macho", "Pequeno", 4, true, false, true, "Branquinho, cheiroso e muito brincalhão.", Dog + "maltese/n02085936_20313.jpg", diasCadastro: 27);
            NovoPet(O("patinhassolidarias"), "Fubá", "Cão", "SRD", "Fêmea", "Médio", 1, false, true, true, "Resgatada na estrada, hoje é só alegria. Castração já agendada.", Dog + "mix/xeshabelka_(23).jpg", diasCadastro: 6);
            NovoPet(O("patinhassolidarias"), "Duque", "Cão", "Corgi", "Macho", "Pequeno", 3, true, false, true, "Perninhas curtas, coração enorme.", Dog + "corgi-cardigan/n02113186_11741.jpg", diasCadastro: 44);
            NovoPet(O("patinhassolidarias"), "Mimi", "Gato", "SRD", "Fêmea", "Pequeno", 0, false, false, true, "Filhote de 2 meses, muito carinhosa.", Cat + "9no.jpg", diasCadastro: 4);
            NovoPet(P("larissa.martins"), "Tigresa", "Gato", "SRD", "Fêmea", "Pequeno", 2, true, false, true, "Lar temporário. Ela adora janelas teladas e sachê.", Cat + "c5q.jpg", diasCadastro: 15);
            NovoPet(P("larissa.martins"), "Félix", "Gato", "SRD", "Macho", "Médio", 4, true, false, true, "Gato frajola tranquilo, convive com cães.", Cat + "dhq.jpg", diasCadastro: 60);
            NovoPet(P("thiago.ferreira"), "Valente", "Cão", "Rottweiler", "Macho", "Grande", 6, true, true, false, "Guardião do sítio, mas precisa de um lar com menos cães.", Dog + "rottweiler/n02106550_12424.jpg", diasCadastro: 110);
            NovoPet(P("isabela.ribeiro"), "Bolota", "Cão", "Pug", "Fêmea", "Pequeno", 5, true, false, true, "Roncadora oficial da casa 😂. Ama sofá e petiscos.", Dog + "pug/n02110958_340.jpg", diasCadastro: 35);
            NovoPet(P("gabriel.oliveira"), "Max", "Cão", "Schnauzer", "Macho", "Pequeno", 7, true, false, true, "Adestrado, faz xixi no lugar certo e anda bem na guia.", Dog + "schnauzer-miniature/n02097047_1791.jpg", diasCadastro: 50);
            NovoPet(P("natalia.pereira"), "Dona Flor", "Cão", "Cocker Spaniel", "Fêmea", "Médio", 11, true, false, true, "Senhorinha de 11 anos, saudável e muito meiga. Procura uma aposentadoria feliz.", Dog + "spaniel-cocker/n02102318_3700.jpg", diasCadastro: 240);
            NovoPet(P("fernanda.lima"), "Lua", "Gato", "Bengal", "Fêmea", "Médio", 2, true, false, false, "Muito ativa, gosta de brincar de caçar. Ideal para quem tem tempo.", Cat + "mNOdlAkXR.jpg", diasCadastro: 20);

            // Em processo (alguém já recebeu o termo)
            var luke = NovoPet(O("amordebicho"), "Luke", "Cão", "Husky Siberiano", "Macho", "Grande", 2, true, true, true, "Olhos azuis hipnotizantes e muita energia!", Dog + "husky/n02110185_1446.jpg", "Em Processo", diasCadastro: 25);

            // Adotados (com quem adotou, para os perfis e o Final Feliz)
            var thor = NovoPet(O("quatropatas"), "Thor", "Cão", "Labrador", "Macho", "Grande", 3, true, true, true, "Adotado pelo Lucas! Hoje corre 5 km com ele todo domingo.", Dog + "labrador/n02099712_1930.jpg", "Adotado", P("lucas.almeida"), 260);
            var nina = NovoPet(felinos, "Nina Bolinha", "Gato", "SRD", "Fêmea", "Pequeno", 1, true, false, true, "Foi adotada pela Beatriz e virou a gerente da clínica.", Cat + "4m7.jpg", "Adotado", P("beatriz.rocha"), 230);
            var luna = NovoPet(O("patinhassolidarias"), "Luna Vira-Lata", "Cão", "SRD", "Fêmea", "Médio", 4, true, true, true, "Adotada pela Camila. Dorme na cama todos os dias (contra as regras 😅).", Dog + "mix/floki-the-chilean-mutt-1.jpg", "Adotado", P("camila.nogueira"), 170);
            var bento2 = NovoPet(sosBichos, "Bento Caramelo", "Cão", "SRD", "Macho", "Médio", 5, true, false, true, "Caramelo raiz, adotado pelo Pedro.", Dog + "retriever-golden/n02099601_109.jpg", "Adotado", P("pedro.costa"), 140);
            var pandoro = NovoPet(O("quatropatas"), "Pandoro", "Cão", "Pug", "Macho", "Pequeno", 9, true, false, true, "Velhinho adotado pelo Marcos. Os dois fazem caminhada toda manhã.", Dog + "pug/roxycruising.jpg", "Adotado", P("marcos.santos"), 130);
            var biscoito2 = NovoPet(O("amordebicho"), "Biscoito Jr.", "Cão", "Pomerânia", "Macho", "Pequeno", 2, true, false, true, "Adotado pelo Bruno em São Carlos.", Dog + "pomeranian/n02112018_2277.jpg", "Adotado", P("bruno.carvalho"), 70);
            var amora = NovoPet(O("gatilesperanca"), "Amora", "Gato", "SRD", "Fêmea", "Pequeno", 3, true, false, true, "Adotada pela Fernanda, virou melhor amiga da calopsita!", Cat + "2t9.jpg", "Adotado", P("fernanda.lima"), 100);
            var pipoca2 = NovoPet(P("juliana.freitas"), "Pipoca Rural", "Cão", "SRD", "Fêmea", "Médio", 2, true, true, true, "Resgatada no sítio e adotada pela Ana Clara.", Dog + "beagle/n02088364_9318.jpg", "Adotado", P("anaclara.souza"), 200);
            var rex = NovoPet(O("amordebicho"), "Rex", "Cão", "Labrador", "Macho", "Grande", 6, true, true, true, "Adotado pelo Diego e hoje mora em Campinas.", Dog + "labrador/n02099712_4462.jpg", "Adotado", P("diego.araujo"), 90);
            var bela = NovoPet(O("quatropatas"), "Bela", "Cão", "Dálmata", "Fêmea", "Grande", 4, true, true, true, "Adotada pela Natália: a prova de que adoção não tem idade.", Dog + "dalmatian/cooper1.jpg", "Adotado", P("natalia.pereira"), 300);
            var tom = NovoPet(O("gatilesperanca"), "Tom", "Gato", "SRD", "Macho", "Médio", 2, true, false, true, "Adotado pelo Rafael, estrela das fotos dele.", Cat + "xZysIjSqa.jpg", "Adotado", P("rafael.mendes"), 160);
            var bob2 = NovoPet(O("patinhassolidarias"), "Paçoca Corgi", "Cão", "Corgi", "Fêmea", "Pequeno", 3, true, false, true, "Adotada pelo Gabriel e já sabe 15 comandos!", Dog + "corgi-cardigan/n02113186_9579.jpg", "Adotado", P("gabriel.oliveira"), 115);

            db.Pets.AddRange(novosPets);
            await db.SaveChangesAsync();

            // Pets adotados da primeira leva ganham adotante
            var adotadosAntigos = petsAntigos.Where(p => p.Status == "Adotado").ToList();
            var adotantesAntigos = new[] { P("isabela.ribeiro"), P("thiago.ferreira"), P("larissa.martins"), juliana, carlos };
            for (var i = 0; i < adotadosAntigos.Count; i++) adotadosAntigos[i].AdotanteId = adotantesAntigos[i % adotantesAntigos.Length].Id;
            await db.SaveChangesAsync();

            // ===================== 4. ITENS DO BAZAR (fotos reais) =====================
            ItemDoacao Item(Usuario dono, string nome, string categoria, string destino, string descricao, string foto, string status = "Disponível") => new()
            {
                Nome = nome, Categoria = categoria, EspecieDestino = destino, Descricao = descricao, Status = status,
                UsuarioDoadorId = dono.Id, FotoUrl = "", DataCadastro = agora.AddDays(-rnd.Next(1, 90)),
                Imagens = new List<ItemImagem> { new() { Base64Data = foto } }
            };
            db.ItensDoacao.AddRange(
                Item(P("isabela.ribeiro"), "Roupinha de tricô P", "Brinquedo/Acessório", "Cão", "Feita à mão, ideal para o inverno. Tamanho P (até 6 kg).", Foto.Roupinha),
                Item(P("isabela.ribeiro"), "Kit 4 meias antiderrapantes", "Brinquedo/Acessório", "Cão", "Para pisos lisos, ajuda cães idosos a não escorregar.", Foto.Meias),
                Item(P("isabela.ribeiro"), "Caminha redonda vermelha", "Casinha/Cama", "Todos", "Tamanho M, lavável, quase sem uso.", Foto.CaminhaVermelha),
                Item(P("beatriz.rocha"), "Comedouro automático", "Brinquedo/Acessório", "Gato", "Reservatório de 3 litros, solta ração aos poucos.", Foto.ComedouroGato),
                Item(P("beatriz.rocha"), "Caixa de areia fechada", "Remédio/Higiene", "Gato", "Com tampa e entrada por cima, evita espalhar areia.", Foto.CaixaAreia),
                Item(P("larissa.martins"), "Pá para areia em formato de gato", "Remédio/Higiene", "Gato", "Novinha, ainda com etiqueta.", Foto.PaAreia),
                Item(P("larissa.martins"), "Arranhador de sisal", "Brinquedo/Acessório", "Gato", "Arranhador de 70 cm, base firme.", Foto.Arranhador),
                Item(P("fernanda.lima"), "Ratinho de brinquedo (kit 5)", "Brinquedo/Acessório", "Gato", "Os gatos daqui enjoaram, os seus vão amar!", Foto.RatinhoGato),
                Item(P("fernanda.lima"), "Bolinha geométrica para gatos", "Brinquedo/Acessório", "Gato", "Bolinha vazada com guizo.", Foto.BolinhaGato),
                Item(P("fernanda.lima"), "Gaiola de madeira para pássaros", "Gaiola/Aquário", "Pássaro", "Peça decorativa e funcional, com dois poleiros.", Foto.GaiolaMadeira),
                Item(P("thiago.ferreira"), "Casinha de madeira com telhado", "Casinha/Cama", "Cão", "Para cães médios. Telhado impermeabilizado.", Foto.CasinhaVermelha),
                Item(P("gabriel.oliveira"), "Peitoral ajustável G", "Brinquedo/Acessório", "Cão", "Peitoral acolchoado, ótimo para cães que puxam.", Foto.Peitoral),
                Item(P("gabriel.oliveira"), "Bolinhas de borracha (kit 3)", "Brinquedo/Acessório", "Cão", "Resistentes, boas para buscar.", Foto.Bolinha),
                Item(P("rafael.mendes"), "Mochila de transporte", "Brinquedo/Acessório", "Cão", "Para cães até 8 kg. Ótima para passeios e veterinário.", Foto.MochilaTransporte),
                Item(P("bruno.carvalho"), "Bolsa de transporte para gatos", "Brinquedo/Acessório", "Gato", "Bolsa macia com tela de ventilação.", Foto.BolsaGato),
                Item(P("diego.araujo"), "Habitat para hamster (aquário)", "Gaiola/Aquário", "Outros", "Aquário de 60 litros adaptado com casinha e roda.", Foto.AquarioHamster),
                Item(P("camila.nogueira"), "Gaiola grande para hamster", "Gaiola/Aquário", "Outros", "Com dois andares e túnel.", Foto.GaiolaHamster),
                Item(O("quatropatas"), "Coleira de couro M", "Brinquedo/Acessório", "Cão", "Doação de parceiro, nova.", Foto.ColeiraVerde),
                Item(O("amordebicho"), "Coleira de couro marrom", "Brinquedo/Acessório", "Cão", "Tamanho G, couro legítimo.", Foto.Coleira),
                Item(O("gatilesperanca"), "Ração úmida para gatos (caixa 12 sachês)", "Ração/Alimento", "Gato", "Validade 10/2027. Sabor salmão.", Foto.RacaoUmidaGato),
                Item(O("patinhassolidarias"), "Casinha de madeira dupla", "Casinha/Cama", "Cão", "Duas casinhas unidas, para dois cães pequenos.", Foto.Casinha, "Doado"),
                Item(P("marcos.santos"), "Ração 10 kg para cães idosos", "Ração/Alimento", "Cão", "Saco lacrado, sabor frango.", Foto.RacaoCao, "Doado")
            );
            await db.SaveChangesAsync();

            // ===================== 5. AMIZADES =====================
            var todos = pessoas.Concat(ongs).Concat(antigos).ToList();
            var pares = new HashSet<(int, int)>();
            void Amigos(Usuario a, Usuario b, bool aceita = true)
            {
                if (a.Id == b.Id || pares.Contains((a.Id, b.Id)) || pares.Contains((b.Id, a.Id))) return;
                pares.Add((a.Id, b.Id));
                db.Amizades.Add(new Amizade { SolicitanteId = a.Id, DestinatarioId = b.Id, Status = aceita ? "Aceita" : "Pendente", DataCriacao = agora.AddDays(-rnd.Next(1, 120)), DataResposta = aceita ? agora.AddDays(-rnd.Next(0, 30)) : null });
            }
            foreach (var p in pessoas)
            {
                // Cada pessoa é amiga de 4 a 8 outras e segue algumas ONGs
                foreach (var outro in todos.OrderBy(_ => rnd.Next()).Take(rnd.Next(4, 9))) Amigos(p, outro);
            }
            // Alguns pedidos pendentes para Mariana e Juliana (contas antigas) verem a função
            Amigos(P("rafael.mendes"), mariana, false);
            Amigos(P("anaclara.souza"), mariana, false);
            Amigos(P("gabriel.oliveira"), juliana, false);
            await db.SaveChangesAsync();

            // ===================== 6. PUBLICAÇÕES, CURTIDAS E COMENTÁRIOS =====================
            var posts = new List<Postagem>();
            Postagem Post(Usuario autor, string texto, string? midia, string? tipo, Pet? pet, int diasAtras)
            {
                var p = new Postagem { AutorId = autor.Id, Texto = texto, MidiaUrl = midia, TipoMidia = tipo, PetId = pet?.Id, DataCriacao = agora.AddDays(-diasAtras).AddHours(-rnd.Next(0, 23)) };
                posts.Add(p);
                return p;
            }
            string Img(Pet p) => p.Imagens.First().Base64Data;

            Post(P("lucas.almeida"), "Domingo de corrida com o Thor! 5 km e ele ainda queria mais 😅🐕 #adotenaocompre", Img(thor), "imagem", thor, 1);
            Post(P("beatriz.rocha"), "A Nina já se acha dona da clínica. Recebe os pacientes na recepção todo dia 😂", Img(nina), "imagem", nina, 2);
            Post(P("camila.nogueira"), "Um ano de Luna em casa. Melhor decisão que tomamos! 💛", Img(luna), "imagem", luna, 3);
            Post(O("quatropatas"), "🎉 MUTIRÃO DE CASTRAÇÃO neste sábado! Vagas limitadas para cães e gatos de famílias de baixa renda. Inscrições pelo chat.", null, null, null, 2);
            Post(O("amordebicho"), "Feira de adoção confirmada para o próximo sábado, 9h, na praça central de Araraquara. Venha conhecer o Bidu, a Lili e o Algodão! 🐶", Dog + "collie-border/caesar.jpg", "imagem", null, 4);
            Post(P("rafael.mendes"), "Ensaio novo com o Tom. Esse olhar não existe! 📸", Img(tom), "imagem", tom, 5);
            Post(P("gabriel.oliveira"), "Dica de adestramento: recompense SEMPRE o comportamento certo na hora em que ele acontece. 3 segundos depois o cão já não associa! 🦴", null, null, null, 6);
            Post(P("gabriel.oliveira"), "A Paçoca aprendendo o 'fica' 🐾", Videos[3], "video", bob2, 7);
            Post(P("larissa.martins"), "Gatinhos do lar temporário brincando. Quem quer adotar uma dessas fofuras? 🐱", Videos[1], "video", null, 3);
            Post(P("fernanda.lima"), "Amora descobriu o laser 😂", Videos[2], "video", amora, 8);
            Post(P("bruno.carvalho"), "Meu home office agora tem fiscal. Biscoito aprova cada linha de código 💻🐶", Img(biscoito2), "imagem", biscoito2, 9);
            Post(P("marcos.santos"), "Pandoro e eu na caminhada das 6h. Aposentado também precisa de rotina! 👴🐾", Img(pandoro), "imagem", pandoro, 10);
            Post(P("natalia.pereira"), "Adotar um cão adulto foi a melhor coisa que fiz. A Bela chegou educada e cheia de amor. Pensem nos 'velhinhos' dos abrigos 💛", Img(bela), "imagem", bela, 11);
            Post(O("gatilesperanca"), "Precisamos de ração úmida e areia sanitária! Qualquer ajuda faz diferença. Chave Pix no nosso perfil 🙏", Cat + "dar.jpg", "imagem", null, 4);
            Post(P("isabela.ribeiro"), "Fiz 20 roupinhas de tricô para o inverno e vou doar todas para as ONGs da cidade 🧶❄️", Foto.Roupinha, "imagem", null, 12);
            Post(P("pedro.costa"), "Bento no sofá depois do banho. Cheirosinho e folgado.", Img(bento2), "imagem", bento2, 13);
            Post(P("diego.araujo"), "Rex se adaptou super bem a Campinas! Obrigado Amor de Bicho por todo o cuidado 💛", Img(rex), "imagem", rex, 14);
            Post(P("thiago.ferreira"), "Hora da brincadeira no sítio 🐶🌳", Videos[0], "video", null, 6);
            Post(P("anaclara.souza"), "Lembrete: maus-tratos a cães e gatos dá até 5 anos de reclusão (Lei 14.064/2020). Denuncie! ⚖️", null, null, null, 15);
            Post(P("anaclara.souza"), "Pipoca no primeiro dia em casa vs. hoje. Que transformação! 🥹", Img(pipoca2), "imagem", pipoca2, 16);
            Post(O("patinhassolidarias"), "A Fubá foi resgatada na estrada semana passada. Hoje está saudável e pronta para um lar ❤️", Dog + "mix/xeshabelka_(23).jpg", "imagem", null, 5);
            Post(P("juliana.freitas"), "Mais um resgate na zona rural. Obrigada a todos que ajudaram com a ração! 🙏", null, null, null, 18);
            Post(P("larissa.martins"), "Diversão garantida com brinquedo de trilha 😻", Videos[5], "video", null, 9);
            Post(P("lucas.almeida"), "Os filhotes da feira estavam assim 😍", Videos[4], "video", null, 20);
            Post(mariana, "Fumaça ainda procura uma família! Ele é um amor de cachorro, super calmo 🐶", null, null, null, 21);
            Post(felinos, "Neve continua esperando um lar. Ela é calma e linda! Venha conhecer 🤍", null, null, null, 22);
            Post(P("beatriz.rocha"), "Vacina em dia é amor! Lembrete de que a antirrábica é anual 💉", null, null, null, 25);
            Post(P("camila.nogueira"), "Comprei uma caminha nova e adivinhem onde a Luna dorme? Na minha cama. 🤷‍♀️", null, null, null, 27);
            Post(O("amordebicho"), "Luke está em processo de adoção! Torçam por ele 🤞🐺", Img(luke), "imagem", luke, 1);
            Post(P("fernanda.lima"), "Bom dia com café e gato no colo ☕🐈", Cat + "c5q.jpg", "imagem", null, 30);
            db.Postagens.AddRange(posts);
            await db.SaveChangesAsync();

            var comentariosModelo = new[]
            {
                "Que lindo! 😍", "Amei demais!", "Que fofura 💛", "Parabéns pela adoção!", "Que história incrível 🥹",
                "Muito bom ver isso!", "Compartilhando!", "Quero conhecer!", "Que olhar 😍", "Adote, não compre! 🐾",
                "Obrigado por divulgar!", "Já mandei mensagem pelo chat!", "Isso aquece o coração ❤️", "Que sorte a de vocês!",
            };
            foreach (var post in posts)
            {
                foreach (var u in todos.OrderBy(_ => rnd.Next()).Take(rnd.Next(3, 15)))
                    db.Curtidas.Add(new Curtida { PostagemId = post.Id, UsuarioId = u.Id, DataCriacao = post.DataCriacao.AddHours(rnd.Next(1, 48)) });
                foreach (var u in todos.Where(x => x.Id != post.AutorId).OrderBy(_ => rnd.Next()).Take(rnd.Next(0, 4)))
                    db.Comentarios.Add(new Comentario { PostagemId = post.Id, AutorId = u.Id, Texto = comentariosModelo[rnd.Next(comentariosModelo.Length)], DataCriacao = post.DataCriacao.AddHours(rnd.Next(1, 72)) });
            }
            await db.SaveChangesAsync();

            // ===================== 7. CONVERSAS E TERMOS =====================
            async Task<Conversa> NovaConversa(Usuario de, Usuario para, Pet? pet, ItemDoacao? item, int diasAtras, params (bool doIniciador, string texto)[] msgs)
            {
                var c = new Conversa { IniciadorId = de.Id, DestinatarioId = para.Id, PetId = pet?.Id, ItemDoacaoId = item?.Id, DataCriacao = agora.AddDays(-diasAtras) };
                db.Conversas.Add(c);
                await db.SaveChangesAsync();
                var t = c.DataCriacao;
                foreach (var (doIniciador, texto) in msgs)
                {
                    t = t.AddMinutes(rnd.Next(3, 240));
                    db.Mensagens.Add(new Mensagem { ConversaId = c.Id, RemetenteId = doIniciador ? de.Id : para.Id, Texto = texto, DataEnvio = t, Lida = true });
                }
                c.UltimaMensagemEm = t;
                await db.SaveChangesAsync();
                return c;
            }

            async Task EnviarTermo(Conversa c, Pet pet, Usuario doador, Usuario adotante, bool assinado)
            {
                var termo = new TermoAdocao
                {
                    PetId = pet.Id, DoadorId = doador.Id, AdotanteId = adotante.Id, ConversaId = c.Id,
                    Texto = ChatService.GerarTexto(pet, doador, adotante), Status = assinado ? "Assinado" : "Aguardando assinatura",
                    Codigo = "MP-" + rnd.Next(1000, 9999) + "-" + rnd.Next(1000, 9999),
                    DataEnvio = c.UltimaMensagemEm.AddMinutes(30), IpDoador = "200.150.10." + rnd.Next(2, 250),
                    DataAssinatura = assinado ? c.UltimaMensagemEm.AddHours(5) : null, IpAdotante = assinado ? "177.80.20." + rnd.Next(2, 250) : null
                };
                db.TermosAdocao.Add(termo);
                await db.SaveChangesAsync();
                db.Mensagens.Add(new Mensagem { ConversaId = c.Id, RemetenteId = doador.Id, Tipo = "termo", TermoAdocaoId = termo.Id, Texto = $"Termo de adoção de {pet.Nome}", DataEnvio = termo.DataEnvio, Lida = true });
                if (assinado)
                    db.Mensagens.Add(new Mensagem { ConversaId = c.Id, RemetenteId = adotante.Id, Tipo = "sistema", Texto = "✅ Termo assinado! A adoção foi concluída. Parabéns pela nova família! 💛", DataEnvio = termo.DataAssinatura!.Value, Lida = true });
                c.UltimaMensagemEm = termo.DataAssinatura ?? termo.DataEnvio;
                await db.SaveChangesAsync();
            }

            var c1 = await NovaConversa(P("lucas.almeida"), O("quatropatas"), thor, null, 230,
                (true, "Olá! Vi o anúncio do Thor e tenho interesse em adotar. Podemos conversar? 🐾"),
                (false, "Oi, Lucas! Que bom! O Thor é muito ativo, você tem tempo para passeios?"),
                (true, "Tenho sim! Corro todo fim de semana e moro em casa com quintal."),
                (false, "Perfeito! Podemos marcar uma visita no sábado às 10h?"),
                (true, "Combinado! Até sábado."),
                (false, "Foi ótimo te conhecer. Vou te enviar o termo de adoção."));
            await EnviarTermo(c1, thor, O("quatropatas"), P("lucas.almeida"), true);

            var c2 = await NovaConversa(P("beatriz.rocha"), felinos, nina, null, 210,
                (true, "Olá! Sou veterinária e me apaixonei pela Nina. Ela ainda está disponível?"),
                (false, "Está sim! Ela é muito carinhosa. Quer vir conhecê-la?"),
                (true, "Quero! Posso amanhã depois do expediente."));
            await EnviarTermo(c2, nina, felinos, P("beatriz.rocha"), true);

            var c3 = await NovaConversa(P("natalia.pereira"), O("amordebicho"), luke, null, 3,
                (true, "Oi! O Luke é lindo demais. Sei que husky precisa de muita atividade, tenho um quintal grande."),
                (false, "Que bom, Natália! Ele precisa mesmo de espaço. Já teve husky?"),
                (true, "Já tive dois! Conheço bem a raça."),
                (false, "Maravilha! Vou te mandar o termo para você ler com calma."));
            await EnviarTermo(c3, luke, O("amordebicho"), P("natalia.pereira"), false);

            await NovaConversa(mariana, P("lucas.almeida"), null, null, 2,
                (true, "Lucas, vi suas fotos com o Thor! Você conhece alguém que queira adotar o Fumaça?"),
                (false, "Oi Mariana! Vou divulgar no grupo de corrida, tem muita gente que ama cachorro lá 🏃"),
                (true, "Muito obrigada! 💛"));

            await NovaConversa(juliana, P("fernanda.lima"), null, null, 1,
                (true, "Oi Fernanda! Você que tem calopsita, sabe se a gaiola grande é melhor para casal?"),
                (false, "Oi! Sim, quanto maior melhor. E coloca poleiros de alturas diferentes 🐦"));

            var itemCaixa = await db.ItensDoacao.FirstAsync(i => i.Nome == "Caixa de areia fechada");
            await NovaConversa(P("larissa.martins"), P("beatriz.rocha"), null, itemCaixa, 4,
                (true, "Oi Beatriz! A caixa de areia ainda está disponível? É para os gatinhos do lar temporário."),
                (false, "Está sim! Posso deixar na clínica para você retirar."),
                (true, "Perfeito, passo aí amanhã. Obrigada! 😻"));

            // ===================== 8. HISTÓRIAS DO FINAL FELIZ =====================
            void Historia(Usuario autor, Pet? pet, string titulo, string texto, int diasAtras) =>
                db.Depoimentos.Add(new Depoimento { AutorId = autor.Id, PetId = pet?.Id, Titulo = titulo, Texto = texto, DataCriacao = agora.AddDays(-diasAtras) });

            Historia(P("lucas.almeida"), thor, "O Thor me tirou do sedentarismo",
                "Eu vivia no sofá. Desde que o Thor chegou, corro 5 km todo domingo e perdi 8 kg. Ele me escolheu na feira e foi amor à primeira lambida. O termo pelo chat foi super simples e a ONG acompanhou a adaptação nas primeiras semanas.", 20);
            Historia(P("camila.nogueira"), luna, "Minha filha ganhou uma irmã",
                "A Luna chegou assustada, com medo de barulho. Hoje dorme abraçada com a minha filha e acompanha ela até o portão da escola todos os dias. Não imagino mais a nossa casa sem ela.", 35);
            Historia(P("marcos.santos"), pandoro, "Dois velhinhos fazendo companhia um para o outro",
                "Depois que fiquei viúvo, a casa ficou silenciosa demais. O Pandoro tem 9 anos, como eu ele também precisava de alguém. Fazemos caminhada às 6h e vemos o jornal juntos. Recomendo adotar animais idosos: eles são gratos todos os dias.", 18);
            Historia(P("natalia.pereira"), bela, "Adoção não tem idade",
                "Todo mundo queria filhote. A Bela tinha 4 anos e estava há meses no abrigo. Chegou educada, carinhosa e virou minha sombra. Se você está pensando em adotar, dê uma chance aos adultos!", 40);
            Historia(P("beatriz.rocha"), nina, "A gerente da clínica",
                "Adotei a Nina pelo Match-Pet e hoje ela fica na recepção da clínica. Os pacientes já perguntam por ela quando chegam! A conversa pelo chat com a ONG foi rápida e o termo ficou salvo na conversa, o que me deu muita segurança.", 25);
            Historia(P("diego.araujo"), rex, "Mudança de cidade, mesmo amor",
                "Adotei o Rex em Franca e me mudei para Campinas dois meses depois. Ele se adaptou super bem ao apartamento com passeios três vezes ao dia. A ONG ainda acompanha pelo chat e manda mensagem perguntando dele.", 12);
            Historia(P("anaclara.souza"), pipoca2, "De resgatada a rainha da casa",
                "A Pipoca foi encontrada na zona rural, magrinha e com medo. Hoje tem caminha, roupinha e até perfil na Comunidade! Obrigada à Juliana, que fez o resgate e confiou em mim.", 30);
            Historia(P("fernanda.lima"), amora, "Gata e calopsita: melhores amigas",
                "Todo mundo disse que não daria certo colocar uma gata em casa com calopsita. A Amora é a gata mais tranquila do mundo e passa horas olhando a Kiwi cantar. Cuidado e adaptação aos poucos fazem toda a diferença.", 8);
            Historia(P("pedro.costa"), bento2, "O Bento me ensinou a desacelerar",
                "Trabalho muito e chegava em casa estressado. Hoje tenho um motivo para sair do trabalho na hora: o Bento me espera no portão. Adotar foi a melhor decisão que tomei.", 50);
            await db.SaveChangesAsync();

            log.LogInformation("Cadastros de demonstração da Comunidade criados.");
        }
    }
}
