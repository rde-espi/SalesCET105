using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class ConfirmarEmailUseCase
    {
        private readonly UserManager<User> _userManager;

        public ConfirmarEmailUseCase(
            UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            ConfirmarEmailDTO dto)
        {
            var user =
                await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Pedido de confirmação inválido.");
            }

            if (user.EmailConfirmed)
            {
                return UseCaseResult<bool>.Ok(true);
            }

            var resultado =
                await _userManager.ConfirmEmailAsync(
                    user,
                    dto.Token);

            if (!resultado.Succeeded)
            {
                return UseCaseResult<bool>.Falha(
                    "Token de confirmação inválido ou expirado.");
            }

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
