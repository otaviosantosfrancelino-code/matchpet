namespace crudcomdb.Services
{
    // Avisos que aparecem no canto da tela (substituem os "alert" do navegador)
    public class ToastService
    {
        public record Toast(Guid Id, string Tipo, string Mensagem);

        private readonly List<Toast> _toasts = new();
        public IReadOnlyList<Toast> Toasts => _toasts;
        public event Action? OnChange;

        public void Sucesso(string msg) => Mostrar("sucesso", msg);
        public void Erro(string msg) => Mostrar("erro", msg);
        public void Aviso(string msg) => Mostrar("aviso", msg);
        public void Info(string msg) => Mostrar("info", msg);

        private void Mostrar(string tipo, string msg)
        {
            var toast = new Toast(Guid.NewGuid(), tipo, msg);
            _toasts.Add(toast);
            OnChange?.Invoke();
            _ = RemoverDepois(toast, tipo == "erro" ? 7000 : 4500);
        }

        public void Fechar(Guid id)
        {
            _toasts.RemoveAll(t => t.Id == id);
            OnChange?.Invoke();
        }

        private async Task RemoverDepois(Toast toast, int ms)
        {
            await Task.Delay(ms);
            Fechar(toast.Id);
        }
    }
}
