using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Notificacoes
{
    public class MarcarTodasComoLidasUseCase
    {
        private readonly INotificacaoRepository _notificacaoRepository;

        public MarcarTodasComoLidasUseCase(INotificacaoRepository notificacaoRepository)
        {
            _notificacaoRepository = notificacaoRepository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
           string userId)
        {
            var notificacoes =
                await _notificacaoRepository
                    .GetNaoLidasByUserIdAsync(userId);

            if (!notificacoes.Any())
            {
                return UseCaseResult<bool>.Ok(true);
            }

            var agora = DateTime.Now;

            foreach (var notificacao in notificacoes)
            {
                notificacao.Lida = true;
                notificacao.DataLeitura = agora;

                await _notificacaoRepository
                    .UpdateAsync(notificacao);
            }

            return UseCaseResult<bool>.Ok(true);
        }

    }
}
