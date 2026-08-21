using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks
{
    public class GetFeedbackResumoFuncionarioUseCase
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public GetFeedbackResumoFuncionarioUseCase(
            IFeedbackRepository feedbackRepository,
            IFuncionarioRepository funcionarioRepository)
        {
            _feedbackRepository = feedbackRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        public async Task<UseCaseResult<FeedbackResumoDTO>> ExecuteAsync(
            int funcionarioId)
        {
            var funcionario =
                await _funcionarioRepository.GetByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<FeedbackResumoDTO>.Falha(
                    "Funcionário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            var query = _feedbackRepository
                .GetAll()
                .Where(f => f.FuncionarioId == funcionarioId);

            var total = await query.CountAsync();

            var media = total == 0
                ? 0
                : await query.AverageAsync(f => f.Classificacao);

            var resposta = new FeedbackResumoDTO
            {
                FuncionarioId = funcionarioId,
                Media = Math.Round(media, 1),
                TotalAvaliacoes = total
            };

            return UseCaseResult<FeedbackResumoDTO>.Ok(resposta);
        }
    }
}
