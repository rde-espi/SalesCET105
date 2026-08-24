using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class ResetPasswordUseCase
    {
        private readonly UserManager<User> _userManager;

        public ResetPasswordUseCase(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            ResetPasswordDTO dto)
        {
            var user =
                await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Pedido de recuperação inválido.");
            }

            if (!user.Ativo)
            {
                return UseCaseResult<bool>.Falha(
                    "Pedido de recuperação inválido.");
            }

            var resultado =
                await _userManager.ResetPasswordAsync(
                    user,
                    dto.Token,
                    dto.NovaPassword);

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
