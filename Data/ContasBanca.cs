using System.Security.Cryptography;
using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Data
{
    // Contas prontas para a apresentação à banca: administrador, adotante e ONG.
    //
    // As senhas são ALEATÓRIAS e nunca ficam no código (o repositório é público).
    // As contas só são criadas quando o site roda no computador do desenvolvedor (ambiente Development):
    // as senhas são gravadas no arquivo local CONTAS_BANCA.local.txt, que o .gitignore deixa fora do GitHub.
    //
    // Em produção, este código apenas promove a administrador os e-mails da configuração Admin:Emails.
    public static class ContasBanca
    {
        public const string EmailAdmin = "admin@matchpet.example";
        public const string EmailAdotante = "adotante@matchpet.example";
        public const string EmailOng = "ong@matchpet.example";
        public const string Arquivo = "CONTAS_BANCA.local.txt";

        public static async Task PrepararAsync(AppDbContext db, IConfiguration config, IWebHostEnvironment ambiente, ILogger log)
        {
            // Admin:Emails = "pessoa1@x.com;pessoa2@y.com" (variável de ambiente Admin__Emails no Render)
            var emails = (config["Admin:Emails"] ?? "").Split(';', ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => e.ToLower()).ToList();
            if (emails.Count > 0)
                await db.Usuarios.Where(u => emails.Contains(u.Email.ToLower()) && !u.EhAdmin)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.EhAdmin, true));

            if (!ambiente.IsDevelopment()) return;
            if (await db.Usuarios.AnyAsync(u => u.Email == EmailAdmin)) return;

            log.LogInformation("Criando as contas da banca...");
            var agora = DateTime.UtcNow;
            var senhas = new Dictionary<string, string>
            {
                [EmailAdmin] = GerarSenha(), [EmailAdotante] = GerarSenha(), [EmailOng] = GerarSenha()
            };

            Usuario Conta(string nome, string email, string documento, bool ong, bool admin) => new()
            {
                Nome = nome, Email = email, Documento = documento, Telefone = "(16) 3700-0000",
                Senha = SenhaService.GerarHash(senhas[email]),
                CEP = "14400-000", Logradouro = "Av. Champagnat", Numero = "1808", Bairro = "Centro", Cidade = "Franca", Estado = "SP",
                EhOng = ong, EhAdmin = admin,
                ChavePix = ong ? "pix.amigosdefranca@matchpet.example" : null,
                DescricaoOng = ong ? "ONG de demonstração para a apresentação do Match-Pet. Resgata, castra e encaminha cães e gatos para adoção responsável em Franca/SP." : null,
                Bio = admin ? "Conta da equipe de moderação do Match-Pet." : ong ? null : "Apaixonada por animais, procurando um companheiro para a família. 🐾",
                StatusValidacaoDocumento = "Aprovado", DocumentoObservacao = "Conta de demonstração para a banca.", DataValidacaoDocumento = agora,
                Classificacao = ong ? "ONG Verificada" : "Usuário Verificado",
                EmailConfirmado = true, CodigoConfirmacao = "ISENTO",
                AceitouTermos = true, DataAceiteTermos = agora, IpAceite = "seed", DataCadastro = agora.AddDays(-40)
            };

            var admin = Conta("Moderação Match-Pet", EmailAdmin, DadosDemonstracao.GerarCpf("731842650"), false, true);
            var adotante = Conta("Beatriz Almeida", EmailAdotante, DadosDemonstracao.GerarCpf("582913746"), false, false);
            var ong = Conta("ONG Amigos de Franca", EmailOng, DadosDemonstracao.GerarCnpj("481726390001"), true, false);
            db.Usuarios.AddRange(admin, adotante, ong);
            await db.SaveChangesAsync();

            // Adotante com o questionário de adoção já respondido
            db.PerfisAdotante.Add(new PerfilAdotante
            {
                UsuarioId = adotante.Id, Moradia = "Casa", MoradiaPropria = true, TemQuintal = true, TemCriancas = true,
                TemOutrosAnimais = true, OutrosAnimais = "Uma gata castrada de 4 anos", HorasSozinho = 4, JaTevePets = true,
                TodosConcordam = true, CienteDosCustos = true, AceitaAcompanhamento = true,
                Motivacao = "Nossa família quer dar um lar para um cão que goste de crianças. Temos quintal murado e tempo para passeios."
            });

            // ONG com pets no catálogo e uma ave aguardando a moderação
            const string dog = "https://images.dog.ceo/breeds/";
            const string cat = "https://cdn2.thecatapi.com/images/";
            Pet Pet(string nome, string especie, string raca, string sexo, string porte, int idade, bool castrado, bool quintal, bool criancas,
                string descricao, string status, string foto) => new()
            {
                Nome = nome, Especie = especie, Raca = raca, Sexo = sexo, Porte = porte, Idade = idade, Castrado = castrado,
                PrecisaDeQuintal = quintal, RecomendadoParaCriancas = criancas, Descricao = descricao, Status = status,
                UsuarioDoadorId = ong.Id, DataCadastro = agora.AddDays(-3), Imagens = new() { new PetImagem { Base64Data = foto } }
            };
            var pets = new[]
            {
                Pet("Bolinha", "Cão", "SRD", "Macho", "Médio", 2, true, false, true,
                    "Resgatado em uma estrada de Franca. Vacinado, vermifugado e castrado. Adora crianças e brinquedos de corda.", "Disponível", dog + "mix/penny.jpg"),
                Pet("Amora", "Gato", "SRD", "Fêmea", "Pequeno", 1, true, false, true,
                    "Gatinha carinhosa, usa a caixa de areia direitinho. Precisa de telas nas janelas.", "Disponível", cat + "MTgzNTk1Nw.jpg"),
                Pet("Thor", "Cão", "Labrador", "Macho", "Grande", 5, true, true, true,
                    "Labrador calmo e obediente. Precisa de quintal e passeios diários.", "Disponível", dog + "labrador/n02099712_6823.jpg"),
                Pet("Kiwi", "Pássaro", "Calopsita", "Macho", "Pequeno", 1, false, false, true,
                    "Calopsita mansa que assobia. Vai com gaiola. (Anúncio de ave: aguarda a conferência da moderação.)",
                    EspeciesPermitidas.StatusModeracao, "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2f/Nymphicus_hollandicus_-_Forst_01.jpg/960px-Nymphicus_hollandicus_-_Forst_01.jpg"),
            };
            db.Pets.AddRange(pets);
            await db.SaveChangesAsync();

            db.Favoritos.AddRange(pets.Take(2).Select(p => new Favorito { UsuarioId = adotante.Id, PetId = p.Id }));

            // Exemplos de denúncia para mostrar o fluxo de moderação
            var postagem = await db.Postagens.OrderBy(p => p.Id).Select(p => (int?)p.Id).FirstOrDefaultAsync();
            if (postagem != null)
                db.Denuncias.Add(new Denuncia { DenuncianteId = adotante.Id, Tipo = "post", AlvoId = postagem.Value, Motivo = "Spam",
                    Detalhes = "Exemplo de denúncia criado para a demonstração do painel de moderação." });
            db.Denuncias.Add(new Denuncia { DenuncianteId = adotante.Id, Tipo = "pet", AlvoId = pets[2].Id, Motivo = "Informações falsas",
                Detalhes = "Exemplo de denúncia criado para a demonstração do painel de moderação." });
            await db.SaveChangesAsync();

            // As senhas vão para um arquivo local (fora do GitHub)
            var caminho = Path.Combine(ambiente.ContentRootPath, Arquivo);
            await File.WriteAllTextAsync(caminho,
                "CONTAS DA BANCA - Match-Pet SP (arquivo local, NÃO enviar ao GitHub)\n" +
                "Site: https://matchpet.onrender.com/login\n\n" +
                $"Administrador:  {EmailAdmin}  /  {senhas[EmailAdmin]}\n" +
                $"Adotante:       {EmailAdotante}  /  {senhas[EmailAdotante]}\n" +
                $"ONG:            {EmailOng}  /  {senhas[EmailOng]}\n\n" +
                "O administrador acessa o painel em /admin. Para trocar uma senha, use \"Esqueci minha senha\" ou o perfil.\n");
            log.LogInformation("Contas da banca criadas. Senhas gravadas em {Arquivo}", Arquivo);
        }

        // 14 caracteres com letras, números e símbolo (sem caracteres parecidos como 0/O e l/1)
        private static string GerarSenha()
        {
            const string letras = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ";
            const string numeros = "23456789";
            var s = new char[14];
            for (var i = 0; i < 10; i++) s[i] = letras[RandomNumberGenerator.GetInt32(letras.Length)];
            for (var i = 10; i < 13; i++) s[i] = numeros[RandomNumberGenerator.GetInt32(numeros.Length)];
            s[13] = '!';
            return new string(s);
        }
    }
}
