using System.Collections.Concurrent;

namespace crudcomdb.Services
{
    // Freia ataques de "adivinhar senha" e envio em massa de e-mails.
    // Conta as tentativas por chave (ex.: "login:ip:usuario") numa janela de tempo e bloqueia ao passar do limite.
    public class LimiteTentativasService
    {
        private readonly ConcurrentDictionary<string, List<DateTime>> _tentativas = new();

        // Devolve quanto tempo falta para liberar (null = liberado)
        public TimeSpan? Bloqueio(string chave, int limite, TimeSpan janela)
        {
            if (!_tentativas.TryGetValue(chave, out var lista)) return null;
            lock (lista)
            {
                var agora = DateTime.UtcNow;
                lista.RemoveAll(t => t < agora - janela);
                if (lista.Count < limite) return null;
                return lista[0] + janela - agora;
            }
        }

        public void Registrar(string chave)
        {
            var lista = _tentativas.GetOrAdd(chave, _ => new List<DateTime>());
            lock (lista) { lista.Add(DateTime.UtcNow); if (lista.Count > 50) lista.RemoveAt(0); }
            if (_tentativas.Count > 20_000) Limpar();
        }

        public void Zerar(string chave) => _tentativas.TryRemove(chave, out _);

        public static string Tempo(TimeSpan t) => t.TotalMinutes >= 1 ? $"{Math.Ceiling(t.TotalMinutes)} min" : $"{Math.Max(1, (int)t.TotalSeconds)} s";

        private void Limpar()
        {
            var limite = DateTime.UtcNow.AddHours(-1);
            foreach (var (chave, lista) in _tentativas)
                lock (lista) if (lista.Count == 0 || lista[^1] < limite) _tentativas.TryRemove(chave, out _);
        }
    }
}
