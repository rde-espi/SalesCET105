using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager,IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO dto)
        {
            // Procura o utilizador pelo email
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
            {
                return Unauthorized("Email ou password inválidos.");
            }

            // Verifica a password através do ASP.NET Identity
            var passwordValida =
                await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!passwordValida)
            {
                return Unauthorized("Email ou password inválidos.");
            }

            // Impede login de utilizadores desativados
            if (!user.Ativo)
            {
                return Unauthorized("O utilizador encontra-se desativado.");
            }

            // Obtém as roles do utilizador
            var roles = await _userManager.GetRolesAsync(user);

            // Claims = informações que vão ficar dentro do token
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),

            new Claim(
                ClaimTypes.Name,
                user.NomeCompleto),

            new Claim(
                ClaimTypes.Email,
                user.Email!)
        };

            // Adicionamos cada Role às claims
            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role));
            }

            // Obtemos a chave definida no appsettings.json
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // Criamos o token JWT
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,

                // Por exemplo, login válido durante 8 horas
                expires: DateTime.UtcNow.AddHours(8),

                signingCredentials: credentials
            );

            var tokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            // Resposta enviada ao Mobile/Web
            var resposta = new LoginResponseDTO
            {
                Token = tokenString,
                UserId = user.Id,
                NomeCompleto = user.NomeCompleto,
                Email = user.Email!,
                Roles = roles
            };

            return Ok(resposta);
        }

        [Authorize]
        [HttpGet("teste-auth")]
        public IActionResult TesteAuth()
        {
            return Ok("Utilizador autenticado.");
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("teste-cliente")]
        public IActionResult TesteCliente()
        {
            return Ok("Cliente autenticado.");
        }
    }
}
