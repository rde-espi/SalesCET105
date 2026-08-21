using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks
{
    public class UpdateFeedbackUseCase
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public UpdateFeedbackUseCase(
            IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id,string userId,bool isAdmin,UpdateFeedbackDTO dto)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);

            if (feedback == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Feedback não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (!isAdmin && feedback.ClienteId != userId)
            {
                return UseCaseResult<bool>.Falha(
                    "Não tem permissão para alterar este feedback.",
                    TipoErro.Proibido);
            }

            if (dto.Classificacao < 1 || dto.Classificacao > 5)
            {
                return UseCaseResult<bool>.Falha(
                    "A classificação deve estar entre 1 e 5.");
            }

            try
            {
                feedback.Classificacao = dto.Classificacao;
                feedback.Comentario = dto.Comentario;

                await _feedbackRepository.UpdateAsync(feedback);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao alterar o feedback.");
            }
        }
    }
}
