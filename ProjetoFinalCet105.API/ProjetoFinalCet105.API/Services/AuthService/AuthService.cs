using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Services.EmailService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjetoFinalCet105.API.Services.AuthService
{
    public class AuthService:IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<User> userManager,IConfiguration configuration,IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<LoginResponseDTO>GerarRespostaLoginAsync(User user)
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

            var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

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

        public async Task EnviarConfirmacaoEmailAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException(
                    "O utilizador não possui um email válido.");
            }

            if (user.EmailConfirmed)
            {
                return;
            }

            var token =
                await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var mensagem = $@"
        <h2>Confirmação de email</h2>

        <p>Olá {user.NomeCompleto},</p>

        <p>Obrigado pelo seu registo.</p>

        <p>Utilize o seguinte código para confirmar
        o seu endereço de email:</p>

        <p><strong>{token}</strong></p>

        <p>Se não efetuou este registo,
        ignore esta mensagem.</p>";

            await _emailService.EnviarEmailAsync(
                user.Email,
                "Confirmação de email",
                mensagem);
        }

    }
}
