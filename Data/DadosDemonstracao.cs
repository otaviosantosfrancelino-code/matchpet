using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Data
{
    // Cadastros de demonstração (ONGs, pessoas, pets e itens) para o site não ficar vazio.
    // Roda na inicialização SÓ UMA VEZ: se já existir alguma conta @demo.matchpet.com.br, não faz nada.
    // Para desligar: "SeedDemo": false no appsettings.json.
    // Todas as contas de demonstração usam a senha: Demo@123
    public static class DadosDemonstracao
    {
        public const string DominioDemo = "@demo.matchpet.com.br";
        public const string SenhaDemo = "Demo@123";

        public static async Task CriarAsync(AppDbContext db, ILogger log)
        {
            if (await db.Usuarios.AnyAsync(u => u.Email.EndsWith(DominioDemo))) return;

            log.LogInformation("Criando cadastros de demonstração...");
            var senha = SenhaService.GerarHash(SenhaDemo);
            var agora = DateTime.UtcNow;

            Usuario Pessoa(string nome, string email, string cpfBase, string cidade, string fone, bool ong = false, string? pix = null, string? descricao = null) => new()
            {
                Nome = nome,
                Email = email + DominioDemo,
                Documento = ong ? GerarCnpj(cpfBase) : GerarCpf(cpfBase),
                Telefone = fone,
                Senha = senha,
                CEP = "14400-000",
                Logradouro = "Rua de Demonstração",
                Numero = "100",
                Bairro = "Centro",
                Cidade = cidade,
                Estado = "SP",
                EhOng = ong,
                ChavePix = pix,
                DescricaoOng = descricao,
                // Contas fictícias já marcadas como verificadas para a demonstração funcionar
                StatusValidacaoDocumento = "Aprovado",
                DocumentoObservacao = "Conta de demonstração.",
                DataValidacaoDocumento = agora,
                Classificacao = ong ? "ONG Verificada" : "Usuário Verificado",
                EmailConfirmado = true,
                CodigoConfirmacao = "ISENTO",
                AceitouTermos = true,
                DataAceiteTermos = agora,
                IpAceite = "seed",
                DataCadastro = agora.AddDays(-60)
            };

            var patinhas = Pessoa("Instituto Patinhas de Franca", "patinhas", "910000010001", "Franca", "(16) 99100-1001", true,
                "pix.patinhas" + DominioDemo, "Resgatamos cães e gatos abandonados em Franca desde 2015. Todos os animais saem vacinados, vermifugados e castrados.");
            var felinos = Pessoa("Associação Amigos dos Felinos", "amigosfelinos", "910000020001", "Franca", "(16) 99100-2002", true,
                "pix.felinos" + DominioDemo, "Lar temporário especializado em gatos. Promovemos feiras de adoção aos sábados e campanhas de castração.");
            var sosBichos = Pessoa("SOS Bichos Ribeirão", "sosbichos", "910000030001", "Ribeirão Preto", "(16) 99100-3003", true,
                "pix.sosbichos" + DominioDemo, "Atuamos na região de Ribeirão Preto com resgate de animais em situação de rua e maus-tratos.");
            var mariana = Pessoa("Mariana Souza", "mariana", "910000001", "Franca", "(16) 99200-1001");
            var carlos = Pessoa("Carlos Henrique Lima", "carlos", "910000002", "Batatais", "(16) 99200-2002");
            var juliana = Pessoa("Juliana Pereira", "juliana", "910000003", "Franca", "(16) 99200-3003");

            db.Usuarios.AddRange(patinhas, felinos, sosBichos, mariana, carlos, juliana);
            await db.SaveChangesAsync();

            Pet NovoPet(Usuario dono, string nome, string especie, string raca, string sexo, string porte, int idade,
                bool castrado, bool quintal, bool criancas, string descricao, string status, params string[] fotos) => new()
            {
                Nome = nome, Especie = especie, Raca = raca, Sexo = sexo, Porte = porte, Idade = idade,
                Castrado = castrado, PrecisaDeQuintal = quintal, RecomendadoParaCriancas = criancas,
                Descricao = descricao, Status = status, UsuarioDoadorId = dono.Id,
                FotoUrl = fotos.FirstOrDefault() ?? "",
                Imagens = fotos.Select(f => new PetImagem { Base64Data = f }).ToList()
            };

            const string dog = "https://images.dog.ceo/breeds/";
            const string cat = "https://cdn2.thecatapi.com/images/";

            db.Pets.AddRange(
                NovoPet(patinhas, "Mel", "Cão", "Golden Retriever", "Fêmea", "Grande", 3, true, true, true,
                    "Mel é pura doçura! Adora brincar de buscar a bolinha e é ótima com crianças. Precisa de espaço para correr.", "Disponível",
                    dog + "retriever-golden/n02099601_4843.jpg"),
                NovoPet(patinhas, "Thor", "Cão", "Labrador", "Macho", "Grande", 1, true, true, true,
                    "Filhote cheio de energia, muito sociável com outros cães. Já sabe sentar e dar a pata.", "Disponível",
                    dog + "labrador/n02099712_7963.jpg"),
                NovoPet(patinhas, "Snoopy", "Cão", "Beagle", "Macho", "Médio", 4, true, true, true,
                    "Curioso e brincalhão, ama passeios. Convive bem com gatos.", "Disponível",
                    dog + "beagle/n02088364_9825.jpg", dog + "beagle/n02088364_15877.jpg"),
                NovoPet(patinhas, "Luna", "Cão", "Husky Siberiano", "Fêmea", "Grande", 2, true, true, false,
                    "Linda e muito ativa. Indicada para tutores com experiência e tempo para atividades diárias.", "Disponível",
                    dog + "husky/n02110185_4186.jpg"),
                NovoPet(sosBichos, "Salsicha", "Cão", "Dachshund", "Macho", "Pequeno", 6, true, false, true,
                    "Companheiro tranquilo, adora colo e cochilos no sofá. Perfeito para apartamento.", "Disponível",
                    dog + "dachshund/daschund-2.jpg", dog + "dachshund/dachshund-2033796_640.jpg"),
                NovoPet(sosBichos, "Pipoca", "Cão", "Poodle", "Fêmea", "Pequeno", 5, true, false, true,
                    "Resgatada das ruas, hoje é só alegria. Muito carinhosa e não late muito.", "Disponível",
                    dog + "poodle-toy/n02113624_3103.jpg"),
                NovoPet(sosBichos, "Bento", "Cão", "SRD", "Macho", "Médio", 2, true, false, true,
                    "Vira-lata de coração enorme! Educado, faz as necessidades no lugar certo.", "Disponível",
                    dog + "mix/milka6.jpg"),
                NovoPet(mariana, "Fumaça", "Cão", "Shih Tzu", "Macho", "Pequeno", 7, true, false, true,
                    "Precisei me mudar para um local que não aceita animais. Ele é dócil e acostumado com crianças.", "Disponível",
                    dog + "shihtzu/n02086240_3827.jpg", dog + "shihtzu/n02086240_818.jpg"),
                NovoPet(carlos, "Princesa", "Cão", "Yorkshire", "Fêmea", "Pequeno", 8, true, false, false,
                    "Idosa e muito calma, procura um lar tranquilo para curtir a melhor idade.", "Disponível",
                    dog + "terrier-yorkshire/n02094433_10124.jpg"),

                NovoPet(felinos, "Tigrinho", "Gato", "SRD", "Macho", "Pequeno", 1, true, false, true,
                    "Gatinho rajado super brincalhão. Já usa caixa de areia e arranhador.", "Disponível",
                    cat + "sDns8AQGz.jpg"),
                NovoPet(felinos, "Nina", "Gato", "SRD", "Fêmea", "Pequeno", 2, true, false, true,
                    "Tímida no começo, depois não sai do seu lado. Ideal para apartamento telado.", "Disponível",
                    cat + "c0q.jpg"),
                NovoPet(felinos, "Bolinha", "Gato", "SRD", "Fêmea", "Pequeno", 0, false, false, true,
                    "Filhote de 3 meses, dorminhoca e muito fofa. Castração garantida pela ONG aos 6 meses.", "Disponível",
                    cat + "eaa.jpg"),
                NovoPet(juliana, "Simba", "Gato", "SRD", "Macho", "Médio", 4, true, false, true,
                    "Gato laranja muito tranquilo. Estou doando por alergia na família.", "Disponível",
                    cat + "ayEJiATFy.jpg"),
                NovoPet(felinos, "Neve", "Gato", "Persa", "Fêmea", "Pequeno", 3, true, false, false,
                    "Pelagem longa que precisa de escovação semanal. Muito calma e silenciosa.", "Disponível",
                    cat + "MjAzMDEwNw.jpg"),
                NovoPet(juliana, "Piu e Pia", "Pássaro", "Calopsita", "Macho", "Pequeno", 2, false, false, true,
                    "Casal de calopsitas mansas, vão junto com a gaiola. Assobiam o dia todo!", "Disponível",
                    "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Nymphicus_hollandicus_-_Forst_01.jpg/960px-Nymphicus_hollandicus_-_Forst_01.jpg"),
                NovoPet(carlos, "Paçoca", "Hamster", "Sírio", "Fêmea", "Pequeno", 1, false, false, true,
                    "Hamster dócil, acompanha gaiola com roda e bebedouro.", "Disponível",
                    "https://upload.wikimedia.org/wikipedia/commons/c/cd/Peach_the_pet_hamster.jpg"),

                // Em processo de adoção (alguém já assinou o termo)
                NovoPet(patinhas, "Lola", "Cão", "Beagle", "Fêmea", "Médio", 3, true, true, true,
                    "Simpática e comilona! Já tem uma família interessada.", "Em Processo",
                    dog + "beagle/n02088364_15877.jpg"),

                // Galeria Final Feliz
                NovoPet(patinhas, "Bob", "Cão", "Golden Retriever", "Macho", "Grande", 5, true, true, true,
                    "Adotado em agosto por uma família de Franca. Hoje vive em uma chácara!", "Adotado",
                    dog + "retriever-golden/sasha2_delgado.jpg"),
                NovoPet(sosBichos, "Biscoito", "Cão", "SRD", "Macho", "Pequeno", 2, true, false, true,
                    "Ganhou um lar cheio de crianças para brincar.", "Adotado",
                    dog + "mix/denver.jpg"),
                NovoPet(felinos, "Mingau", "Gato", "SRD", "Macho", "Pequeno", 1, true, false, true,
                    "Adotado junto com o irmão. Os dois vivem grudados!", "Adotado",
                    cat + "egn.jpg"),
                NovoPet(felinos, "Chanel", "Gato", "SRD", "Fêmea", "Pequeno", 2, true, false, true,
                    "Hoje é a rainha da casa.", "Adotado",
                    cat + "MTcxMDM5OA.jpg"),
                NovoPet(mariana, "Pandora", "Cão", "Poodle", "Fêmea", "Pequeno", 6, true, false, true,
                    "Encontrou uma tutora aposentada que a leva para passear todo dia.", "Adotado",
                    dog + "poodle-toy/n02113624_253.jpg")
            );

            ItemDoacao NovoItem(Usuario dono, string nome, string categoria, string destino, string descricao, string emoji, string cor1, string cor2)
            {
                var foto = Ilustracao(emoji, cor1, cor2);
                return new ItemDoacao
                {
                    Nome = nome, Categoria = categoria, EspecieDestino = destino, Descricao = descricao,
                    Status = "Disponível", UsuarioDoadorId = dono.Id, FotoUrl = foto,
                    Imagens = new List<ItemImagem> { new() { Base64Data = foto } }
                };
            }

            db.ItensDoacao.AddRange(
                NovoItem(mariana, "Ração Premium 15kg", "Ração/Alimento", "Cão", "Saco lacrado, sabor carne. Validade 08/2027.", "🦴", "#fde68a", "#f59e0b"),
                NovoItem(juliana, "Ração para Gatos 3kg", "Ração/Alimento", "Gato", "Dois pacotes fechados, sabor peixe. Validade 05/2027.", "🐟", "#bae6fd", "#0ea5e9"),
                NovoItem(carlos, "Casinha de Madeira G", "Casinha/Cama", "Cão", "Casinha para cães grandes, telhado impermeável. Pouco usada.", "🏠", "#fed7aa", "#ea580c"),
                NovoItem(mariana, "Caminha Acolchoada", "Casinha/Cama", "Todos", "Caminha redonda tamanho M, lavável. Ótimo estado.", "🛏️", "#e9d5ff", "#9333ea"),
                NovoItem(juliana, "Arranhador com Torre", "Brinquedo/Acessório", "Gato", "Arranhador de 1,20m com três andares e brinquedo pendurado.", "🧶", "#fbcfe8", "#db2777"),
                NovoItem(carlos, "Gaiola para Pássaros", "Gaiola/Aquário", "Pássaro", "Gaiola média com poleiros, comedouro e bebedouro.", "🐦", "#bbf7d0", "#16a34a"),
                NovoItem(patinhas, "Coleira + Guia Peitoral", "Brinquedo/Acessório", "Cão", "Kit novo tamanho M, ainda na embalagem. Doação de parceiro da ONG.", "🦮", "#c7d2fe", "#4f46e5"),
                NovoItem(sosBichos, "Caixa de Transporte", "Brinquedo/Acessório", "Todos", "Caixa de transporte nº 3, ideal para levar ao veterinário.", "🧳", "#e2e8f0", "#475569"),
                NovoItem(felinos, "Kit Vermífugo e Antipulgas", "Remédio/Higiene", "Gato", "Kit dentro da validade para gatos até 4kg.", "💊", "#fecaca", "#dc2626"),
                NovoItem(patinhas, "Comedouro Duplo Inox", "Brinquedo/Acessório", "Todos", "Comedouro e bebedouro inox com base antiderrapante.", "🥣", "#99f6e4", "#0d9488")
            );

            await db.SaveChangesAsync();
            log.LogInformation("Cadastros de demonstração criados (senha das contas: {Senha}).", SenhaDemo);
        }

        // Desenho simples (SVG) usado como foto dos itens de demonstração
        private static string Ilustracao(string emoji, string cor1, string cor2)
        {
            var svg = $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 300'><defs><linearGradient id='g' x1='0' y1='0' x2='1' y2='1'><stop offset='0' stop-color='{cor1}'/><stop offset='1' stop-color='{cor2}'/></linearGradient></defs><rect width='400' height='300' fill='url(#g)'/><circle cx='200' cy='150' r='95' fill='white' fill-opacity='0.35'/><text x='200' y='185' font-size='110' text-anchor='middle'>{emoji}</text></svg>";
            return "data:image/svg+xml;charset=utf-8," + Uri.EscapeDataString(svg);
        }

        // CPF/CNPJ fictícios, mas com dígitos verificadores corretos (passam na validação)
        internal static string GerarCpf(string base9)
        {
            var d = base9.Select(c => c - '0').ToList();
            for (var t = 9; t <= 10; t++)
            {
                var soma = 0;
                for (var i = 0; i < t; i++) soma += d[i] * (t + 1 - i);
                var r = soma % 11;
                d.Add(r < 2 ? 0 : 11 - r);
            }
            return DocumentoUtil.Formatar(string.Concat(d));
        }

        internal static string GerarCnpj(string base12)
        {
            var d = base12.Select(c => c - '0').ToList();
            int[][] pesos = { new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }, new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 } };
            foreach (var p in pesos)
            {
                var soma = 0;
                for (var i = 0; i < p.Length; i++) soma += d[i] * p[i];
                var r = soma % 11;
                d.Add(r < 2 ? 0 : 11 - r);
            }
            return DocumentoUtil.Formatar(string.Concat(d));
        }
    }
}
