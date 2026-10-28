using crudcomdb.Interfaces;
using crudcomdb.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace crudcomdb.Services
{
    public class AuthService
    {
        private const string ChaveSessao = "matchpet_sessao";
        private static readonly TimeSpan DuracaoSessao = TimeSpan.FromDays(7);

        private readonly ProtectedLocalStorage _storage;
        private readonly IUsuarioRepository _usuarios;
        private bool _sessaoVerificada;

        public AuthService(ProtectedLocalStorage storage, IUsuarioRepository usuarios)
        {
            _storage = storage;
            _usuarios = usuarios;
        }

        public Usuario? UsuarioLogado { get; private set; }
        public bool EstaLogado => UsuarioLogado != null;
        public bool DocumentoAprovado => UsuarioLogado?.StatusValidacaoDocumento == "Aprovado";
        public event Action? OnChage;

        public record Sessao(int UsuarioId, DateTime ExpiraEm);

        // O login fica salvo (criptografado) no navegador: apertar F5 não desloga mais
        public async Task LoginAsync(Usuario usuario)
        {
            AtualizarClassificacao(usuario);
            UsuarioLogado = usuario;
            _sessaoVerificada = true;
            try { await _storage.SetAsync(ChaveSessao, new Sessao(usuario.Id, DateTime.UtcNow.Add(DuracaoSessao))); }
            catch { /* navegador sem armazenamento: o login vale só nesta aba */ }
            NotifyStateChanged();
        }

        public async Task LogoutAsync()
        {
            UsuarioLogado = null;
            try { await _storage.DeleteAsync(ChaveSessao); } catch { }
            NotifyStateChanged();
        }

        // Chamado uma vez pelo layout ao abrir o site: recupera o login salvo
        public async Task RestaurarSessaoAsync()
        {
            if (_sessaoVerificada) return;
            _sessaoVerificada = true;
            Sessao? sessao;
            try
            {
                var resultado = await _storage.GetAsync<Sessao>(ChaveSessao);
                if (!resultado.Success || resultado.Value == null) return;
                sessao = resultado.Value;
            }
            catch
            {
                // Sessão ilegível (ex.: chave de criptografia do servidor mudou): descarta
                try { await _storage.DeleteAsync(ChaveSessao); } catch { }
                return;
            }

            if (sessao.ExpiraEm < DateTime.UtcNow) { try { await _storage.DeleteAsync(ChaveSessao); } catch { } return; }

            try
            {
                var usuario = await _usuarios.GetByIdAsync(sessao.UsuarioId);
                if (usuario == null) { await _storage.DeleteAsync(ChaveSessao); return; }

                AtualizarClassificacao(usuario);
                UsuarioLogado = usuario;
                NotifyStateChanged();
            }
            catch
            {
                // Falha no banco: segue deslogado nesta visita, mas mantém o login salvo para a próxima
                _sessaoVerificada = false;
            }
        }

        public void AtualizarClassificacao(Usuario user)
        {
            if (user == null) return;
            var tempoDeConta = DateTime.UtcNow - user.DataCadastro;
            var verificado = user.StatusValidacaoDocumento == "Aprovado";

            if (user.EhOng && verificado) user.Classificacao = "ONG Verificada";
            else if (tempoDeConta.TotalDays <= 21) user.Classificacao = verificado ? "Novo Usuário Verificado" : "Novo Usuário";
            else user.Classificacao = verificado ? "Usuário Verificado" : "Usuário";
        }

        public void NotifyStateChanged() => OnChage?.Invoke();
    }
}
