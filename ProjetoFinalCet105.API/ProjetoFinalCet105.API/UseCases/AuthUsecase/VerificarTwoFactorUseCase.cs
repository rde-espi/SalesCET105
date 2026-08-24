using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class VerificarTwoFactorUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;

        public VerificarTwoFactorUseCase(
            UserManager<User> userManager,
            IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<UseCaseResult<LoginResponseDTO>> ExecuteAsync(VerificarTwoFactorDTO dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null || !user.Ativo)
            {
                return UseCaseResult<LoginResponseDTO>.Falha(
                    "Código inválido.",
                    TipoErro.NaoAutorizado);
            }

            var codigoValido =
                await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    TokenOptions.DefaultEmailProvider,
                    dto.Codigo);

            if (!codigoValido)
            {
                return UseCaseResult<LoginResponseDTO>.Falha(
                    "Código de autenticação inválido.",
                    TipoErro.NaoAutorizado);
            }

            var resposta =
                await _authService.GerarRespostaLoginAsync(user);

            resposta.RequiresTwoFactor = false;

            return UseCaseResult<LoginResponseDTO>.Ok(resposta);
        }
    }
}
