using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.Services.GoogleCalendarService;
using ProjetoFinalCet105.API.UseCases.GoogleCalendarUsecases;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GoogleCalendarController : ControllerBase
    {
        private readonly ConectarGoogleCalendarUseCase _conectarUseCase;
        private readonly IGoogleCalendarService _googleCalendarService;
        private readonly CallbackGoogleCalendarUseCase _callbackUseCase;

        public GoogleCalendarController(
            ConectarGoogleCalendarUseCase conectarUseCase,
            CallbackGoogleCalendarUseCase callbackUseCase,
            IGoogleCalendarService googleCalendarService)
        {
            _conectarUseCase = conectarUseCase;
            _callbackUseCase = callbackUseCase;
            _googleCalendarService = googleCalendarService;
        }

        [HttpGet("conectar")]
        public IActionResult Conectar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var resultado = _conectarUseCase.Execute(userId);

            if (!resultado.Sucesso)
            {
                return BadRequest(resultado.Erro);
            }

            return Ok(new
            {
                authorizationUrl = resultado.Dados
            });
        }
        [AllowAnonymous]
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string? code,[FromQuery] string? state, [FromQuery] string? error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return BadRequest( $"A autorização do Google Calendar foi recusada: {error}");
            }

            if (string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(state))
            {
                return BadRequest( "Resposta de autorização Google inválida.");
            }

            var userId = _googleCalendarService.ObterUserIdDoState(state);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest( "Estado de autorização inválido.");
            }

            var resultado = await _callbackUseCase.ExecuteAsync( userId, code);

            if (!resultado.Sucesso)
            {
                return BadRequest(resultado.Erro);
            }

            return Ok(new
            {
                mensagem = "Google Calendar ligado com sucesso."
            });
        }
    }
}
