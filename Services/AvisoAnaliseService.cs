namespace crudcomdb.Services
{
    // Janela "Enviado para análise": aparece sempre que o usuário publica algo que
    // precisa ser aprovado pelo administrador antes de ficar público.
    // Fica no layout, então continua aberta mesmo se a página mudar (ex.: depois de anunciar um pet).
    public class AvisoAnaliseService
    {
        public record Aviso(string Titulo, string Oque, string Onde);

        public Aviso? Atual { get; private set; }
        public event Action? OnMudou;

        // oque: "Sua publicação", "Seu comentário"...  onde: "na Comunidade", "no catálogo"...
        public void Mostrar(string titulo, string oque, string onde)
        {
            Atual = new Aviso(titulo, oque, onde);
            OnMudou?.Invoke();
        }

        public void Fechar()
        {
            Atual = null;
            OnMudou?.Invoke();
        }
    }
}
