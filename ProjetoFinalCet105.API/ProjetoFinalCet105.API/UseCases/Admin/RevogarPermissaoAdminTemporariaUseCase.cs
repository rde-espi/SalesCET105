using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Admin
{
    public class RevogarPermissaoAdminTemporariaUseCase
    {
        private readonly IPermissaoAdminTemporariaRepository _permissaoRepository;
        private readonly INotificacaoService _notificacaoService;

        public RevogarPermissaoAdminTemporariaUseCase(IPermissaoAdminTemporariaRepository permissaoRepository,INotificacaoService notificacaoService)
        {
            _permissaoRepository = permissaoRepository;
            _notificacaoService = notificacaoService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int permissaoId, string adminUserId)
        {
            var permissao = await _permissaoRepository.GetByIdAsync(permissaoId);

            if (permissao == null)
            {
                return UseCaseResult<bool>.Falha("Permissão temporária não encontrada.", TipoErro.NaoEncontrado);
            }

            if (permissao.Revogada)
            {
                return UseCaseResult<bool>.Falha("Esta permissão temporária já foi revogada.");
            }

            var agora = DateTime.UtcNow;

            if (permissao.DataFim <= agora)
            {
                return UseCaseResult<bool>.Falha( "Esta permissão temporária já expirou.");
            }

            permissao.Revogada = true;
            permissao.DataRevogacao = agora;
            permissao.RevogadaPorUserId = adminUserId;

            await _permissaoRepository.UpdateAsync(permissao);

            await _notificacaoService.CriarNotificacaoAsync(
                permissao.FuncionarioUserId,
                "Privilégios administrativos revogados",
                "Os seus privilégios administrativos temporários foram revogados pelo administrador.");

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
