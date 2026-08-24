using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class GerirTwoFactorUseCase
    {
        private readonly UserManager<User> _userManager;

        public GerirTwoFactorUseCase(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            string userId,
            bool ativo)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Utilizador não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (!user.Ativo)
            {
                return UseCaseResult<bool>.Falha(
                    "O utilizador encontra-se desativado.");
            }

            var resultado =
                await _userManager.SetTwoFactorEnabledAsync(
                    user,
                    ativo);

            if (!resultado.Succeeded)
            {
                var erros = string.Join(
                    " ",
                    resultado.Errors.Select(e => e.Description));

                return UseCaseResult<bool>.Falha(erros);
            }

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
