using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks
{
    public class GetFeedbackByIdUseCase
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public GetFeedbackByIdUseCase(
            IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<UseCaseResult<FeedbackDTO>> ExecuteAsync(int id)
        {
            var feedback = await _feedbackRepository.GetByIdWithDetailsAsync(id);

            if (feedback == null)
            {
                return UseCaseResult<FeedbackDTO>.Falha(
                    "Feedback não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            var resposta = new FeedbackDTO
            {
                Id = feedback.Id,
                MarcacaoId = feedback.MarcacaoId,

                ClienteId = feedback.ClienteId,
                ClienteNome = feedback.Cliente.NomeCompleto,

                FuncionarioId = feedback.FuncionarioId,
                FuncionarioNome = feedback.Funcionario.User.NomeCompleto,

                Classificacao = feedback.Classificacao,
                Comentario = feedback.Comentario,
                DataCriacao = feedback.DataCriacao
            };

            return UseCaseResult<FeedbackDTO>.Ok(resposta);
        }
    }
}
