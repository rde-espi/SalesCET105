using Microsoft.EntityFrameworkCore;

using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks;

public class GetAllFeedbacksUseCase
{
    private readonly IFeedbackRepository _feedbackRepository;

    public GetAllFeedbacksUseCase(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<UseCaseResult<IEnumerable<FeedbackDTO>>> ExecuteAsync()
    {
        var feedbacks = await _feedbackRepository
            .GetAllWithDetails()
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

        return UseCaseResult<IEnumerable<FeedbackDTO>>.Ok(feedbacks);
    }
}