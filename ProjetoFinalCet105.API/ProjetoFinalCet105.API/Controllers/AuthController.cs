using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.UseCases.AuthUsecase;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly LoginUseCase _loginUseCase;
        private readonly AlterarPasswordUseCase _alterarPasswordUseCase;
        private readonly RecuperarPasswordUseCase _recuperarPasswordUseCase;
        private readonly ResetPasswordUseCase _resetPasswordUseCase;
        private readonly VerificarTwoFactorUseCase _verificarTwoFactorUseCase;
        private readonly GerirTwoFactorUseCase _gerirTwoFactorUseCase;
        private readonly ConfirmarEmailUseCase _confirmarEmailUseCase;
        private readonly ReenviarConfirmacaoEmailUseCase _reenviarConfirmacaoEmailUseCase;

        public AuthController(LoginUseCase loginUseCase, AlterarPasswordUseCase alterarPasswordUseCase,
            RecuperarPasswordUseCase recuperarPasswordUseCase, ResetPasswordUseCase resetPasswordUseCase, VerificarTwoFactorUseCase verificarTwoFactorUseCase,
            GerirTwoFactorUseCase gerirTwoFactorUseCase, ConfirmarEmailUseCase confirmarEmailUseCase, ReenviarConfirmacaoEmailUseCase reenviarConfirmacaoEmailUseCase)
        {
            _loginUseCase = loginUseCase;
            _alterarPasswordUseCase = alterarPasswordUseCase;
            _recuperarPasswordUseCase = recuperarPasswordUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
            _verificarTwoFactorUseCase = verificarTwoFactorUseCase;
            _gerirTwoFactorUseCase = gerirTwoFactorUseCase;
            _confirmarEmailUseCase = confirmarEmailUseCase;
            _reenviarConfirmacaoEmailUseCase = reenviarConfirmacaoEmailUseCase;
        }
        [Authorize]
        [HttpGet("debug-utilizador")]
        public IActionResult DebugUtilizador()
        {
            return Ok(new
            {
                Nome = User.Identity?.Name,
                Autenticado = User.Identity?.IsAuthenticated,
                EhCliente = User.IsInRole("Cliente"),
                EhFuncionario = User.IsInRole("Funcionario"),
                EhAdmin = User.IsInRole("Admin"),

                Claims = User.Claims.Select(c => new
                {
                    c.Type,
                    c.Value
                })
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginDTO dto)
        {
            var resultado =
                await _loginUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize]
        [HttpPost("alterar-password")]
        public async Task<IActionResult> AlterarPassword(AlterarPasswordDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _alterarPasswordUseCase.ExecuteAsync(
                    userId,
                    dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword(RecuperarPasswordDTO dto)
        {
            var resultado =
                await _recuperarPasswordUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return Ok(new
            {
                mensagem =
                    "Se existir uma conta associada a este email, " +
                    "serão enviadas instruções para recuperação da password."
            });
        }
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            var resultado =
                await _resetPasswordUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return Ok(new
            {
                mensagem = "Password alterada com sucesso."
            });
        }
        [AllowAnonymous]
        [HttpPost("verificar-2fa")]
        public async Task<ActionResult<LoginResponseDTO>> VerificarTwoFactor(VerificarTwoFactorDTO dto)
        {
            var resultado =
                await _verificarTwoFactorUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }


        [Authorize]
        [HttpPut("2fa")]
        public async Task<IActionResult> GerirTwoFactor(TwoFactorStatusDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _gerirTwoFactorUseCase.ExecuteAsync(
                    userId,
                    dto.Ativo);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return Ok(new
            {
                mensagem = dto.Ativo
                    ? "Autenticação de dois fatores ativada."
                    : "Autenticação de dois fatores desativada."
            });
        }

        [AllowAnonymous]
        [HttpPost("confirmar-email")]
        public async Task<IActionResult> ConfirmarEmail(ConfirmarEmailDTO dto)
        {
            var resultado =
                await _confirmarEmailUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return Ok(new
            {
                mensagem = "Email confirmado com sucesso."
            });
        }

        [AllowAnonymous]
        [HttpPost("reenviar-confirmacao-email")]
        public async Task<IActionResult> ReenviarConfirmacaoEmail(ReenviarConfirmacaoEmailDTO dto)
        {
            var resultado = await _reenviarConfirmacaoEmailUseCase.ExecuteAsync(dto);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return Ok(new
            {
                mensagem =
                    "Se existir uma conta por confirmar associada a este email, " +
                    "será enviada uma nova mensagem de confirmação."
            });
        }
    }
}
