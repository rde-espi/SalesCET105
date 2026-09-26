using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks;

public class GetMeusFeedbacksUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly GetFeedbacksByFuncionarioUseCase _getFeedbacksByFuncionarioUseCase;

    public GetMeusFeedbacksUseCase(IFuncionarioRepository funcionarioRepository, GetFeedbacksByFuncionarioUseCase getFeedbacksByFuncionarioUseCase)
    {
        _funcionarioRepository = funcionarioRepository;
        _getFeedbacksByFuncionarioUseCase = getFeedbacksByFuncionarioUseCase;
    }

    public async Task<UseCaseResult<IEnumerable<FeedbackDTO>>> ExecuteAsync(string userId)
    {
        var funcionario = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

        if (funcionario == null)
        {
            return UseCaseResult<IEnumerable<FeedbackDTO>>.Falha("Funcionário autenticado não encontrado.", TipoErro.Proibido);
        }

        return await _getFeedbacksByFuncionarioUseCase.ExecuteAsync(funcionario.Id);
    }
}