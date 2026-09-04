using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.FirebaseService;

namespace ProjetoFinalCet105.API.Services.NotificacaoService
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly INotificacaoRepository _notificacaoRepository;
        private readonly IDispositivoUserRepository _dispositivoUserRepository;
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<NotificacaoService> _logger;

        public NotificacaoService(INotificacaoRepository notificacaoRepository,
            IDispositivoUserRepository dispositivoUserRepository,
            IFirebaseService firebaseService,
            ILogger<NotificacaoService> logger)
        {
            _notificacaoRepository = notificacaoRepository;
            _dispositivoUserRepository = dispositivoUserRepository;
            _firebaseService = firebaseService;
            _logger = logger;
        }

        public async Task CriarNotificacaoAsync( string userId, string titulo, string mensagem)
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
                       
            try
            {
                var dispositivos = await _dispositivoUserRepository
                    .GetAtivosByUserId(userId)
                    .ToListAsync();

                foreach (var dispositivo in dispositivos)
                {
                    try
                    {
                        await _firebaseService.EnviarPushAsync( dispositivo.Fid, titulo, mensagem);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "Falha ao enviar push para o dispositivo {DispositivoId} do utilizador {UserId}.",
                            dispositivo.Id,
                            userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,"Não foi possível processar o envio push da notificação para o utilizador {UserId}.", userId);
            }
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
                await CriarNotificacaoAsync( funcionarioUserId, "Nova marcação", mensagem);

                return;
            }

            if (isFuncionario && !isAdmin)
            {
                await CriarNotificacaoAsync( clienteUserId, "Nova marcação", mensagem);

                return;
            }

            if (isAdmin)
            {
                await CriarNotificacaoAsync( clienteUserId, "Nova marcação", mensagem);

                await CriarNotificacaoAsync( funcionarioUserId, "Nova marcação",  mensagem);
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
                await CriarNotificacaoAsync( funcionarioUserId, "Marcação alterada", mensagem);

                return;
            }

            if (isFuncionario && !isAdmin)
            {
                await CriarNotificacaoAsync( clienteUserId, "Marcação alterada", mensagem);

                return;
            }

            if (isAdmin)
            {
                await CriarNotificacaoAsync( clienteUserId, "Marcação alterada", mensagem);

                await CriarNotificacaoAsync( funcionarioUserId,"Marcação alterada",   mensagem);
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

            await CriarNotificacaoAsync( clienteUserId, titulo, mensagem);
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
                await CriarNotificacaoAsync( funcionarioUserId, "Marcação cancelada",  mensagem);

                return;
            }

            // Funcionário cancelou → cliente recebe
            if (isFuncionario && !isAdmin)
            {
                await CriarNotificacaoAsync( clienteUserId, "Marcação cancelada", mensagem);

                return;
            }

            // Admin cancelou → ambos recebem
            if (isAdmin)
            {
                await CriarNotificacaoAsync( clienteUserId, "Marcação cancelada",  mensagem);

                await CriarNotificacaoAsync( funcionarioUserId, "Marcação cancelada",  mensagem);
            }
        }
        public async Task NotificarNovaMensagemAsync( string destinatarioId,string remetenteNome)
        {
            await CriarNotificacaoAsync( destinatarioId, "Nova mensagem", $"Recebeu uma nova mensagem de {remetenteNome}.");
        }
    }
}
