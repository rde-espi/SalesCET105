using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.EmailService;

namespace ProjetoFinalCet105.API.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<User> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<LoginResponseDTO> GerarRespostaLoginAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Name,user.NomeCompleto),
                new Claim(ClaimTypes.Email,user.Email!),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDTO
            {
                Token = tokenString,
                UserId = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email!,
                Roles = roles
            };
        }

        public async Task EnviarConfirmacaoEmailAsync(User user, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("O utilizador não possui um email válido.");
            }

            if (user.EmailConfirmed)
            {
                return;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var webBaseUrl = _configuration["WebSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(webBaseUrl))
            {
                throw new InvalidOperationException(
                    "A configuração WebSettings:BaseUrl não foi definida.");
            }

            var emailCodificado = Uri.EscapeDataString(user.Email);
            var tokenCodificado = Uri.EscapeDataString(token);

            var link =
                $"{webBaseUrl}/Account/ConfirmarEmail" +
                $"?email={emailCodificado}&token={tokenCodificado}";

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                link += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
            }

            var mensagem = $@"
    <h2>Confirme o seu email</h2>

    <p>Olá {user.NomeCompleto},</p>

    <p>Obrigado por criar a sua conta na Infinity Beauty.</p>

    <p>
        Para concluir o seu registo e ativar a sua conta,
        confirme o seu endereço de email através do botão abaixo:
    </p>

    <p style=""margin: 30px 0;"">
        <a href=""{link}""
           style=""background-color:#191919;
                  color:#D8B071;
                  padding:14px 24px;
                  text-decoration:none;
                  border-radius:4px;
                  font-weight:bold;"">
            Confirmar o meu email
        </a>
    </p>

    <p>
        Se não criou esta conta, pode ignorar esta mensagem.
    </p>

    <p>
        Infinity Beauty<br>
        CENTER | SPA | WELLNESS
    </p>";

            await _emailService.EnviarEmailAsync(user.Email, "Confirmação de email", mensagem);
        }

        public async Task EnviarConviteFuncionarioAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("O funcionário não possui um email válido.");
            }

            var tokenConfirmacao = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenPassword = await _userManager.GeneratePasswordResetTokenAsync(user);

            var webBaseUrl = _configuration["WebSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(webBaseUrl))
            {
                throw new InvalidOperationException("A configuração WebSettings:BaseUrl não foi definida.");
            }

            var emailCodificado = Uri.EscapeDataString(user.Email);
            var tokenConfirmacaoCodificado = Uri.EscapeDataString(tokenConfirmacao);
            var tokenPasswordCodificado = Uri.EscapeDataString(tokenPassword);

            var link = $"{webBaseUrl}/Account/PrimeiroAcesso?email={emailCodificado}&tokenConfirmacao={tokenConfirmacaoCodificado}&tokenPassword={tokenPasswordCodificado}";
            var mensagem = $@"
        <h2>Bem-vindo(a) à Infinity Beauty</h2>

        <p>Olá {user.NomeCompleto},</p>

        <p>Foi criada uma conta de funcionário para si na plataforma Infinity Beauty.</p>

        <p>Para ativar a sua conta, confirme o seu endereço de email e escolha a sua palavra-passe através do botão abaixo:</p>

        <p style=""margin: 30px 0;"">
            <a href=""{link}""
               style=""background-color:#191919;
                      color:#D8B071;
                      padding:14px 24px;
                      text-decoration:none;
                      border-radius:4px;
                      font-weight:bold;"">
                Ativar a minha conta
            </a>
        </p>

        <p>Por motivos de segurança, não partilhe este link.</p>

        <p>Se não estava à espera desta mensagem, contacte a administração da Infinity Beauty.</p>

        <p>Infinity Beauty<br>
        CENTER | SPA | WELLNESS</p>";

            await _emailService.EnviarEmailAsync(user.Email, "Ativação da sua conta - Infinity Beauty", mensagem);
        }

    }
}