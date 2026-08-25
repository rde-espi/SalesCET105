using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Conversas;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversasController : BaseApiController
    {
        private readonly CriarConversaUseCase _criarConversaUseCase;
        private readonly EnviarMensagemUseCase _enviarMensagemUseCase;
        private readonly GetMinhasConversasUseCase _getMinhasConversasUseCase;
        private readonly GetConversaByIdUseCase _getConversaByIdUseCase;
        private readonly MarcarMensagensComoLidasUseCase _marcarMensagensComoLidasUseCase;
        private readonly IMensagemRepository _mensagemRepository;

        public ConversasController(
            CriarConversaUseCase criarConversaUseCase,
            EnviarMensagemUseCase enviarMensagemUseCase,
            GetMinhasConversasUseCase getMinhasConversasUseCase,
            GetConversaByIdUseCase getConversaByIdUseCase,
            MarcarMensagensComoLidasUseCase marcarMensagensComoLidasUseCase,
            IMensagemRepository mensagemRepository)
        {
            _criarConversaUseCase = criarConversaUseCase;
            _enviarMensagemUseCase = enviarMensagemUseCase;
            _getMinhasConversasUseCase = getMinhasConversasUseCase;
            _getConversaByIdUseCase = getConversaByIdUseCase;
            _marcarMensagensComoLidasUseCase = marcarMensagensComoLidasUseCase;
            _mensagemRepository = mensagemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<ConversaDTO>>>GetMinhasConversas()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var isAdmin = User.IsInRole("Admin");

            var resultado = await _getMinhasConversasUseCase.ExecuteAsync(userId,isAdmin);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ConversaDTO>>GetConversaById(int id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _getConversaByIdUseCase.ExecuteAsync(id, userId,User.IsInRole("Admin"));

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Roles = "Cliente,Funcionario")]
        [HttpGet("contador-nao-lidas")]
        public async Task<ActionResult<int>> GetContadorNaoLidas()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var total = await _mensagemRepository.CountNaoLidasByUserIdAsync(userId);

            return Ok(total);
        }

        [Authorize(Roles = "Cliente,Funcionario")]
        [HttpPost]
        public async Task<ActionResult<ConversaDTO>>CriarConversa(NovaConversaDTO dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _criarConversaUseCase
                    .ExecuteAsync(userId, dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Roles = "Cliente,Funcionario")]
        [HttpPost("{id:int}/mensagens")]
        public async Task<ActionResult<MensagemDTO>>EnviarMensagem(int id, EnviarMensagemDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _enviarMensagemUseCase.ExecuteAsync(id, userId, dto);

            if (!resultado.Sucesso)
            {
                return TratarErroComDados(resultado);
            }

            return Ok(resultado.Dados);
        }

        [Authorize(Roles = "Cliente,Funcionario")]
        [HttpPut("{id:int}/mensagens/lidas")]
        public async Task<IActionResult>MarcarMensagensComoLidas(int id)
        {
            var userId =User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado = await _marcarMensagensComoLidasUseCase.ExecuteAsync(id,userId);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }
    }
}
