using crudcomdb.Data;
using crudcomdb.Models;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Consultas reaproveitadas pelo chat, comunidade e estatísticas
    public static class Consultas
    {
        // Dados públicos dos usuários (sem documento, senha ou e-mail), buscados de uma vez
        public static async Task<Dictionary<int, Usuario>> UsuariosPublicosAsync(AppDbContext db, IEnumerable<int> ids)
        {
            var lista = ids.Distinct().ToList();
            if (lista.Count == 0) return new();
            return await db.Usuarios.AsNoTracking()
                .Where(u => lista.Contains(u.Id))
                .Select(u => new Usuario
                {
                    Id = u.Id, Nome = u.Nome, Cidade = u.Cidade, Estado = u.Estado, EhOng = u.EhOng,
                    StatusValidacaoDocumento = u.StatusValidacaoDocumento, Bio = u.Bio, DataCadastro = u.DataCadastro,
                    FotoPerfilBase64 = u.FotoPerfilBase64 != null ? Usuario.FotoPorUrl : null,
                    DataUltimaAlteracao = u.DataUltimaAlteracao
                })
                .ToDictionaryAsync(u => u.Id);
        }

        // Pets "leves" (1ª foto só pelo número) para listas
        public static IQueryable<Pet> PetsLeves(IQueryable<Pet> query) => query
            .AsNoTracking()
            .Select(p => new Pet
            {
                Id = p.Id, Nome = p.Nome, Especie = p.Especie, Raca = p.Raca, Sexo = p.Sexo, Porte = p.Porte,
                Idade = p.Idade, Status = p.Status, UsuarioDoadorId = p.UsuarioDoadorId, AdotanteId = p.AdotanteId,
                DataCadastro = p.DataCadastro, DataAdocao = p.DataAdocao,
                Castrado = p.Castrado, PrecisaDeQuintal = p.PrecisaDeQuintal, RecomendadoParaCriancas = p.RecomendadoParaCriancas,
                Imagens = p.Imagens.OrderBy(i => i.Id).Take(1).Select(i => new PetImagem { Id = i.Id, PetId = i.PetId }).ToList(),
                UsuarioDoador = new Usuario { Id = p.UsuarioDoador!.Id, Nome = p.UsuarioDoador.Nome, Cidade = p.UsuarioDoador.Cidade, EhOng = p.UsuarioDoador.EhOng }
            });

        // Moderação prévia: só administradores de verdade publicam sem aprovação.
        // A conta de demonstração da banca (QR Code) também passa pela aprovação.
        public static Task<bool> PublicaDiretoAsync(AppDbContext db, int usuarioId) =>
            db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.EhAdmin && !u.ContaDemonstracao).FirstOrDefaultAsync();

        public static bool PublicaDireto(Usuario? u) => u is { EhAdmin: true, ContaDemonstracao: false };

        // Guarda um arquivo enviado e devolve o endereço dele (/midia/{id})
        public static async Task<string> SalvarMidiaAsync(AppDbContext db, byte[] dados, string contentType, int usuarioId)
        {
            var midia = new Midia { Dados = dados, ContentType = contentType, Tamanho = dados.LongLength, UsuarioId = usuarioId };
            db.Midias.Add(midia);
            await db.SaveChangesAsync();
            return $"/midia/{midia.Id}";
        }

        // "há 5 min", "ontem", "12/03"
        public static string TempoRelativo(DateTime utc)
        {
            var d = DateTime.UtcNow - utc;
            if (d.TotalMinutes < 1) return "agora";
            if (d.TotalMinutes < 60) return $"há {(int)d.TotalMinutes} min";
            if (d.TotalHours < 24) return $"há {(int)d.TotalHours} h";
            if (d.TotalDays < 2) return "ontem";
            if (d.TotalDays < 7) return $"há {(int)d.TotalDays} dias";
            var local = HoraBrasil(utc);
            return local.Year == HoraBrasil(DateTime.UtcNow).Year ? local.ToString("dd/MM") : local.ToString("dd/MM/yyyy");
        }

        // O servidor (Render) roda em UTC: converte para o horário de Brasília
        private static readonly TimeZoneInfo FusoBrasil = ObterFuso();
        private static TimeZoneInfo ObterFuso()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
            catch { return TimeZoneInfo.CreateCustomTimeZone("BRT", TimeSpan.FromHours(-3), "Brasília", "Brasília"); }
        }
        public static DateTime HoraBrasil(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), FusoBrasil);
    }
}
