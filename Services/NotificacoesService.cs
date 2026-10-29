namespace crudcomdb.Services
{
    // Avisos em tempo real entre usuários conectados (nova mensagem, termo, pedido de amizade).
    // É um serviço único para o site todo: quem está com a tela aberta recebe o aviso na hora.
    public class NotificacoesService
    {
        public record Aviso(int UsuarioId, string Tipo, int ReferenciaId);

        public event Action<Aviso>? OnAviso;

        public void Avisar(int usuarioId, string tipo, int referenciaId = 0) =>
            OnAviso?.Invoke(new Aviso(usuarioId, tipo, referenciaId));
    }
}
