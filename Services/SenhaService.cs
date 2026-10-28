using crudcomdb.Models;
using Microsoft.AspNetCore.Identity;

namespace crudcomdb.Services
{
    // Senhas guardadas com hash (PBKDF2 do ASP.NET Identity), nunca em texto puro.
    // Contas antigas (senha em texto) continuam entrando e são convertidas no 1º login.
    public static class SenhaService
    {
        private static readonly PasswordHasher<Usuario> Hasher = new();

        public static string GerarHash(string senha) => Hasher.HashPassword(null!, senha);

        public static bool EhHash(string? valor) => !string.IsNullOrEmpty(valor) && valor.StartsWith("AQAAAA");

        public static bool Verificar(Usuario usuario, string senhaDigitada)
        {
            if (string.IsNullOrEmpty(usuario.Senha)) return false;
            if (!EhHash(usuario.Senha)) return usuario.Senha == senhaDigitada; // conta antiga
            return Hasher.VerifyHashedPassword(usuario, usuario.Senha, senhaDigitada) != PasswordVerificationResult.Failed;
        }
    }
}
