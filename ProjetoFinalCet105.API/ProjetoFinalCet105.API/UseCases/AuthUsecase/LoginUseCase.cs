using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.Services.EmailService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
        public class LoginUseCase
        {
            private readonly UserManager<User> _userManager;
            private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public LoginUseCase(UserManager<User> userManager,IAuthService authService, IEmailService emailService)
            {
                _userManager = userManager;
                _authService = authService;
            _emailService = emailService;
        }

            public async Task<UseCaseResult<LoginResponseDTO>> ExecuteAsync(
                LoginDTO dto)
            {
                var user =
                    await _userManager.FindByEmailAsync(dto.Email);

                if (user == null)
                {
                    return UseCaseResult<LoginResponseDTO>.Falha(
                        "Email ou password inválidos.",
                        TipoErro.NaoAutorizado);
                }

                var passwordValida = await _userManager.CheckPasswordAsync(user,dto.Password);

                if (!passwordValida)
                {
                    return UseCaseResult<LoginResponseDTO>.Falha(
                        "Email ou password inválidos.",
                        TipoErro.NaoAutorizado);
                }

                if (!user.Ativo)
                {
                    return UseCaseResult<LoginResponseDTO>.Falha(
                        "O utilizador encontra-se desativado.",
                        TipoErro.NaoAutorizado);
                }

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                var codigo =await _userManager.GenerateTwoFactorTokenAsync(user,TokenOptions.DefaultEmailProvider);

                await _emailService.EnviarEmailAsync(
                    user.Email!,
                    "Código de autenticação",
                    $"O seu código de autenticação é: <strong>{codigo}</strong>");

                return UseCaseResult<LoginResponseDTO>.Ok(
                    new LoginResponseDTO
                    {
                        UserId = user.Id,
                        NomeCompleto = user.NomeCompleto,
                        Email = user.Email!,
                        Roles = await _userManager.GetRolesAsync(user),
                        RequiresTwoFactor = true,
                        Token = null
                    });
            }

            var resposta =
                    await _authService
                        .GerarRespostaLoginAsync(user);

                return UseCaseResult<LoginResponseDTO>.Ok(
                    resposta);
            }
        }
}


