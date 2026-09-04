using Microsoft.AspNetCore.SignalR;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.Conversas.SignalR.Hubs;

namespace ProjetoFinalCet105.API.UseCases.Conversas
{
    public class EnviarMensagemUseCase
    {
        private readonly IConversaRepository _conversaRepository;
        private readonly IMensagemRepository _mensagemRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<EnviarMensagemUseCase> _logger;

        public EnviarMensagemUseCase(IConversaRepository conversaRepository, IMensagemRepository mensagemRepository, INotificacaoService notificacaoService,
            IHubContext<ChatHub> hubContext, ILogger<EnviarMensagemUseCase> logger)
        {
            _conversaRepository = conversaRepository;
            _mensagemRepository = mensagemRepository;
            _notificacaoService = notificacaoService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<UseCaseResult<MensagemDTO>> ExecuteAsync(int conversaId, string userId, EnviarMensagemDTO dto)
        {
            var conversa = await _conversaRepository.GetByIdWithDetailsAsync(conversaId);

            if (conversa == null)
            {
                return UseCaseResult<MensagemDTO>.Falha("Conversa não encontrada.", TipoErro.NaoEncontrado);
            }

            var pertenceAConversa = conversa.ClienteId == userId || conversa.FuncionarioUserId == userId;

            if (!pertenceAConversa)
            {
                return UseCaseResult<MensagemDTO>.Falha("Não tem permissão para enviar mensagens nesta conversa.", TipoErro.Proibido);
            }

            if (string.IsNullOrWhiteSpace(dto.Texto))
            {
                return UseCaseResult<MensagemDTO>.Falha("A mensagem não pode estar vazia.");
            }

            try
            {
                var mensagem = new Mensagem
                {
                    ConversaId = conversa.Id,

                    
                    RemetenteId = userId,

                    Texto = dto.Texto.Trim(),

                    DataEnvio = DateTime.Now,

                    
                    Lida = false,
                    DataLeitura = null
                };

                await _mensagemRepository.CreateAsync(mensagem);

                var remetente =
                    conversa.ClienteId == userId
                    ? conversa.Cliente
                    : conversa.Funcionario;

                var destinatarioId =
                    conversa.ClienteId == userId
                    ? conversa.FuncionarioUserId
                    : conversa.ClienteId;

                try
                {
                    await _notificacaoService.NotificarNovaMensagemAsync(destinatarioId, remetente.NomeCompleto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao criar notificação de nova mensagem para o utilizador {DestinatarioId}.", destinatarioId);
                }

                var resposta = new MensagemDTO
                {
                    Id = mensagem.Id,
                    ConversaId = mensagem.ConversaId,

                    RemetenteId = userId,
                    RemetenteNome = remetente.NomeCompleto,

                    Texto = mensagem.Texto,
                    DataEnvio = mensagem.DataEnvio,

                    Lida = mensagem.Lida,
                    DataLeitura = mensagem.DataLeitura
                };
                try
                {
                    await _hubContext.Clients
                        .Group($"conversa-{conversa.Id}")
                        .SendAsync("NovaMensagem", resposta);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao enviar mensagem por SignalR na conversa {ConversaId}.", conversa.Id);
                }

                return UseCaseResult<MensagemDTO>.Ok(resposta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem na conversa {ConversaId}.", conversaId);

                return UseCaseResult<MensagemDTO>.Falha("Ocorreu um erro ao enviar a mensagem.");
            }
        }
    }
}
