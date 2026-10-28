using System.Text.Json;

namespace crudcomdb.Services
{
    // Consulta pública de CNPJ na Receita Federal via BrasilAPI (gratuita, sem chave).
    // Usada no cadastro de ONGs: confirma que o CNPJ existe, está ATIVO e traz a razão social.
    public class CnpjService
    {
        public record DadosCnpj(string RazaoSocial, string NomeFantasia, string Situacao, string NaturezaJuridica, string Cidade, string Uf)
        {
            public bool Ativo => Situacao.Equals("ATIVA", StringComparison.OrdinalIgnoreCase);

            // Associações, fundações e organizações religiosas/sociais são entidades sem fins lucrativos
            public bool SemFinsLucrativos =>
                NaturezaJuridica.Contains("Associação", StringComparison.OrdinalIgnoreCase) ||
                NaturezaJuridica.Contains("Fundação", StringComparison.OrdinalIgnoreCase) ||
                NaturezaJuridica.Contains("Organização", StringComparison.OrdinalIgnoreCase) ||
                NaturezaJuridica.Contains("Serviço Social", StringComparison.OrdinalIgnoreCase);
        }

        private readonly IHttpClientFactory _http;
        private readonly ILogger<CnpjService> _log;

        public CnpjService(IHttpClientFactory http, ILogger<CnpjService> log)
        {
            _http = http;
            _log = log;
        }

        // null = não encontrado; lança exceção só se o serviço estiver fora do ar
        public async Task<DadosCnpj?> ConsultarAsync(string cnpj)
        {
            var digitos = DocumentoUtil.SomenteDigitos(cnpj);
            var cliente = _http.CreateClient();
            cliente.Timeout = TimeSpan.FromSeconds(8);

            var resp = await cliente.GetAsync($"https://brasilapi.com.br/api/cnpj/v1/{digitos}");
            if (resp.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.BadRequest) return null;
            resp.EnsureSuccessStatusCode();

            using var json = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var r = json.RootElement;
            string Campo(string nome) => r.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

            return new DadosCnpj(Campo("razao_social"), Campo("nome_fantasia"), Campo("descricao_situacao_cadastral"),
                                 Campo("natureza_juridica"), Campo("municipio"), Campo("uf"));
        }
    }
}
