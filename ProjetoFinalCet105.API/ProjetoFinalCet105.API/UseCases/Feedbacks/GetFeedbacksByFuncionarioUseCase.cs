using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks
{
    public class GetFeedbacksByFuncionarioUseCase
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public GetFeedbacksByFuncionarioUseCase(
            IFeedbackRepository feedbackRepository,
            IFuncionarioRepository funcionarioRepository)
        {
            _feedbackRepository = feedbackRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        public async Task<UseCaseResult<IEnumerable<FeedbackDTO>>> ExecuteAsync(
            int funcionarioId)
        {
            var funcionario = await _funcionarioRepository.GetByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<IEnumerable<FeedbackDTO>>.Falha(
                    "Funcionário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            var feedbacks = await _feedbackRepository
                .GetAllWithDetails()
                .Where(f => f.FuncionarioId == funcionarioId)
                .OrderByDescending(f => f.DataCriacao)
                .Select(f => new FeedbackDTO
                {
                    Id = f.Id,
                    MarcacaoId = f.MarcacaoId,

                    ClienteId = f.ClienteId,
                    ClienteNome = f.Cliente.NomeCompleto,

                    FuncionarioId = f.FuncionarioId,
                    FuncionarioNome = f.Funcionario.User.NomeCompleto,

                    Classificacao = f.Classificacao,
                    Comentario = f.Comentario,
                    DataCriacao = f.DataCriacao
                })
                .ToListAsync();

            return UseCaseResult<IEnumerable<FeedbackDTO>>
                .Ok(feedbacks);
        }
    }
}
