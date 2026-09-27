using Microsoft.AspNetCore.Identity;

using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.EmailService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.AuthUsecase
{
    public class RecuperarPasswordUseCase
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<RecuperarPasswordUseCase> _logger;
        private readonly IConfiguration _configuration;

        public RecuperarPasswordUseCase(UserManager<User> userManager, IEmailService emailService, ILogger<RecuperarPasswordUseCase> logger, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(RecuperarPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return UseCaseResult<bool>.Falha("O email é obrigatório.");
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

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var webBaseUrl = _configuration["WebSettings:BaseUrl"];

                if (string.IsNullOrWhiteSpace(webBaseUrl))
                {
                    throw new InvalidOperationException("A configuração WebSettings:BaseUrl não foi definida.");
                }

                var emailCodificado = Uri.EscapeDataString(user.Email!);
                var tokenCodificado = Uri.EscapeDataString(token);

                var link =
                    $"{webBaseUrl}/Account/ResetPassword" +
                    $"?email={emailCodificado}&token={tokenCodificado}";

                var mensagem = $@"
    <h2>Recuperação de palavra-passe</h2>

    <p>Olá {user.NomeCompleto},</p>

    <p>
        Foi solicitada a recuperação da palavra-passe
        da sua conta Infinity Beauty.
    </p>

    <p>
        Para definir uma nova palavra-passe,
        clique no botão abaixo:
    </p>

    <p style=""margin: 30px 0;"">
        <a href=""{link}""
           style=""background-color:#191919;
                  color:#D8B071;
                  padding:14px 24px;
                  text-decoration:none;
                  border-radius:4px;
                  font-weight:bold;"">
            Redefinir palavra-passe
        </a>
    </p>

    <p>
        Se não solicitou esta alteração,
        pode ignorar este email.
    </p>

    <p>
        Infinity Beauty<br>
        CENTER | SPA | WELLNESS
    </p>";

                await _emailService.EnviarEmailAsync(user.Email!, "Recuperação de password", mensagem);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar o email de recuperação de password.");

                return UseCaseResult<bool>.Falha("Não foi possível enviar o email de recuperação.");
            }
        }
    }
}