using Microsoft.AspNetCore.SignalR;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.Conversas.SignalR.Hubs;

namespace ProjetoFinalCet105.API.UseCases.Conversas
{
    public class MarcarMensagensComoLidasUseCase
    {
        private readonly IConversaRepository _conversaRepository;
        private readonly IMensagemRepository _mensagemRepository;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MarcarMensagensComoLidasUseCase> _logger;

        public MarcarMensagensComoLidasUseCase(
            IConversaRepository conversaRepository,
            IMensagemRepository mensagemRepository,
            IHubContext<ChatHub> hubContext,
            ILogger<MarcarMensagensComoLidasUseCase> logger)
        {
            _conversaRepository = conversaRepository;
            _mensagemRepository = mensagemRepository;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int conversaId, string userId)
        {
            // Verificar se a conversa existe
            var conversa =await _conversaRepository.GetByIdWithDetailsAsync(conversaId);

            if (conversa == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Conversa não encontrada.",
                    TipoErro.NaoEncontrado);
            }

            // Só os participantes podem marcar mensagens como lidas
            var pertenceAConversa =
                conversa.ClienteId == userId ||
                conversa.FuncionarioUserId == userId;

            if (!pertenceAConversa)
            {
                return UseCaseResult<bool>.Falha(
                    "Não tem permissão para aceder a esta conversa.",
                    TipoErro.Proibido);
            }

  
            var mensagens = await _mensagemRepository.GetNaoLidasAsync(conversaId,userId);

            if (!mensagens.Any())
            {
                return UseCaseResult<bool>.Ok(true);
            }

            var agora = DateTime.Now;

            foreach (var mensagem in mensagens)
            {
                mensagem.Lida = true;
                mensagem.DataLeitura = agora;

                await _mensagemRepository
                    .UpdateAsync(mensagem);
            }

            try
            {
                await _hubContext.Clients
                    .Group($"conversa-{conversaId}")
                    .SendAsync(
                        "MensagensLidas",
                        new
                        {
                            ConversaId = conversaId,
                            LidasPorUserId = userId,
                            DataLeitura = agora
                        });
            }
            catch (Exception ex)
            {
                _logger.LogWarning( ex,"As mensagens da conversa {ConversaId} foram marcadas como lidas na BD, mas falhou a notificação SignalR.", conversaId);
            }

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
