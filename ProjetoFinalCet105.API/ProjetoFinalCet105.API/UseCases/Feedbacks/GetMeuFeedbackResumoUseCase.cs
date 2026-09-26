using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Feedbacks;

public class GetMeuFeedbackResumoUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly GetFeedbackResumoFuncionarioUseCase
        _getFeedbackResumoFuncionarioUseCase;

    public GetMeuFeedbackResumoUseCase(IFuncionarioRepository funcionarioRepository, GetFeedbackResumoFuncionarioUseCase getFeedbackResumoFuncionarioUseCase)
    {
        _funcionarioRepository = funcionarioRepository;
        _getFeedbackResumoFuncionarioUseCase = getFeedbackResumoFuncionarioUseCase;
    }

    public async Task<UseCaseResult<FeedbackResumoDTO>> ExecuteAsync(string userId)
    {
        var funcionario = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

        if (funcionario == null)
        {
            return UseCaseResult<FeedbackResumoDTO>.Falha("Funcionário autenticado não encontrado.", TipoErro.Proibido);
        }

        return await _getFeedbackResumoFuncionarioUseCase.ExecuteAsync(funcionario.Id);
    }
}