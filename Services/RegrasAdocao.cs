using crudcomdb.Models;

namespace crudcomdb.Services
{
    // =====================================================================
    // REGRAS DE NEGÓCIO "PURAS" (sem banco): fáceis de testar
    // =====================================================================

    // RN04: só animais domésticos podem ser anunciados.
    // Lista baseada no Anexo I da Portaria Ibama nº 93/1998 (fauna doméstica), limitada aos animais de companhia.
    public static class EspeciesPermitidas
    {
        public record Opcao(string Valor, string Rotulo, string Emoji);

        public static readonly Opcao[] Especies =
        {
            new("Cão", "Cachorro", "🐶"),
            new("Gato", "Gato", "🐱"),
            new("Pássaro", "Ave doméstica", "🐦"),
            new("Coelho", "Coelho", "🐰"),
            new("Hamster", "Hamster", "🐹"),
            new("Porquinho-da-índia", "Porquinho-da-índia", "🐹"),
            new("Chinchila", "Chinchila", "🐭"),
        };

        // Aves domésticas permitidas (as demais, como papagaios e canários-da-terra, são silvestres)
        public static readonly string[] AvesDomesticas = { "Calopsita", "Periquito-australiano", "Canário-belga", "Agapornis", "Manon" };

        // Aves passam por moderação antes de aparecer no catálogo
        public const string StatusModeracao = "Em Moderação";

        public static bool PrecisaModeracao(string especie) => especie == "Pássaro";

        public static string Emoji(string? especie) =>
            Especies.FirstOrDefault(e => e.Valor == especie)?.Emoji ?? (especie == "Peixe" ? "🐟" : "🐾");

        // Devolve a mensagem de erro, ou null se o anúncio for permitido
        public static string? Validar(string? especie, string? raca)
        {
            if (string.IsNullOrWhiteSpace(especie) || !Especies.Any(e => e.Valor == especie))
                return "Escolha uma espécie da lista. O Match-Pet aceita apenas animais domésticos (Portaria Ibama nº 93/1998); animais silvestres não podem ser anunciados.";

            if (especie == "Pássaro" && !AvesDomesticas.Contains(raca?.Trim(), StringComparer.OrdinalIgnoreCase))
                return $"Para aves, escolha uma espécie doméstica: {string.Join(", ", AvesDomesticas)}. Aves silvestres (papagaio, arara, canário-da-terra, trinca-ferro...) não podem ser anunciadas.";

            return null;
        }
    }

    // Compatibilidade entre o pet e o perfil do adotante (questionário)
    public static class Compatibilidade
    {
        public record Item(bool Ok, string Texto);

        public static List<Item> Avaliar(Pet pet, PerfilAdotante perfil)
        {
            var itens = new List<Item>();

            if (pet.PrecisaDeQuintal)
                itens.Add(perfil.TemQuintal
                    ? new(true, "Você tem quintal, e ele precisa de espaço")
                    : new(false, $"{pet.Nome} precisa de quintal e você informou que não tem"));
            else
                itens.Add(new(true, perfil.Moradia == "Apartamento" ? "Vive bem em apartamento" : "Se adapta a qualquer moradia"));

            if (perfil.TemCriancas)
                itens.Add(pet.RecomendadoParaCriancas
                    ? new(true, "Convive bem com crianças")
                    : new(false, "Não é recomendado para casas com crianças"));

            if (pet.Especie == "Gato" && perfil.Moradia == "Apartamento")
                itens.Add(perfil.TelasDeProtecao
                    ? new(true, "Janelas com tela de proteção")
                    : new(false, "Gatos em apartamento precisam de telas nas janelas"));

            if (pet.Especie == "Cão" && perfil.HorasSozinho > 8)
                itens.Add(new(false, $"Cães sofrem ficando mais de 8 horas sozinhos (você informou {perfil.HorasSozinho} h)"));

            if (!perfil.MoradiaPropria && !perfil.ProprietarioPermite)
                itens.Add(new(false, "O proprietário do imóvel não permite animais"));

            if (!perfil.TodosConcordam)
                itens.Add(new(false, "Nem todos da casa concordam com a adoção"));

            return itens;
        }

        // 0 a 100
        public static int Pontuacao(IReadOnlyCollection<Item> itens) =>
            itens.Count == 0 ? 100 : (int)Math.Round(100.0 * itens.Count(i => i.Ok) / itens.Count);
    }

    // SUS — System Usability Scale (Brooke, 1996): questionário padronizado de 10 itens, notas de 1 a 5
    public static class Sus
    {
        public static readonly string[] Perguntas =
        {
            "Eu acho que gostaria de usar o Match-Pet com frequência.",
            "Achei o site desnecessariamente complexo.",
            "Achei o site fácil de usar.",
            "Acho que precisaria da ajuda de alguém com conhecimento técnico para usar o site.",
            "Achei que as funções do site estão bem integradas.",
            "Achei que o site tem muitas inconsistências.",
            "Imagino que a maioria das pessoas aprenderia a usar o site rapidamente.",
            "Achei o site muito complicado de usar.",
            "Me senti muito confiante usando o site.",
            "Precisei aprender muitas coisas antes de conseguir usar o site.",
        };

        // Itens ímpares (positivos): nota - 1. Itens pares (negativos): 5 - nota. Soma × 2,5.
        public static double Calcular(IReadOnlyList<int> notas)
        {
            if (notas.Count != 10 || notas.Any(n => n < 1 || n > 5))
                throw new ArgumentException("O SUS precisa de 10 respostas com notas de 1 a 5.");
            var soma = 0;
            for (var i = 0; i < 10; i++) soma += i % 2 == 0 ? notas[i] - 1 : 5 - notas[i];
            return soma * 2.5;
        }

        // Faixas de Bangor, Kortum e Miller (2009)
        public static string Classificar(double pontuacao) => pontuacao switch
        {
            >= 85 => "Excelente",
            >= 72.5 => "Bom",
            >= 52.5 => "Razoável",
            >= 38 => "Ruim",
            _ => "Inaceitável"
        };
    }
}
