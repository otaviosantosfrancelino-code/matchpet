using crudcomdb.Models;

namespace crudcomdb.Services
{
    public class AuthService
    {
        public Usuario? UsuarioLogado { get; private set; }
        public bool EstaLogado => UsuarioLogado != null;
        public event Action? OnChage;

        public void Login(Usuario usuario)
        {
            AtualizarClassificacao(usuario);
            UsuarioLogado = usuario;
            NotifyStateChanged();
        }

        public void Logout()
        {
            UsuarioLogado = null;
            NotifyStateChanged();
        }

        public void AtualizarClassificacao(Usuario user)
        {
            if (user == null) return;
            bool temDocumento = !string.IsNullOrEmpty(user.FotoDocumentoBase64);
            var tempoDeConta = DateTime.UtcNow - user.DataCadastro;

            if (tempoDeConta.TotalDays <= 21) user.Classificacao = "Novo Usuário";
            else if (temDocumento) user.Classificacao = "Usuário Bronze";
        }

        public void NotifyStateChanged() => OnChage?.Invoke();

        // MÉTODO MODIFICADO PARA O TCC: Não envia mais e-mail real.
        public async Task EnviarEmailConfirmacaoAsync(string emailDestino, string codigo)
        {
            // Simula um tempo de carregamento super rápido apenas para a tela não piscar
            await Task.Delay(200);

            // Imprime o código apenas no terminal preto do Render/VS Code, caso você precise debugar
            Console.WriteLine($"[MODO APRESENTAÇÃO TCC] E-mail simulado para: {emailDestino}. Código gerado: {codigo}");

            // O método finaliza com sucesso absoluto. Nenhuma exception de falha de e-mail será gerada!
        }
    }
}