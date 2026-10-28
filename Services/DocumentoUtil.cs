using System.Globalization;
using System.Text;

namespace crudcomdb.Services
{
    // Funções de apoio para CPF, CNPJ e comparação de nomes
    public static class DocumentoUtil
    {
        // Deixa só letras e números em maiúsculo ("123.456.789-09" -> "12345678909").
        // Letras são mantidas por causa do novo CNPJ alfanumérico (a partir de jul/2026).
        public static string SomenteDigitos(string? valor) =>
            new string((valor ?? "").Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

        public static bool EhCpf(string? doc) => SomenteDigitos(doc).Length == 11;
        public static bool EhCnpj(string? doc) => SomenteDigitos(doc).Length == 14;

        public static bool CpfValido(string? cpf)
        {
            var d = SomenteDigitos(cpf);
            if (d.Length != 11 || !d.All(char.IsDigit) || d.Distinct().Count() == 1) return false;

            int Digito(int tamanho)
            {
                var soma = 0;
                for (var i = 0; i < tamanho; i++) soma += (d[i] - '0') * (tamanho + 1 - i);
                var resto = soma % 11;
                return resto < 2 ? 0 : 11 - resto;
            }
            return Digito(9) == d[9] - '0' && Digito(10) == d[10] - '0';
        }

        public static bool CnpjValido(string? cnpj)
        {
            var d = SomenteDigitos(cnpj);
            if (d.Length != 14 || !char.IsDigit(d[12]) || !char.IsDigit(d[13]) || d.Distinct().Count() == 1) return false;

            int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int Digito(int[] pesos)
            {
                var soma = 0;
                for (var i = 0; i < pesos.Length; i++) soma += (d[i] - '0') * pesos[i]; // letras valem (código ASCII - 48)
                var resto = soma % 11;
                return resto < 2 ? 0 : 11 - resto;
            }
            return Digito(pesos1) == d[12] - '0' && Digito(pesos2) == d[13] - '0';
        }

        public static bool DocumentoValido(string? doc) => EhCpf(doc) ? CpfValido(doc) : EhCnpj(doc) && CnpjValido(doc);

        public static string Formatar(string? doc)
        {
            var d = SomenteDigitos(doc);
            return d.Length switch
            {
                11 => $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}",
                14 => $"{d[..2]}.{d[2..5]}.{d[5..8]}/{d[8..12]}-{d[12..]}",
                _ => doc ?? ""
            };
        }

        public static string NormalizarNome(string? nome)
        {
            var semAcento = new StringBuilder();
            foreach (var c in (nome ?? "").Normalize(NormalizationForm.FormD))
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) semAcento.Append(c);
            return string.Join(' ', semAcento.ToString().ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        private static readonly HashSet<string> Conectivos = new() { "de", "da", "do", "das", "dos", "e" };

        // De 0 a 1: quanto do nome mais curto aparece no outro (ignora acentos e "de/da/dos")
        public static double SimilaridadeNomes(string? a, string? b)
        {
            var ta = NormalizarNome(a).Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => !Conectivos.Contains(t)).ToHashSet();
            var tb = NormalizarNome(b).Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => !Conectivos.Contains(t)).ToHashSet();
            if (ta.Count == 0 || tb.Count == 0) return 0;
            var menor = ta.Count <= tb.Count ? ta : tb;
            var maior = menor == ta ? tb : ta;
            return (double)menor.Count(maior.Contains) / menor.Count;
        }
    }
}
