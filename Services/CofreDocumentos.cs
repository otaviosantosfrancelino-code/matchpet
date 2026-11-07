using Microsoft.AspNetCore.DataProtection;

namespace crudcomdb.Services
{
    // Guarda a foto do documento CRIPTOGRAFADA no banco (LGPD, art. 46: segurança dos dados).
    //
    // - Usa o Data Protection do ASP.NET Core (AES-256-CBC + HMAC-SHA256), com chaves guardadas no banco.
    // - A foto só fica guardada enquanto espera a conferência manual ("Em Análise").
    //   Aprovado ou reprovado, a imagem é apagada e fica apenas o hash SHA-256 (antifraude).
    public class CofreDocumentos
    {
        private const string Prefixo = "enc:v1:";
        private readonly IDataProtector _protetor;

        public CofreDocumentos(IDataProtectionProvider provedor) =>
            _protetor = provedor.CreateProtector("MatchPet.FotoDocumento.v1");

        public static bool EstaCriptografado(string? valor) => valor?.StartsWith(Prefixo) == true;

        public string Fechar(byte[] imagem, string contentType) =>
            Prefixo + _protetor.Protect($"data:{contentType};base64,{Convert.ToBase64String(imagem)}");

        // Converte uma foto antiga (texto puro) para o formato criptografado
        public string Fechar(string dataUri) => EstaCriptografado(dataUri) ? dataUri : Prefixo + _protetor.Protect(dataUri);

        // Devolve "data:image/...;base64,..." para mostrar ao administrador, ou null se não der para abrir
        public string? Abrir(string? valor)
        {
            if (string.IsNullOrEmpty(valor)) return null;
            if (!EstaCriptografado(valor)) return valor.StartsWith("data:") ? valor : null; // registro antigo
            try { return _protetor.Unprotect(valor[Prefixo.Length..]); }
            catch { return null; } // chave perdida ou dado alterado
        }
    }
}
