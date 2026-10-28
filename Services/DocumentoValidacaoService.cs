using System.Security.Cryptography;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Beta.Messages;
using crudcomdb.Interfaces;
using crudcomdb.Models;

namespace crudcomdb.Services
{
    // Valida a foto do documento enviada no Perfil.
    //
    // Verificações (em ordem):
    //  1. Duplicidade: a MESMA foto já foi usada em outra conta? (hash SHA-256)
    //  2. Leitura por IA (Claude): é mesmo um documento? Está legível? Há sinais de montagem/edição?
    //     Extrai nome, CPF/CNPJ e confere com o cadastro.
    //  3. Duplicidade: o CPF/CNPJ que aparece no documento pertence a OUTRA conta?
    //
    // Sem a chave da IA configurada (Anthropic:ApiKey ou ANTHROPIC_API_KEY), só as verificações
    // locais rodam e o documento fica "Em Análise" (ou "Aprovado" se Documentos:AprovarSemIA = true).
    public class DocumentoValidacaoService
    {
        public record Resultado(string Status, string Observacao, string Hash);

        private const string Modelo = "claude-opus-5-5";

        private readonly IUsuarioRepository _usuarios;
        private readonly IConfiguration _config;
        private readonly ILogger<DocumentoValidacaoService> _log;

        public DocumentoValidacaoService(IUsuarioRepository usuarios, IConfiguration config, ILogger<DocumentoValidacaoService> log)
        {
            _usuarios = usuarios;
            _config = config;
            _log = log;
        }

        private string? ChaveIA => _config["Anthropic:ApiKey"] is { Length: > 0 } c ? c : Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        public bool IaConfigurada => !string.IsNullOrWhiteSpace(ChaveIA);

        public static string CalcularHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

        public async Task<Resultado> ValidarAsync(Usuario usuario, byte[] imagem, string contentType)
        {
            var hash = CalcularHash(imagem);

            // 1. Mesma foto em outra conta
            var dono = await _usuarios.GetOutroComMesmoDocumentoHashAsync(hash, usuario.Id);
            if (dono != null)
                return new("Reprovado", "Esta mesma foto de documento já foi enviada por outra conta. Por segurança, ela não pode ser reutilizada.", hash);

            // 2. Leitura por IA
            if (!IaConfigurada)
            {
                var aprovarSemIa = _config.GetValue("Documentos:AprovarSemIA", false);
                return aprovarSemIa
                    ? new("Aprovado", "Verificação básica concluída (formato e duplicidade). A leitura automática do documento está desativada neste servidor.", hash)
                    : new("Em Análise", "Recebemos seu documento. Ele será conferido pela equipe (leitura automática desativada neste servidor).", hash);
            }

            LeituraDocumento? leitura;
            try
            {
                leitura = await LerComIaAsync(imagem, contentType);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Falha na leitura do documento por IA");
                return new("Em Análise", "Não conseguimos analisar o documento automaticamente agora. Ele ficará em análise pela equipe.", hash);
            }

            if (leitura == null)
                return new("Em Análise", "A análise automática não foi concluída. O documento ficará em análise pela equipe.", hash);

            if (!leitura.eh_documento)
                return new("Reprovado", "A imagem enviada não parece ser um documento de identificação. Envie uma foto do RG, CNH ou cartão CNPJ.", hash);

            if (!leitura.legivel)
                return new("Reprovado", "A foto está ilegível (borrada, escura ou cortada). Tire outra foto com boa iluminação, mostrando o documento inteiro.", hash);

            if (leitura.sinais_de_adulteracao)
                return new("Em Análise", $"Encontramos indícios que precisam de conferência manual: {leitura.observacoes}", hash);

            // 3. Confere os dados com o cadastro
            var docCadastro = DocumentoUtil.SomenteDigitos(usuario.Documento);
            var docLido = DocumentoUtil.SomenteDigitos(DocumentoUtil.EhCnpj(usuario.Documento) ? leitura.cnpj : leitura.cpf);

            if (!string.IsNullOrEmpty(docLido))
            {
                if (docLido != docCadastro)
                {
                    var outro = await _usuarios.GetByDocumentoAsync(docLido);
                    return outro != null && outro.Id != usuario.Id
                        ? new("Reprovado", "O documento enviado pertence a outra conta já cadastrada no Match-Pet.", hash)
                        : new("Reprovado", $"O número do documento na foto ({DocumentoUtil.Formatar(docLido)}) não confere com o CPF/CNPJ do seu cadastro.", hash);
                }
            }

            var similaridade = DocumentoUtil.SimilaridadeNomes(leitura.nome, usuario.Nome);
            if (!string.IsNullOrWhiteSpace(leitura.nome) && similaridade < 0.6)
                return new("Reprovado", $"O nome no documento (\"{leitura.nome}\") não confere com o nome do cadastro.", hash);

            if (string.IsNullOrEmpty(docLido) && string.IsNullOrWhiteSpace(leitura.nome))
                return new("Em Análise", "O documento foi reconhecido, mas não foi possível ler o nome nem o número. Ele ficará em análise pela equipe.", hash);

            var conferido = string.IsNullOrEmpty(docLido) ? "nome" : "nome e número";
            return new("Aprovado", $"{leitura.tipo_documento} verificado: {conferido} conferem com o cadastro.", hash);
        }

        // Formato da resposta da IA (os nomes seguem o JSON pedido)
        private record LeituraDocumento(
            bool eh_documento, string tipo_documento, bool legivel, string nome, string cpf, string cnpj,
            bool sinais_de_adulteracao, string observacoes);

        private async Task<LeituraDocumento?> LerComIaAsync(byte[] imagem, string contentType)
        {
            AnthropicClient client = new() { ApiKey = ChaveIA };

            var tipoImagem = contentType.Contains("png") ? "image/png" : contentType.Contains("webp") ? "image/webp" : "image/jpeg";

            var schema = new Dictionary<string, JsonElement>
            {
                ["type"] = JsonSerializer.SerializeToElement("object"),
                ["additionalProperties"] = JsonSerializer.SerializeToElement(false),
                ["properties"] = JsonSerializer.SerializeToElement(new
                {
                    eh_documento = new { type = "boolean" },
                    tipo_documento = new { type = "string" },
                    legivel = new { type = "boolean" },
                    nome = new { type = "string" },
                    cpf = new { type = "string" },
                    cnpj = new { type = "string" },
                    sinais_de_adulteracao = new { type = "boolean" },
                    observacoes = new { type = "string" }
                }),
                ["required"] = JsonSerializer.SerializeToElement(new[]
                    { "eh_documento", "tipo_documento", "legivel", "nome", "cpf", "cnpj", "sinais_de_adulteracao", "observacoes" }),
            };

            var resposta = await client.Beta.Messages.Create(new MessageCreateParams
            {
                Model = Modelo,
                MaxTokens = 4000,
                // Se a IA recusar o pedido por política, outro modelo responde no lugar
                Betas = ["server-side-fallback-2026-06-01"],
                Fallbacks = new List<BetaFallbackParam> { new() { Model = "claude-opus-4-8" } },
                OutputConfig = new BetaOutputConfig
                {
                    Effort = "low",
                    Format = new BetaJsonOutputFormat { Schema = schema },
                },
                System = "Você confere fotos de documentos brasileiros para um site de adoção de animais que exige identificação dos usuários. " +
                         "Analise a imagem e responda em português. " +
                         "eh_documento: é um documento oficial de identificação (RG, CNH, CPF, passaporte, carteira de trabalho, cartão CNPJ ou estatuto de ONG)? " +
                         "tipo_documento: o tipo identificado (ex.: \"RG\", \"CNH\", \"Cartão CNPJ\"), ou \"Nenhum\". " +
                         "legivel: dá para ler nome e número com segurança? " +
                         "nome, cpf, cnpj: copie exatamente como aparecem; use \"\" se não houver. " +
                         "sinais_de_adulteracao: true só se houver indícios claros de montagem, edição digital, texto sobreposto, modelo de exemplo/espécime ou documento claramente falso. " +
                         "observacoes: uma frase curta explicando o que você viu.",
                Messages =
                [
                    new BetaUserMessageParam
                    {
                        Content = new List<BetaContentBlockParam>
                        {
                            new BetaImageBlockParam
                            {
                                Source = new BetaBase64ImageSource { Data = Convert.ToBase64String(imagem), MediaType = tipoImagem }
                            },
                            new BetaTextBlockParam { Text = "Analise este documento." }
                        }
                    }
                ]
            });

            if (resposta.StopReason == "refusal") return null;

            var texto = string.Concat(resposta.Content.Select(b => b.Value).OfType<BetaTextBlock>().Select(t => t.Text));
            return string.IsNullOrWhiteSpace(texto) ? null : JsonSerializer.Deserialize<LeituraDocumento>(texto);
        }
    }
}
