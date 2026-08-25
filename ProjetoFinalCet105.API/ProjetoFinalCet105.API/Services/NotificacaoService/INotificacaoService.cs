using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Services.NotificacaoService
{
    public interface INotificacaoService
    {
        Task CriarNotificacaoAsync(
            string userId,
            string titulo,
            string mensagem);

        Task NotificarCriacaoMarcacaoAsync(
            string clienteUserId,
            string funcionarioUserId,
            string servicoNome,
            DateTime dataHoraInicio,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin);

        Task NotificarAlteracaoMarcacaoAsync(
            string clienteUserId,
            string funcionarioUserId,
            string servicoNome,
            DateTime dataHoraInicio,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin);

        Task NotificarEstadoMarcacaoAsync(
            string clienteUserId,
            string novoEstado,
            DateTime dataHoraInicio);

        Task NotificarCancelamentoMarcacaoAsync(
            string clienteUserId,
            string funcionarioUserId,
            DateTime dataHoraInicio,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin);

        Task NotificarNovaMensagemAsync(
            string destinatarioId,
            string remetenteNome);
    }
}
