using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.FirebaseService;
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
        private readonly IDispositivoUserRepository _dispositivoUserRepository;
        private readonly IFirebaseService _firebaseService;

        public EnviarMensagemUseCase(IConversaRepository conversaRepository, IMensagemRepository mensagemRepository, INotificacaoService notificacaoService,
            IHubContext<ChatHub> hubContext, IDispositivoUserRepository dispositivoUserRepository, IFirebaseService firebaseService)
        {
            _conversaRepository = conversaRepository;
            _mensagemRepository = mensagemRepository;
            _notificacaoService = notificacaoService;
            _hubContext = hubContext;
            _dispositivoUserRepository = dispositivoUserRepository;
            _firebaseService = firebaseService;
        }

        public async Task<UseCaseResult<MensagemDTO>> ExecuteAsync(int conversaId, string userId, EnviarMensagemDTO dto)
        {
            // 1. Verificar se a conversa existe
            var conversa = await _conversaRepository.GetByIdWithDetailsAsync(conversaId);

            if (conversa == null)
            {
                return UseCaseResult<MensagemDTO>.Falha(
                    "Conversa não encontrada.",
                    TipoErro.NaoEncontrado);
            }

            // 2. Só os participantes podem enviar mensagens
            var pertenceAConversa =
                conversa.ClienteId == userId ||
                conversa.FuncionarioUserId == userId;

            if (!pertenceAConversa)
            {
                return UseCaseResult<MensagemDTO>.Falha(
                    "Não tem permissão para enviar mensagens nesta conversa.",
                    TipoErro.Proibido);
            }

            // 3. Não aceitar mensagem vazia
            if (string.IsNullOrWhiteSpace(dto.Texto))
            {
                return UseCaseResult<MensagemDTO>.Falha("A mensagem não pode estar vazia.");
            }

            try
            {
                var mensagem = new Mensagem
                {
                    ConversaId = conversa.Id,

                    // Nunca vem do DTO
                    RemetenteId = userId,

                    Texto = dto.Texto.Trim(),

                    DataEnvio = DateTime.Now,

                    // O destinatário ainda não a leu
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
                catch
                {
                    // Uma falha na notificação não deve impedir
                    // o envio da mensagem.
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
                        .SendAsync(
                            "NovaMensagem",
                            resposta);
                }
                catch
                {
                    // Uma falha no SignalR não deve impedir
                    // que a mensagem seja guardada.
                }
                try
                {
                    var dispositivos =
                        await _dispositivoUserRepository
                            .GetAtivosByUserId(destinatarioId)
                            .ToListAsync();

                    foreach (var dispositivo in dispositivos)
                    {
                        await _firebaseService.EnviarPushAsync(
                            dispositivo.Fid,
                            $"Nova mensagem de {remetente.NomeCompleto}",
                            mensagem.Texto);
                    }
                }
                catch
                {
                    // Uma falha no push não deve impedir
                    // o envio da mensagem.
                }

                return UseCaseResult<MensagemDTO>.Ok(resposta);
            }
            catch (Exception)
            {
                return UseCaseResult<MensagemDTO>.Falha("Ocorreu um erro ao enviar a mensagem.");
            }
        }
    }
}
