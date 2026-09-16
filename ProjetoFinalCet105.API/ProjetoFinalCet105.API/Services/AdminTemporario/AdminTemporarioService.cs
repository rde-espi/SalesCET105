using Microsoft.EntityFrameworkCore;

using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Services.AdminTemporario;

public class AdminTemporarioService
{
    private readonly IPermissaoAdminTemporariaRepository _permissaoRepository;

    public AdminTemporarioService(
        IPermissaoAdminTemporariaRepository permissaoRepository)
    {
        _permissaoRepository = permissaoRepository;
    }

    public async Task<EstadoAdminTemporario> ObterEstadoAsync(string userId)
    {
        var agora = DateTime.UtcNow;

        var permissao = await _permissaoRepository
            .GetAllWithUsers()
            .Where(p =>
                p.FuncionarioUserId == userId &&
                !p.Revogada &&
                p.DataInicio <= agora &&
                p.DataFim > agora)
            .OrderByDescending(p => p.DataFim)
            .FirstOrDefaultAsync();

        if (permissao == null)
        {
            return new EstadoAdminTemporario
            {
                Ativo = false
            };
        }

        return new EstadoAdminTemporario
        {
            Ativo = true,
            DataInicio = permissao.DataInicio,
            DataFim = permissao.DataFim
        };
    }

    public async Task<bool> TemPermissaoAtivaAsync(string userId)
    {
        var estado = await ObterEstadoAsync(userId);
        return estado.Ativo;
    }
}
