using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Notificacoes
{
    public class MarcarNotificacaoLidaUseCase
    {
        private readonly INotificacaoRepository _notificacaoRepository;

        public MarcarNotificacaoLidaUseCase(INotificacaoRepository notificacaoRepository)
        {
            _notificacaoRepository = notificacaoRepository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id,string userId)
        {
            var notificacao =
                await _notificacaoRepository
                    .GetByIdAndUserIdAsync(id, userId);

            if (notificacao == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Notificação não encontrada.",
                    TipoErro.NaoEncontrado);
            }

            if (notificacao.Lida)
            {
                return UseCaseResult<bool>.Ok(true);
            }

            notificacao.Lida = true;
            notificacao.DataLeitura = DateTime.Now;

            await _notificacaoRepository.UpdateAsync(notificacao);

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
