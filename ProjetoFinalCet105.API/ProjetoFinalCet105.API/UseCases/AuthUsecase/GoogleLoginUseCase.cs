using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.AuthService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class GoogleLoginUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleLoginUseCase> _logger;

        public GoogleLoginUseCase(UserManager<User> userManager, IAuthService authService, IConfiguration configuration, ILogger<GoogleLoginUseCase> logger)
        {
            _userManager = userManager;
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UseCaseResult<LoginResponseDTO>> ExecuteAsync( GoogleLoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
            {
                return UseCaseResult<LoginResponseDTO>.Falha("Token Google inválido.",TipoErro.NaoAutorizado);
            }

            GoogleJsonWebSignature.Payload payload;

            try
            {
                var clientId = _configuration["GoogleAuth:ClientId"];

                if (string.IsNullOrWhiteSpace(clientId))
                {
                    return UseCaseResult<LoginResponseDTO>.Falha(
                        "A autenticação Google não está configurada.");
                }

                payload = await GoogleJsonWebSignature.ValidateAsync(
                    dto.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { clientId }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning( ex, "Falha na validação do token de autenticação Google.");

                return UseCaseResult<LoginResponseDTO>.Falha( "Token Google inválido.", TipoErro.NaoAutorizado);
            }

            if (string.IsNullOrWhiteSpace(payload.Subject) ||
                string.IsNullOrWhiteSpace(payload.Email))
            {
                return UseCaseResult<LoginResponseDTO>.Falha("A conta Google não contém os dados necessários.", TipoErro.NaoAutorizado);
            }

            var user = _userManager.Users.FirstOrDefault(u => u.GoogleId == payload.Subject);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
            }

            if (user != null)
            {
                if (!user.Ativo)
                {
                    return UseCaseResult<LoginResponseDTO>.Falha("O utilizador encontra-se desativado.",TipoErro.NaoAutorizado);
                }

                if (string.IsNullOrWhiteSpace(user.GoogleId))
                {
                    user.GoogleId = payload.Subject;

                    var resultadoUpdate = await _userManager.UpdateAsync(user);

                    if (!resultadoUpdate.Succeeded)
                    {
                        return UseCaseResult<LoginResponseDTO>.Falha(
                            "Não foi possível associar a conta Google.");
                    }
                }

                var respostaExistente = await _authService.GerarRespostaLoginAsync(user);

                return UseCaseResult<LoginResponseDTO>.Ok(respostaExistente);
            }

            var novoUser = new User
            {
                UserName = payload.Email,
                Email = payload.Email,
                NomeCompleto = payload.Name ?? payload.Email,
                GoogleId = payload.Subject,
                EmailConfirmed = payload.EmailVerified,
                Ativo = true
            };

            var resultadoCriacao = await _userManager.CreateAsync(novoUser);

            if (!resultadoCriacao.Succeeded)
            {
                var erros = string.Join("; ", resultadoCriacao.Errors.Select(e => e.Description));

                return UseCaseResult<LoginResponseDTO>.Falha($"Não foi possível criar o utilizador: {erros}");
            }

            var resultadoRole = await _userManager.AddToRoleAsync( novoUser,"Cliente");

            if (!resultadoRole.Succeeded)
            {
                await _userManager.DeleteAsync(novoUser);

                return UseCaseResult<LoginResponseDTO>.Falha("Não foi possível atribuir o perfil Cliente.");
            }

            var resposta = await _authService.GerarRespostaLoginAsync(novoUser);

            return UseCaseResult<LoginResponseDTO>.Ok(resposta);
        }
    }
}
