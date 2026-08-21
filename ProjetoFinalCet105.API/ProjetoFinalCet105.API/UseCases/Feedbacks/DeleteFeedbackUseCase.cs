using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks
{
    public class DeleteFeedbackUseCase
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public DeleteFeedbackUseCase(
            IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            int id,
            string userId,
            bool isAdmin)
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
                    "Não tem permissão para eliminar este feedback.",
                    TipoErro.Proibido);
            }

            try
            {
                await _feedbackRepository.DeleteAsync(feedback);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao eliminar o feedback.");
            }
        }
    }
}
