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

        public RecuperarPasswordUseCase( UserManager<User> userManager,IEmailService emailService, ILogger<RecuperarPasswordUseCase> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            RecuperarPasswordDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return UseCaseResult<bool>.Falha(
                    "O email é obrigatório.");
            }

            var user =
                await _userManager.FindByEmailAsync(dto.Email);

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

                var mensagem = $@"
                    <h2>Recuperação de password</h2>

                    <p>Olá {user.NomeCompleto},</p>

                    <p>Foi solicitada a recuperação da password
                    da sua conta.</p>

                    <p>Utilize o seguinte código:</p>

                    <p><strong>{token}</strong></p>

                    <p>Se não solicitou esta alteração,
                    ignore este email.</p>";

                await _emailService.EnviarEmailAsync(user.Email!, "Recuperação de password", mensagem);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError( ex, "Erro ao enviar o email de recuperação de password.");

                return UseCaseResult<bool>.Falha("Não foi possível enviar o email de recuperação.");
            }
        }
    }
}
