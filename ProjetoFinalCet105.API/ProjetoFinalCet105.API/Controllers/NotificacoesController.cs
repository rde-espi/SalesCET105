using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Notificacoes;
using System.Security.Claims;

namespace ProjetoFinalCet105.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificacoesController : BaseApiController
    {
        private readonly INotificacaoRepository
            _notificacaoRepository;
        private readonly MarcarNotificacaoLidaUseCase _marcarNotificacaoLidaUseCase;

        public NotificacoesController(INotificacaoRepository notificacaoRepository, MarcarNotificacaoLidaUseCase marcarNotificacaoLidaUseCase)
        {
            _notificacaoRepository = notificacaoRepository;
            _marcarNotificacaoLidaUseCase = marcarNotificacaoLidaUseCase;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificacaoDTO>>>GetMinhasNotificacoes()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var notificacoes =
                await _notificacaoRepository
                    .GetByUserId(userId)
                    .OrderByDescending(n => n.DataCriacao)
                    .Select(n => new NotificacaoDTO
                    {
                        Id = n.Id,
                        Titulo = n.Titulo,
                        Mensagem = n.Mensagem,
                        Lida = n.Lida,
                        DataCriacao = n.DataCriacao,
                        DataLeitura = n.DataLeitura
                    })
                    .ToListAsync();

            return Ok(notificacoes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NotificacaoDTO>> GetNotificacaoById(int id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var notificacao =
                await _notificacaoRepository
                    .GetByIdAndUserIdAsync(id, userId);

            if (notificacao == null)
            {
                return NotFound();
            }

            var dto = new NotificacaoDTO
            {
                Id = notificacao.Id,
                Titulo = notificacao.Titulo,
                Mensagem = notificacao.Mensagem,
                Lida = notificacao.Lida,
                DataCriacao = notificacao.DataCriacao,
                DataLeitura = notificacao.DataLeitura
            };

            return Ok(dto);
        }


        [HttpPut("{id:int}/lida")]
        public async Task<IActionResult> MarcarComoLida(int id)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resultado =
                await _marcarNotificacaoLidaUseCase
                    .ExecuteAsync(id, userId);

            if (!resultado.Sucesso)
            {
                return TratarErro(resultado);
            }

            return NoContent();
        }

        
    }
}
