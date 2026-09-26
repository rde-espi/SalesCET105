using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks
{
    public class GetFeedbackByMarcacaoUseCase
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public GetFeedbackByMarcacaoUseCase( IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<UseCaseResult<FeedbackDTO>> ExecuteAsync( int marcacaoId, string userId, bool isAdmin)
        {
            var feedback = await _feedbackRepository.GetByMarcacaoIdWithDetailsAsync(marcacaoId);

            if (feedback == null)
            {
                return UseCaseResult<FeedbackDTO>.Falha("Feedback não encontrado.", TipoErro.NaoEncontrado);
            }

           
            if (!isAdmin && feedback.ClienteId != userId)
            {
                return UseCaseResult<FeedbackDTO>.Falha( "Não tem permissão para consultar este feedback.", TipoErro.Proibido);
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