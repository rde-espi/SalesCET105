using Microsoft.EntityFrameworkCore;

using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Notificacoes;

public class DeleteTodasNotificacoesUseCase
{
    private readonly INotificacaoRepository _notificacaoRepository;

    public DeleteTodasNotificacoesUseCase(INotificacaoRepository notificacaoRepository)
    {
        _notificacaoRepository = notificacaoRepository;
    }

    public async Task<UseCaseResult<bool>> ExecuteAsync(string userId)
    {
        var notificacoes = await _notificacaoRepository.GetByUserId(userId).ToListAsync();

        if (!notificacoes.Any())
        {
            return UseCaseResult<bool>.Ok(true);
        }

        foreach (var notificacao in notificacoes)
        {
            await _notificacaoRepository.DeleteAsync(notificacao);
        }

        return UseCaseResult<bool>.Ok(true);
    }
}
