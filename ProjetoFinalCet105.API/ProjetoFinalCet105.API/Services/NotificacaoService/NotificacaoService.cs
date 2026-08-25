using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Services.NotificacaoService
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly INotificacaoRepository _notificacaoRepository;

        public NotificacaoService(INotificacaoRepository notificacaoRepository)
        {
            _notificacaoRepository = notificacaoRepository;
        }

        public async Task CriarNotificacaoAsync(string userId,string titulo,string mensagem)
        {
            var notificacao = new Notificacao
            {
                UserId = userId,
                Titulo = titulo,
                Mensagem = mensagem,
                Lida = false,
                DataCriacao = DateTime.Now
            };

            await _notificacaoRepository.CreateAsync(notificacao);
        }

        public async Task NotificarCriacaoMarcacaoAsync(
            string clienteUserId,
            string funcionarioUserId,
            string servicoNome,
            DateTime dataHoraInicio,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin)
        {
            var mensagem =
                $"Foi criada uma nova marcação para o serviço " +
                $"'{servicoNome}' no dia " +
                $"{dataHoraInicio:dd/MM/yyyy} às {dataHoraInicio:HH:mm}.";

            if (isCliente && !isAdmin)
            {
                await CriarNotificacaoAsync(
                    funcionarioUserId,
                    "Nova marcação",
                    mensagem);

                return;
            }

            if (isFuncionario && !isAdmin)
            {
                await CriarNotificacaoAsync(
                    clienteUserId,
                    "Nova marcação",
                    mensagem);

                return;
            }

            if (isAdmin)
            {
                await CriarNotificacaoAsync(
                    clienteUserId,
                    "Nova marcação",
                    mensagem);

                await CriarNotificacaoAsync(
                    funcionarioUserId,
                    "Nova marcação",
                    mensagem);
            }
        }
        public async Task NotificarAlteracaoMarcacaoAsync(
            string clienteUserId,
            string funcionarioUserId,
            string servicoNome,
            DateTime dataHoraInicio,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin)
        {
            var mensagem =
                $"A marcação do serviço '{servicoNome}' " +
                $"foi alterada para o dia " +
                $"{dataHoraInicio:dd/MM/yyyy} às " +
                $"{dataHoraInicio:HH:mm}.";

            if (isCliente && !isAdmin)
            {
                await CriarNotificacaoAsync(
                    funcionarioUserId,
                    "Marcação alterada",
                    mensagem);

                return;
            }

            if (isFuncionario && !isAdmin)
            {
                await CriarNotificacaoAsync(
                    clienteUserId,
                    "Marcação alterada",
                    mensagem);

                return;
            }

            if (isAdmin)
            {
                await CriarNotificacaoAsync(
                    clienteUserId,
                    "Marcação alterada",
                    mensagem);

                await CriarNotificacaoAsync(
                    funcionarioUserId,
                    "Marcação alterada",
                    mensagem);
            }
        }

        public async Task NotificarEstadoMarcacaoAsync(string clienteUserId,string novoEstado,DateTime dataHoraInicio)
        {
            string titulo;
            string mensagem;

            if (novoEstado == "Confirmada")
            {
                titulo = "Marcação confirmada";
                mensagem =
                    $"A sua marcação para o dia " +
                    $"{dataHoraInicio:dd/MM/yyyy} às " +
                    $"{dataHoraInicio:HH:mm} foi confirmada.";
            }
            else if (novoEstado == "Não Compareceu")
            {
                titulo = "Não comparência";
                mensagem =
                    $"A marcação do dia " +
                    $"{dataHoraInicio:dd/MM/yyyy} às " +
                    $"{dataHoraInicio:HH:mm} foi registada como não comparecimento.";
            }
            else
            {
                return;
            }

            await CriarNotificacaoAsync(
                clienteUserId,
                titulo,
                mensagem);
        }

        public async Task NotificarCancelamentoMarcacaoAsync(
            string clienteUserId,
            string funcionarioUserId,
            DateTime dataHoraInicio,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin)
        {
            var mensagem =
                $"A marcação do dia " +
                $"{dataHoraInicio:dd/MM/yyyy} às " +
                $"{dataHoraInicio:HH:mm} foi cancelada.";

            // Cliente cancelou → funcionário recebe
            if (isCliente && !isAdmin)
            {
                await CriarNotificacaoAsync(
                    funcionarioUserId,
                    "Marcação cancelada",
                    mensagem);

                return;
            }

            // Funcionário cancelou → cliente recebe
            if (isFuncionario && !isAdmin)
            {
                await CriarNotificacaoAsync(
                    clienteUserId,
                    "Marcação cancelada",
                    mensagem);

                return;
            }

            // Admin cancelou → ambos recebem
            if (isAdmin)
            {
                await CriarNotificacaoAsync(
                    clienteUserId,
                    "Marcação cancelada",
                    mensagem);

                await CriarNotificacaoAsync(
                    funcionarioUserId,
                    "Marcação cancelada",
                    mensagem);
            }
        }
        public async Task NotificarNovaMensagemAsync(string destinatarioId, string remetenteNome)
        {
            var notificacao = new Notificacao
            {
                UserId = destinatarioId,
                Titulo = "Nova mensagem",
                Mensagem = $"Recebeu uma nova mensagem de {remetenteNome}.",
                Lida = false,
                DataCriacao = DateTime.Now
            };

            await _notificacaoRepository.CreateAsync(notificacao);
        }
    }
}
