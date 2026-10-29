using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace crudcomdb.Services
{
    // Coordenadas aproximadas para o mapa da página do pet.
    // Por privacidade o mapa mostra uma ÁREA (bairro/cidade), nunca o endereço exato do doador.
    public class GeoService
    {
        public record Ponto(double Lat, double Lng, int RaioMetros, string Descricao);

        // Cidades da região já conhecidas (não precisam de consulta na internet)
        private static readonly Dictionary<string, (double, double)> Cidades = new()
        {
            ["franca"] = (-20.5386, -47.4008),
            ["ribeirao preto"] = (-21.1775, -47.8103),
            ["batatais"] = (-20.8914, -47.5853),
            ["patrocinio paulista"] = (-20.6394, -47.2817),
            ["cristais paulista"] = (-20.4036, -47.4206),
            ["restinga"] = (-20.6056, -47.4833),
            ["sao jose da bela vista"] = (-20.5936, -47.6406),
            ["ituverava"] = (-20.3394, -47.7806),
            ["pedregulho"] = (-20.2569, -47.4767),
            ["rifaina"] = (-20.0806, -47.4214),
            ["sertaozinho"] = (-21.1378, -47.9900),
            ["araraquara"] = (-21.7845, -48.1780),
            ["sao carlos"] = (-22.0174, -47.8909),
            ["campinas"] = (-22.9099, -47.0626),
            ["sao paulo"] = (-23.5505, -46.6333),
            ["orlandia"] = (-20.7206, -47.8867),
            ["altinopolis"] = (-21.0256, -47.3728),
            ["igarapava"] = (-20.0383, -47.7469),
            ["ribeirao corrente"] = (-20.4581, -47.5903),
            ["jeriquara"] = (-20.3117, -47.5917),
        };

        private readonly ConcurrentDictionary<string, Ponto?> _cache = new();
        private readonly IHttpClientFactory _http;
        private readonly ILogger<GeoService> _log;

        public GeoService(IHttpClientFactory http, ILogger<GeoService> log)
        {
            _http = http;
            _log = log;
        }

        public async Task<Ponto?> LocalizarAsync(string? cidade, string? estado, string? bairro)
        {
            if (string.IsNullOrWhiteSpace(cidade)) return null;
            var chaveCidade = DocumentoUtil.NormalizarNome(cidade);
            var temBairro = !string.IsNullOrWhiteSpace(bairro) && !bairro.Equals("Centro", StringComparison.OrdinalIgnoreCase);
            var chave = temBairro ? $"{DocumentoUtil.NormalizarNome(bairro)}|{chaveCidade}" : chaveCidade;

            if (_cache.TryGetValue(chave, out var guardado)) return guardado;

            Ponto? ponto = null;
            if (temBairro) ponto = await ConsultarAsync($"{bairro}, {cidade}, {estado ?? "SP"}, Brasil", 900, $"{bairro}, {cidade}");
            if (ponto == null && Cidades.TryGetValue(chaveCidade, out var c)) ponto = new Ponto(c.Item1, c.Item2, 2500, cidade);
            ponto ??= await ConsultarAsync($"{cidade}, {estado ?? "SP"}, Brasil", 3000, cidade);

            _cache[chave] = ponto;
            return ponto;
        }

        // Nominatim (OpenStreetMap): gratuito, pede identificação e uso moderado (por isso o cache)
        private async Task<Ponto?> ConsultarAsync(string busca, int raio, string descricao)
        {
            try
            {
                var cliente = _http.CreateClient();
                cliente.Timeout = TimeSpan.FromSeconds(6);
                cliente.DefaultRequestHeaders.UserAgent.ParseAdd("MatchPetSP/1.0 (projeto academico)");
                var url = $"https://nominatim.openstreetmap.org/search?format=json&limit=1&countrycodes=br&q={Uri.EscapeDataString(busca)}";
                using var json = JsonDocument.Parse(await cliente.GetStringAsync(url));
                var primeiro = json.RootElement.EnumerateArray().FirstOrDefault();
                if (primeiro.ValueKind != JsonValueKind.Object) return null;
                return new Ponto(
                    double.Parse(primeiro.GetProperty("lat").GetString()!, CultureInfo.InvariantCulture),
                    double.Parse(primeiro.GetProperty("lon").GetString()!, CultureInfo.InvariantCulture),
                    raio, descricao);
            }
            catch (Exception ex)
            {
                _log.LogWarning("Não foi possível localizar '{Busca}': {Erro}", busca, ex.Message);
                return null;
            }
        }
    }
}
