using crudcomdb.Data;
using Microsoft.EntityFrameworkCore;

namespace crudcomdb.Services
{
    // Acompanhamento pós-adoção e lembretes de Achados e Perdidos: roda sozinho a cada 6 horas.
    //  - 7 dias depois da adoção: dicas de adaptação para o adotante.
    //  - 30 dias depois: convida o adotante a contar como o pet está (galeria Final Feliz)
    //    e avisa o doador que pode perguntar pelo chat.
    // A "chave" de cada notificação garante que ninguém recebe o mesmo lembrete duas vezes.
    public class AcompanhamentoService : BackgroundService
    {
        private readonly IServiceProvider _servicos;
        private readonly ILogger<AcompanhamentoService> _log;

        public AcompanhamentoService(IServiceProvider servicos, ILogger<AcompanhamentoService> log)
        {
            _servicos = servicos;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken parar)
        {
            // Espera o site terminar de subir
            try { await Task.Delay(TimeSpan.FromMinutes(2), parar); } catch (TaskCanceledException) { return; }

            while (!parar.IsCancellationRequested)
            {
                try { await VerificarAsync(); }
                catch (Exception ex) { _log.LogWarning(ex, "Falha no acompanhamento pós-adoção"); }

                try { await Task.Delay(TimeSpan.FromHours(6), parar); } catch (TaskCanceledException) { return; }
            }
        }

        public async Task VerificarAsync()
        {
            using var escopo = _servicos.CreateScope();
            var fabrica = escopo.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var notificacoes = escopo.ServiceProvider.GetRequiredService<NotificacaoService>();
            await using var db = await fabrica.CreateDbContextAsync();

            // Achados e Perdidos: a cada 2 dias, pergunta a quem anunciou se o pet já foi encontrado
            try { await escopo.ServiceProvider.GetRequiredService<AchadosPerdidosService>().EnviarLembretesAsync(); }
            catch (Exception ex) { _log.LogWarning(ex, "Falha nos lembretes de Achados e Perdidos"); }

            var agora = DateTime.UtcNow;
            var adotados = await db.Pets.AsNoTracking()
                .Where(p => p.Status == "Adotado" && p.AdotanteId != null && p.DataAdocao != null
                            && p.DataAdocao >= agora.AddDays(-45) && p.DataAdocao <= agora.AddDays(-7))
                .Select(p => new { p.Id, p.Nome, p.AdotanteId, p.UsuarioDoadorId, p.DataAdocao })
                .ToListAsync();

            foreach (var p in adotados)
            {
                var dias = (agora - p.DataAdocao!.Value).TotalDays;
                await notificacoes.CriarAsync(p.AdotanteId!.Value, null, "acompanhamento",
                    $"Uma semana com {p.Nome}! Dica: mantenha a rotina de comida e passeios e leve ao veterinário para um check-up. 🩺",
                    $"/pets/detalhes/{p.Id}", $"acomp7:{p.Id}");

                if (dias >= 30)
                {
                    await notificacoes.CriarAsync(p.AdotanteId.Value, null, "acompanhamento",
                        $"Já faz um mês que {p.Nome} chegou! Que tal contar como ele está na galeria Final Feliz? 💛",
                        "/final-feliz", $"acomp30:{p.Id}");
                    await notificacoes.CriarAsync(p.UsuarioDoadorId, null, "acompanhamento",
                        $"Faz um mês que {p.Nome} foi adotado. Mande uma mensagem ao adotante para saber como ele está! 🐾",
                        "/mensagens", $"acomp30doador:{p.Id}");
                }
            }
        }
    }
}
