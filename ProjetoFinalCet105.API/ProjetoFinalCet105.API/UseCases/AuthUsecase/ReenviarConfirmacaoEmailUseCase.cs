using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class ReenviarConfirmacaoEmailUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;

        public ReenviarConfirmacaoEmailUseCase(
            UserManager<User> userManager,
            IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(ReenviarConfirmacaoEmailDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return UseCaseResult<bool>.Falha(
                    "O email é obrigatório.");
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return UseCaseResult<bool>.Ok(true);
            }

            if (!user.Ativo)
            {
                return UseCaseResult<bool>.Ok(true);
            }

            if (user.EmailConfirmed)
            {
                return UseCaseResult<bool>.Ok(true);
            }

            try
            {
                await _authService
                    .EnviarConfirmacaoEmailAsync(user);

                return UseCaseResult<bool>.Ok(true);
            }
            catch
            {
                return UseCaseResult<bool>.Falha(
                    "Não foi possível enviar o email de confirmação.");
            }
        }
    }
}
