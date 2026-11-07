using crudcomdb.Data;
using crudcomdb.Interfaces;
using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;
        public UsuarioRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Usuario>> GetAllAsync() => await _context.Usuarios.ToListAsync();

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            var e = email.Trim().ToLower();
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == e);
        }

        // Compara só os números: "123.456.789-09" e "12345678909" são o mesmo CPF
        public async Task<Usuario?> GetByDocumentoAsync(string documento)
        {
            var digitos = DocumentoUtil.SomenteDigitos(documento);
            if (digitos.Length == 0) return null;
            return await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.Documento.Replace(".", "").Replace("-", "").Replace("/", "").Replace(" ", "") == digitos);
        }

        public async Task<Usuario?> GetByIdAsync(int id) => await _context.Usuarios.FindAsync(id);

        public async Task<Usuario?> GetByResetTokenHashAsync(string tokenHash) =>
            await _context.Usuarios.FirstOrDefaultAsync(u => u.ResetSenhaTokenHash == tokenHash);

        public async Task<List<Usuario>> GetOngsAsync() => await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.EhOng)
            .OrderBy(u => u.Nome)
            .Select(u => new Usuario
            {
                Id = u.Id, Nome = u.Nome, Cidade = u.Cidade, Estado = u.Estado, Telefone = u.Telefone,
                EhOng = true, ChavePix = u.ChavePix, DescricaoOng = u.DescricaoOng,
                FotoPerfilBase64 = u.FotoPerfilBase64 != null ? Usuario.FotoPorUrl : null, DataUltimaAlteracao = u.DataUltimaAlteracao,
                StatusValidacaoDocumento = u.StatusValidacaoDocumento
            })
            .ToListAsync();

        public async Task<int> ContarOngsAsync() => await _context.Usuarios.CountAsync(u => u.EhOng);

        public async Task<Usuario?> GetOutroComMesmoDocumentoHashAsync(string hash, int idAtual) =>
            await _context.Usuarios.AsNoTracking()
                .Where(u => u.DocumentoHash == hash && u.Id != idAtual)
                .Select(u => new Usuario { Id = u.Id, Nome = u.Nome })
                .FirstOrDefaultAsync();

        public async Task SincronizarModeracaoAsync(Usuario usuario)
        {
            var atual = await _context.Usuarios.AsNoTracking().Where(u => u.Id == usuario.Id)
                .Select(u => new { u.StatusValidacaoDocumento, u.DocumentoObservacao, u.DataValidacaoDocumento, u.FotoDocumentoBase64, u.EhAdmin, u.Bloqueado, u.MotivoBloqueio })
                .FirstOrDefaultAsync();
            if (atual == null) return;
            usuario.StatusValidacaoDocumento = atual.StatusValidacaoDocumento;
            usuario.DocumentoObservacao = atual.DocumentoObservacao;
            usuario.DataValidacaoDocumento = atual.DataValidacaoDocumento;
            usuario.FotoDocumentoBase64 = atual.FotoDocumentoBase64;
            usuario.EhAdmin = atual.EhAdmin;
            usuario.Bloqueado = atual.Bloqueado;
            usuario.MotivoBloqueio = atual.MotivoBloqueio;
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
